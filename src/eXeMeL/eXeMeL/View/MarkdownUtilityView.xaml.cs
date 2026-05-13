using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using CommunityToolkit.Mvvm.Messaging;
using eXeMeL.Messages;
using eXeMeL.Model;
using eXeMeL.ViewModel.MarkdownUtility;
using Microsoft.Web.WebView2.Core;

namespace eXeMeL.View
{
  public partial class MarkdownUtilityView : UserControl, INotifyPropertyChanged
  {
    private const string PreviewVirtualHost = "preview.example";
    private const string AppDataFolderName = "eXeMeL2";

    private static readonly JsonSerializerOptions JsonOpts = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    private readonly MarkdownPreviewRenderer _renderer = new();

    private bool _webViewInitStarted;
    private bool _webViewReady;          // JS has signalled 'ready'

    // Pending state — applied as soon as the WebView is ready.
    private string _pendingContent;
    private string _pendingTheme;
    private int? _pendingRevealLine;

    private MarkdownUtilityViewModel _attachedViewModel;

    public MarkdownUtilityView()
    {
      InitializeComponent();

      this.DataContextChanged += OnDataContextChanged;
      this.Loaded += OnLoaded;
      this.Unloaded += OnUnloaded;
    }

    public MarkdownUtilityViewModel ViewModel => this.DataContext as MarkdownUtilityViewModel;
    public Settings Settings => this.ViewModel?.Settings;
    public event PropertyChangedEventHandler PropertyChanged;

    protected virtual void OnPropertyChanged(string propertyName) =>
      this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    // ---------- Lifecycle ----------

    private async void OnLoaded(object sender, RoutedEventArgs e)
    {
      // Resubscribe (handles unload/reload cycles, e.g. tab switching).
      SubscribeMessages();
      await BeginInitializationAsync();
    }

    /// <summary>
    /// Kicks off WebView2 initialization on a background-friendly path so callers can warm the
    /// preview before it becomes visible (e.g. as soon as content is detected as Markdown).
    /// Safe to call repeatedly — the underlying init only runs once.
    /// </summary>
    public async Task BeginInitializationAsync()
    {
      if (_webViewInitStarted) return;
      _webViewInitStarted = true;

      if (!IsWebView2RuntimeInstalled())
      {
        ShowRuntimeMissing();
        return;
      }

      try
      {
        await InitializeWebViewAsync();
      }
      catch (Exception)
      {
        ShowRuntimeMissing();
      }
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
      UnsubscribeMessages();
    }

    private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
      if (_attachedViewModel != null)
        _attachedViewModel.PropertyChanged -= OnViewModelPropertyChanged;

      _attachedViewModel = this.ViewModel;

      if (_attachedViewModel != null)
        _attachedViewModel.PropertyChanged += OnViewModelPropertyChanged;

      OnPropertyChanged(nameof(ViewModel));
      OnPropertyChanged(nameof(Settings));

      // Push current content as soon as DataContext arrives.
      PushContent(_attachedViewModel?.DocumentText);
      PushTheme();
    }

    private void OnViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
    {
      if (e.PropertyName == nameof(MarkdownUtilityViewModel.DocumentText))
        PushContent(_attachedViewModel?.DocumentText);
    }

    // ---------- WebView2 init ----------

    private static bool IsWebView2RuntimeInstalled()
    {
      try
      {
        var version = CoreWebView2Environment.GetAvailableBrowserVersionString(null);
        return !string.IsNullOrEmpty(version);
      }
      catch
      {
        return false;
      }
    }

    private void ShowRuntimeMissing()
    {
      this.RuntimeMissingPanel.Visibility = Visibility.Visible;
      this.PreviewWebView.Visibility = Visibility.Collapsed;
    }

    private async Task InitializeWebViewAsync()
    {
      var userDataFolder = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        AppDataFolderName, "WebView2");

      var env = await CoreWebView2Environment.CreateAsync(null, userDataFolder, null);
      await this.PreviewWebView.EnsureCoreWebView2Async(env);

      // Make the WebView2 itself transparent so the theme/glass chrome behind it shows through
      // (the default is opaque white, which made dark-theme text unreadable against the white
      // backdrop bleeding through transparent CSS).
      this.PreviewWebView.DefaultBackgroundColor = System.Drawing.Color.Transparent;

      var core = this.PreviewWebView.CoreWebView2;
      core.Settings.AreDevToolsEnabled = Debugger.IsAttached;
      core.Settings.AreDefaultContextMenusEnabled = Debugger.IsAttached;
      core.Settings.IsStatusBarEnabled = false;
      core.Settings.IsGeneralAutofillEnabled = false;
      core.Settings.IsPasswordAutosaveEnabled = false;
      core.WebMessageReceived += OnWebMessageReceived;

      var assetsPath = Path.Combine(AppContext.BaseDirectory, "Assets", "MarkdownPreview");
      core.SetVirtualHostNameToFolderMapping(
        PreviewVirtualHost, assetsPath,
        CoreWebView2HostResourceAccessKind.DenyCors);

