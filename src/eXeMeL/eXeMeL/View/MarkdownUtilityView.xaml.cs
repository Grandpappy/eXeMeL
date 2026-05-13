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
    private (int line, double scrollHint)? _pendingHighlight;
    private int? _pendingScrollLine;

    private MarkdownUtilityViewModel _attachedViewModel;
    private Settings _attachedSettings;

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
      if (_attachedSettings != null)
        _attachedSettings.PropertyChanged -= OnSettingsPropertyChanged;

      _attachedViewModel = this.ViewModel;
      _attachedSettings = _attachedViewModel?.Settings;

      if (_attachedViewModel != null)
        _attachedViewModel.PropertyChanged += OnViewModelPropertyChanged;
      if (_attachedSettings != null)
        _attachedSettings.PropertyChanged += OnSettingsPropertyChanged;

      OnPropertyChanged(nameof(ViewModel));
      OnPropertyChanged(nameof(Settings));

      // Push current content as soon as DataContext arrives.
      PushContent(_attachedViewModel?.DocumentText);
      PushTheme();
      ApplyZoomFactor();
    }

    private void OnViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
    {
      if (e.PropertyName == nameof(MarkdownUtilityViewModel.DocumentText))
        PushContent(_attachedViewModel?.DocumentText);
    }

    private void OnSettingsPropertyChanged(object sender, PropertyChangedEventArgs e)
    {
      if (e.PropertyName == nameof(Settings.AppScale))
        ApplyZoomFactor();
    }

    /// <summary>Mirrors the app's zoom level into the preview's WebView2 so the
    /// preview text scales together with the rest of the UI (Ctrl + / -).</summary>
    private void ApplyZoomFactor()
    {
      if (this.PreviewWebView?.CoreWebView2 == null) return;
      var scale = _attachedSettings?.AppScale ?? 1.0;
      try { this.PreviewWebView.ZoomFactor = scale; } catch { /* not yet ready */ }
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

      // Apply the current app zoom now that CoreWebView2 exists.
      ApplyZoomFactor();
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
      var accentRgba = ResolveAccentRgba(0.30);
      if (!_webViewReady) { _pendingTheme = theme; return; }
      PostMessage(new { type = "setTheme", theme, accentRgba });
    }

    /// <summary>Converts the user's accent color (e.g. #D4AA00) to an rgba() string
    /// at the given alpha, for use as the line-highlight background in the preview.</summary>
    private string ResolveAccentRgba(double alpha)
    {
      var hex = this.Settings?.AccentColor;
      if (string.IsNullOrWhiteSpace(hex)) hex = "#D4AA00";
      try
      {
        var color = (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString(hex);
        return $"rgba({color.R}, {color.G}, {color.B}, {alpha.ToString("0.##", System.Globalization.CultureInfo.InvariantCulture)})";
      }
      catch
      {
        return $"rgba(212, 170, 0, {alpha.ToString("0.##", System.Globalization.CultureInfo.InvariantCulture)})";
      }
    }

    private void PushHighlightLine(int line, double scrollHint)
    {
      if (!_webViewReady) { _pendingHighlight = (line, scrollHint); return; }
      PostMessage(new { type = "highlightLine", line, scrollHint });
    }

    private void PushScrollToLine(int line)
    {
      if (!_webViewReady) { _pendingScrollLine = line; return; }
      PostMessage(new { type = "scrollToLine", line });
    }

    private void FlushPending()
    {
      if (_pendingTheme == null) _pendingTheme = ResolveCssTheme();
      PostMessage(new { type = "setTheme", theme = _pendingTheme, accentRgba = ResolveAccentRgba(0.30) });
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

      if (_pendingHighlight.HasValue)
      {
        var ph = _pendingHighlight.Value;
        PostMessage(new { type = "highlightLine", line = ph.line, scrollHint = ph.scrollHint });
        _pendingHighlight = null;
      }
      if (_pendingScrollLine.HasValue)
      {
        PostMessage(new { type = "scrollToLine", line = _pendingScrollLine.Value });
        _pendingScrollLine = null;
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
      m.Register<MarkdownUtilityView, EditorCaretChangedMessage>(this, (r, msg) =>
        r.Dispatcher.BeginInvoke(new Action(() => r.PushHighlightLine(msg.Line, msg.ScrollHint))));
      m.Register<MarkdownUtilityView, EditorScrolledToLineMessage>(this, (r, msg) =>
        r.Dispatcher.BeginInvoke(new Action(() => r.PushScrollToLine(msg.Line))));
    }

    private void UnsubscribeMessages()
    {
      WeakReferenceMessenger.Default.UnregisterAll(this);
    }
  }
}