      this.PreviewWebView.Source = new Uri($"https://{PreviewVirtualHost}/preview.html");
    }

    // ---------- Inbound from page (preview.js) ----------

    private void OnWebMessageReceived(object sender, CoreWebView2WebMessageReceivedEventArgs args)
    {
      string json = null;
      try { json = args.WebMessageAsJson; }
      catch { return; }
      if (string.IsNullOrEmpty(json)) return;

      try
      {
        using var doc = JsonDocument.Parse(json);
        if (!doc.RootElement.TryGetProperty("type", out var typeProp)) return;
        var type = typeProp.GetString();

        switch (type)
        {
          case "ready":
            _webViewReady = true;
            FlushPending();
            break;

          case "scroll":
            if (doc.RootElement.TryGetProperty("line", out var lineProp) &&
                lineProp.TryGetInt32(out var line) && line > 0)
            {
              WeakReferenceMessenger.Default.Send(new PreviewScrolledToLineMessage(line));
            }
            break;

          case "openExternal":
            if (doc.RootElement.TryGetProperty("url", out var urlProp))
              OpenExternal(urlProp.GetString());
            break;
        }
      }
      catch (JsonException)
      {
        // Malformed message — ignore.
      }
    }

    private static void OpenExternal(string url)
    {
      if (string.IsNullOrWhiteSpace(url)) return;
      if (!Uri.TryCreate(url, UriKind.Absolute, out var uri)) return;
      // Only allow http(s) and mailto to avoid launching arbitrary protocol handlers.
      if (uri.Scheme != Uri.UriSchemeHttp &&
          uri.Scheme != Uri.UriSchemeHttps &&
          uri.Scheme != Uri.UriSchemeMailto) return;

      try
      {
        Process.Start(new ProcessStartInfo(uri.AbsoluteUri) { UseShellExecute = true });
      }
      catch
      {
        // Swallow — failing to launch the OS handler shouldn't crash the app.
      }
    }

    // ---------- Outbound to page ----------

    private void PushContent(string markdown)
    {
      var html = _renderer.ToHtml(markdown);
      if (!_webViewReady) { _pendingContent = html; return; }
      PostMessage(new { type = "setContent", html });
    }

    private void PushTheme()
    {
      var theme = ResolveCssTheme();
      if (!_webViewReady) { _pendingTheme = theme; return; }
      PostMessage(new { type = "setTheme", theme });
    }

    private void PushRevealLine(int line)
    {
      if (!_webViewReady) { _pendingRevealLine = line; return; }
      PostMessage(new { type = "revealLine", line });
    }

    private void FlushPending()
    {
      if (_pendingTheme == null) _pendingTheme = ResolveCssTheme();
      PostMessage(new { type = "setTheme", theme = _pendingTheme });
      _pendingTheme = null;

      if (_pendingContent != null)
      {
        PostMessage(new { type = "setContent", html = _pendingContent });
        _pendingContent = null;
      }
      else
      {
        // No queued content but DataContext may have arrived after init — push current value.
        var current = _attachedViewModel?.DocumentText;
        if (!string.IsNullOrEmpty(current))
          PostMessage(new { type = "setContent", html = _renderer.ToHtml(current) });
      }

      if (_pendingRevealLine.HasValue)
      {
        PostMessage(new { type = "revealLine", line = _pendingRevealLine.Value });
        _pendingRevealLine = null;
      }
    }

    private void PostMessage(object payload)
    {
      var core = this.PreviewWebView?.CoreWebView2;
      if (core == null) return;
      try
      {
        var json = JsonSerializer.Serialize(payload, JsonOpts);
        core.PostWebMessageAsJson(json);
      }
      catch
      {
        // Swallow — a single failed post shouldn't break the preview.
      }
    }

    // ---------- Theme resolution ----------

    private string ResolveCssTheme()
    {
      var settings = this.Settings;
      if (settings == null) return "light";

      switch (settings.ApplicationTheme)
      {
        case ApplicationTheme.Light:
          return "light";
        case ApplicationTheme.Dark:
          return "dark";
        case ApplicationTheme.SolarizedDark:
          return "solarized-dark";
        case ApplicationTheme.Glass:
        case ApplicationTheme.Tinted:
          return ApplicationThemeExtensions.IsLightColor(settings.ChromeTintColor) ? "light" : "dark";
        default:
          return "light";
      }
    }

    // ---------- Messenger subscriptions ----------

    private void SubscribeMessages()
    {
      var m = WeakReferenceMessenger.Default;
      m.Register<MarkdownUtilityView, ApplicationThemeUpdatedMessage>(this, (r, _) =>
        r.Dispatcher.BeginInvoke(new Action(r.PushTheme)));
      m.Register<MarkdownUtilityView, EditorScrolledToLineMessage>(this, (r, msg) =>
        r.Dispatcher.BeginInvoke(new Action(() => r.PushRevealLine(msg.Line))));
    }

    private void UnsubscribeMessages()
    {
      WeakReferenceMessenger.Default.UnregisterAll(this);
    }
  }
}
