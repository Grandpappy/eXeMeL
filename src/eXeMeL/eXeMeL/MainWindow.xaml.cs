using eXeMeL.Messages;
using eXeMeL.ViewModel;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Folding;
using eXeMeL.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.ComponentModel;
using eXeMeL.View.ChangeLog;
using eXeMeL.Model;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using System.Threading.Tasks;
using Wpf.Ui.Controls;

namespace eXeMeL
{
  public partial class MainWindow : FluentWindow
  {
    private FoldingManager FoldingManager { get; set; }
    private XmlFoldingStrategy XmlFoldingStrategy { get; set; }
    private JsonFoldingStrategy JsonFoldingStrategy { get; set; }
    private YamlFoldingStrategy YamlFoldingStrategy { get; set; }
    private DocumentContentType _currentContentType = DocumentContentType.Xml;
    private MarkdownFormattingTransformer _markdownTransformer;
    public MainViewModel ViewModel => this.DataContext as MainViewModel;
    private PropertyObserver<TextDocument> TextDocumentObserver { get; set; }
    private bool IgnoreNextTextChange { get; set; }

    public ICommand FocusOnFindControlCommand { get; private set; }
    public ICommand ResetFocusCommand { get; private set; }

    public ICommand FoldLevelCommand { get; private set; }
    public ICommand UnFoldLevelCommand { get; private set; }

    private bool _isSettingsOpen;
    private bool _isPreviewPinned;
    private System.Windows.Threading.DispatcherTimer _previewDebounce;


    public MainWindow()
    {
      UIThread.Initialize(this.Dispatcher, false);

      this.Closing += MainWindow_Closing;
      this.Loaded += MainWindow_Loaded;
      this.DataContextChanged += MainWindow_DataContextChanged;
      this.StateChanged += MainWindow_StateChanged;
      this.AllowDrop = true;
      this.Drop += MainWindow_Drop;
      this.SizeChanged += (s, e) => UpdateFindBarMode();
      this.FocusOnFindControlCommand = new RelayCommand(FocusOnFindControlCommand_Executed);
      this.ResetFocusCommand = new RelayCommand(ResetFocusCommand_Executed);
      this.FoldLevelCommand = new RelayCommand<string>(l => FoldSections(l, true));
      this.UnFoldLevelCommand = new RelayCommand<string>(l => FoldSections(l, false));

      InitializeComponent();

      // Set initial backdrop type right after InitializeComponent
      // (before Loaded, before our chrome override — FluentWindow hasn't
      // configured chrome yet so this won't conflict)
      var initialTheme = this.ViewModel?.Settings?.ApplicationTheme ?? ApplicationTheme.Dark;
      this.WindowBackdropType = initialTheme.IsGlassTheme()
        ? Wpf.Ui.Controls.WindowBackdropType.Acrylic
        : Wpf.Ui.Controls.WindowBackdropType.Mica;

      // Apply chrome override after window is loaded
      this.Loaded += (s, e) =>
      {
        ApplyWindowChrome();
        UpdateCurrentLineHighlight();
        CheckForUpdatesOnStartup();
        UpdateFindBarMode();
      };

      this.AvalonEditor.PreviewKeyDown += AvalonEditor_PreviewKeyDown;
      this.AvalonEditor.TextArea.DocumentChanged += TextArea_DocumentChanged;
      this.AvalonEditor.TextArea.TextView.LineTransformers.Add(new AllSelectionColorizer(this.AvalonEditor, this.ViewModel.Settings));
      this.AvalonEditor.TextArea.SelectionChanged += (sender, args) => this.AvalonEditor.TextArea.TextView.Redraw();
      this.AvalonEditor.TextArea.Caret.PositionChanged += AvalonEditor_CaretPositionChanged;
      this.AvalonEditor.TextChanged += AvalonEditor_TextChanged;

      this.FoldingManager = FoldingManager.Install(this.AvalonEditor.TextArea);
      this.XmlFoldingStrategy = new XmlFoldingStrategy();
      this.JsonFoldingStrategy = new JsonFoldingStrategy();
      this.YamlFoldingStrategy = new YamlFoldingStrategy();

      // Bracket highlighting (always on)
      var bracketRenderer = new BracketHighlightRenderer(this.AvalonEditor);
      this.AvalonEditor.TextArea.TextView.BackgroundRenderers.Add(bracketRenderer);

      // Note: AvalonEdit's built-in SearchPanel removed — regex support
      // added to our custom Find bar instead

      this.IgnoreNextTextChange = false;

      RestoreWindowPosition();
    }

    private void AvalonEditor_TextChanged(object sender, EventArgs e)
    {
      if (!this.IgnoreNextTextChange)
      {
        this.ViewModel.Editor.ClearSnapshotsAfterDocument(this.AvalonEditor.TextArea.Document);
      }
      else
      {
        this.IgnoreNextTextChange = false;
      }
    }



    private void MainWindow_Drop(object sender, DragEventArgs e)
    {
      if (e.Data.GetDataPresent(DataFormats.FileDrop, true))
      {
        string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
        this.ViewModel.Editor.OpenFileAsync(files[0]);
      }
    }



    private void MainWindow_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
      this.TextDocumentObserver =
        new PropertyObserver<TextDocument>(this.ViewModel.Editor.Document)
          .RegisterHandler(x => x.Text, HandleChangedDocumentText);

      WeakReferenceMessenger.Default.Register<SelectTextInEditorMessage>(this, (r, m) => HandleSelectTextInEditorMessage(m));
      WeakReferenceMessenger.Default.Register<UnselectTextInEditorMessage>(this, (r, m) => HandleUnselectTextInEditorMessage(m));
      WeakReferenceMessenger.Default.Register<DocumentTextReplacedMessage>(this, (r, m) => HandleDocumentTextReplacedMessage(m));
      WeakReferenceMessenger.Default.Register<ApplicationThemeUpdatedMessage>(this, (r, m) => HandleApplicationThemeUpdatedMessage(m));
      WeakReferenceMessenger.Default.Register<SetKeyboardFocusToEditor>(this, (r, m) => HandleSetKeyboardFocusToEditorMessage(m));
      WeakReferenceMessenger.Default.Register<EditorModeChangedMessage>(this, (r, m) => HandleEditorModeChangedMessage(m));
      WeakReferenceMessenger.Default.Register<ContentTypeChangedMessage>(this, (r, m) => HandleContentTypeChanged(m));

      this.ViewModel.Editor.RefreshComplete += Editor_RefreshComplete;
      this.ViewModel.Editor.PropertyChanging += Editor_PropertyChanging;
      this.ViewModel.Editor.PropertyChanged += Editor_PropertyChanged;

      // Observe editor settings changes
      this.ViewModel.Settings.PropertyChanged += (s, e) =>
      {
        if (e.PropertyName == "HighlightCurrentLine")
          UpdateCurrentLineHighlight();
        if (e.PropertyName == nameof(Settings.AppScale))
          UpdateFindBarMode();
      };

      HandleChangedDocumentText(this.ViewModel.Editor.Document);
    }



    private void TextArea_DocumentChanged(object sender, EventArgs e)
    {
      this.FoldingManager = FoldingManager.Install(this.AvalonEditor.TextArea);
      UpdateDocumentFoldings();
    }



    private void Editor_PropertyChanging(object sender, PropertyChangingEventArgs e)
    {
      if (e.PropertyName == "Document")
      {
        FoldingManager.Uninstall(this.FoldingManager);
        this.FoldingManager = null;
        this.IgnoreNextTextChange = true;
      };
    }



    private void HandleSelectTextInEditorMessage(SelectTextInEditorMessage message)
    {
      this.AvalonEditor.Select(message.Index, message.Length);

      var editorLocation = this.AvalonEditor.Document.GetLocation(message.Index);
      this.AvalonEditor.ScrollTo(editorLocation.Line, editorLocation.Column);
    }



    private void HandleUnselectTextInEditorMessage(UnselectTextInEditorMessage message)
    {
      this.AvalonEditor.SelectionLength = 0;
    }



    private void HandleDocumentTextReplacedMessage(DocumentTextReplacedMessage obj)
    {
      this.AvalonEditor.ScrollToHome();
      this.AvalonEditor.CaretOffset = 0;
      UpdateWindowTitle();
    }



    private void HandleChangedDocumentText(TextDocument document)
    {
      UpdateDocumentFoldings();
    }



    private void UpdateDocumentFoldings()
    {
      if (this.FoldingManager == null) return;

      switch (_currentContentType)
      {
        case DocumentContentType.Json:
          this.JsonFoldingStrategy?.UpdateFoldings(this.FoldingManager, this.AvalonEditor.Document);
          break;
        case DocumentContentType.Yaml:
          this.YamlFoldingStrategy?.UpdateFoldings(this.FoldingManager, this.AvalonEditor.Document);
          break;
        case DocumentContentType.Text:
        case DocumentContentType.Markdown:
          // No folding for plain text or markdown
          this.FoldingManager.UpdateFoldings(new List<ICSharpCode.AvalonEdit.Folding.NewFolding>(), -1);
          break;
        default:
          this.XmlFoldingStrategy?.UpdateFoldings(this.FoldingManager, this.AvalonEditor.Document);
          break;
      }
    }



    private void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
      if (StartupOptions.InitialFilePath == null)
      {
        this.ViewModel.Editor.RefreshCommand.Execute(null);
      }
      else
      {
        this.ViewModel.Editor.OpenFileAsync(StartupOptions.InitialFilePath);
      }


      if (!ApplicationVersionControl.CurrentVersionIsDifferentFromLastRunVersion(this.ViewModel.Settings))
      {
        ShowChangeLog();
        ApplicationVersionControl.WriteCurrentVersion(this.ViewModel.Settings);
        SettingsIO.SaveSettings(this.ViewModel.Settings);
      }
    }



    private void Editor_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
      if (e.PropertyName == "IsContentFromFile" || e.PropertyName == "FilePath")
      {
        UpdateEditorTabLabel();
      }
    }

    private void UpdateEditorTabLabel()
    {
      if (this.EditorTabLabel == null || this.ViewModel?.Editor == null) return;

      if (this.ViewModel.Editor.IsContentFromFile && !string.IsNullOrEmpty(this.ViewModel.Editor.FilePath))
      {
        this.EditorTabLabel.Text = System.IO.Path.GetFileName(this.ViewModel.Editor.FilePath);
      }
      else
      {
        this.EditorTabLabel.Text = "Clipboard Content";
      }

      UpdateWindowTitle();
    }

    private void UpdateWindowTitle()
    {
      if (this.ViewModel?.Editor == null) return;

      var editor = this.ViewModel.Editor;
      if (editor.IsContentFromFile && !string.IsNullOrEmpty(editor.FilePath))
      {
        this.Title = $"{System.IO.Path.GetFileName(editor.FilePath)} — eXeMeL";
      }
      else
      {
        var preview = GetContentPreview(editor.Document?.Text, _currentContentType);
        if (!string.IsNullOrEmpty(preview))
          this.Title = $"{preview} — eXeMeL";
        else
          this.Title = "eXeMeL";
      }
    }

    private static string GetContentPreview(string text, DocumentContentType contentType)
    {
      if (string.IsNullOrWhiteSpace(text))
        return null;

      try
      {
        if (contentType == DocumentContentType.Xml)
        {
          return GetXmlPreview(text);
        }
        else
        {
          return GetJsonPreview(text);
        }
      }
      catch
      {
        return null;
      }
    }

    private static string GetXmlPreview(string xml)
    {
      try
      {
        var doc = System.Xml.Linq.XElement.Parse(xml);
        // Skip AddedRoot wrapper if present
        var root = doc.Name.LocalName == "AddedRoot" && doc.Elements().Any()
          ? doc.Elements().First()
          : doc;
        return root.Name.LocalName;
      }
      catch
      {
        // Try to find the first element name with regex as fallback
        var match = System.Text.RegularExpressions.Regex.Match(xml, @"<(\w+)[\s>/]");
        if (match.Success)
        {
          var name = match.Groups[1].Value;
          return name == "AddedRoot" ? null : name;
        }
        return null;
      }
    }

    private static string GetJsonPreview(string json)
    {
      try
      {
        using var doc = System.Text.Json.JsonDocument.Parse(json);
        var root = doc.RootElement;

        if (root.ValueKind == System.Text.Json.JsonValueKind.Object)
        {
          // Find the first string property to use as a preview
          foreach (var prop in root.EnumerateObject())
          {
            if (prop.Value.ValueKind == System.Text.Json.JsonValueKind.String)
            {
              var val = prop.Value.GetString();
              if (val.Length > 40) val = val[..40] + "...";
              return $"{prop.Name}: {val}";
            }
            // If first property is an object/array, show the key name
            if (prop.Value.ValueKind == System.Text.Json.JsonValueKind.Object ||
                prop.Value.ValueKind == System.Text.Json.JsonValueKind.Array)
            {
              return prop.Name;
            }
          }
        }
        else if (root.ValueKind == System.Text.Json.JsonValueKind.Array)
        {
          return $"{root.GetArrayLength()} items";
        }
      }
      catch { }
      return null;
    }

    private void Editor_RefreshComplete(object sender, EventArgs e)
    {
      this.AvalonEditor.CaretOffset = 0;
      this.AvalonEditor.Focus();
    }



    private void AvalonEditor_PreviewKeyDown(object sender, KeyEventArgs e)
    {
    }



    private void AvalonEditor_CaretPositionChanged(object sender, EventArgs e)
    {
      this.ViewModel.Editor.CaretPosition = this.AvalonEditor.TextArea.Caret.Position;
    }



    private void AvalonEditor_TextAreaMouseRightButtonDown(object sender, MouseButtonEventArgs e)
    {
      var position = this.AvalonEditor.GetPositionFromPoint(e.GetPosition(this.AvalonEditor));
      if (position.HasValue)
      {
        this.AvalonEditor.TextArea.Caret.Position = position.Value;
      }
    }



    private void MainWindow_Closing(object sender, CancelEventArgs e)
    {
      SaveWindowPosition();
      WeakReferenceMessenger.Default.Send<ApplicationClosingMessage>(new ApplicationClosingMessage());
    }



    private void OpenSettingsButton_Click(object sender, RoutedEventArgs e)
    {
      ToggleSettingsPanel();
    }



    private void CloseSettingsButton_Click(object sender, RoutedEventArgs e)
    {
      CloseSettingsPanel();
    }



    private void ToggleSettingsPanel()
    {
      if (_isSettingsOpen)
      {
        CloseSettingsPanel();
      }
      else
      {
        OpenSettingsPanel();
      }
    }



    private void OpenSettingsPanel()
    {
      _isSettingsOpen = true;
      SettingsPanelColumn.Width = new GridLength(370);
    }



    private void CloseSettingsPanel()
    {
      _isSettingsOpen = false;
      SettingsPanelColumn.Width = new GridLength(0);
    }



    private void FocusOnFindControlCommand_Executed()
    {
      if (!string.IsNullOrEmpty(this.AvalonEditor.SelectedText))
        WeakReferenceMessenger.Default.Send<SetSearchTextMessage>(new SetSearchTextMessage(this.AvalonEditor.SelectedText));

      if (this.FindBarInHeader.Visibility == Visibility.Visible)
      {
        this.FindBarInHeader.Focus();
      }
      else
      {
        ShowFindOverlay();
      }
    }



    private void ResetFocusCommand_Executed()
    {
      HideFindOverlay();
      this.AvalonEditor.Focus();
    }



    private void FindButton_Click(object sender, RoutedEventArgs e)
    {
      if (this.FindOverlayPanel.Visibility == Visibility.Visible)
        HideFindOverlay();
      else
        FocusOnFindControlCommand_Executed();
    }



    private void ShowFindOverlay()
    {
      var outerGrid = (System.Windows.UIElement)this.Content;
      var pt = this.FindHeaderSearchButton.TranslatePoint(new System.Windows.Point(0, 0), outerGrid);
      this.FindOverlayPanel.Margin = new Thickness(pt.X, 50, 0, 0);
      this.FindOverlayPanel.Visibility = Visibility.Visible;
      this.FindBarInOverlay.Focus();
    }



    private void HideFindOverlay()
    {
      this.FindOverlayPanel.Visibility = Visibility.Collapsed;
    }



    private void UpdateFindBarMode()
    {
      if (this.ViewModel?.Settings == null || this.ActualWidth <= 0) return;

      // Find bar needs ~900 logical px: left (280) + find bar (440) + chrome (184)
      var logicalWidth = this.ActualWidth / this.ViewModel.Settings.AppScale;
      bool findBarFits = logicalWidth >= 900;

      this.FindBarInHeader.Visibility = findBarFits ? Visibility.Visible : Visibility.Collapsed;
      this.FindHeaderSearchButton.Visibility = findBarFits ? Visibility.Collapsed : Visibility.Visible;

      if (findBarFits && this.FindOverlayPanel.Visibility == Visibility.Visible)
        HideFindOverlay();
    }



    private void ZoomLevelButton_Click(object sender, RoutedEventArgs e)
    {
      var menu = new System.Windows.Controls.ContextMenu();
      foreach (var level in this.ViewModel.ZoomLevels)
      {
        var item = new System.Windows.Controls.MenuItem
        {
          Header = $"{(int)Math.Round(level * 100)}%",
          IsChecked = Math.Abs(this.ViewModel.Settings.AppScale - level) < 0.001
        };
        var capturedLevel = level;
        item.Click += (s, args) => this.ViewModel.Settings.AppScale = capturedLevel;
        menu.Items.Add(item);
      }
      menu.PlacementTarget = sender as System.Windows.UIElement;
      menu.Placement = System.Windows.Controls.Primitives.PlacementMode.Top;
      menu.IsOpen = true;
    }



    private FoldingSection[] FoldSectionCache { get; set; }
    private List<FoldingSection>[] FoldingLevels { get; set; }



    private void FoldSections(string level, bool fold)
    {
      if (string.IsNullOrEmpty(level) || level.Length > 1 || "1234567890-".IndexOf(level) < 0) return;

      if (this.FoldSectionCache == null || this.FoldSectionCache.Any(f => !this.FoldingManager.AllFoldings.Contains(f)))
      {
        this.FoldSectionCache = this.FoldingManager.AllFoldings.ToArray();
        this.FoldingLevels = new List<FoldingSection>[10];

        for (var i = 0; i < 10; i++)
          this.FoldingLevels[i] = new List<FoldingSection>();

        // rebuild
        var stack = new Stack<FoldingSection>();
        foreach (var foldSection in this.FoldingManager.AllFoldings)
        {
          if (stack.Any())
            while (foldSection.StartOffset > stack.Peek().EndOffset) stack.Pop();
          if (stack.Count < 10)
            this.FoldingLevels[stack.Count].Add(foldSection);
          stack.Push(foldSection);
        }
      }

      var itemsToFold = level != "-" ? this.FoldingLevels[level[0] - '0'] : this.FoldingManager.AllFoldings.ToList();
      itemsToFold.ForEach(f => f.IsFolded = fold);
    }



    #region Window Chrome Button Handlers

    private void MinimizeButton_Click(object sender, RoutedEventArgs e)
    {
      this.WindowState = WindowState.Minimized;
    }

    private void MaximizeRestoreButton_Click(object sender, RoutedEventArgs e)
    {
      this.WindowState = this.WindowState == WindowState.Maximized
        ? WindowState.Normal
        : WindowState.Maximized;
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
      this.Close();
    }

    private void MainWindow_StateChanged(object sender, EventArgs e)
    {
      if (this.MaximizeRestoreGlyph != null)
      {
        this.MaximizeRestoreGlyph.Text = this.WindowState == WindowState.Maximized ? "\uE923" : "\uE739";
      }
    }

    #endregion



    private void EditorTabHeader_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
      if (this.ViewModel.EditorMode != EditorMode.Editor)
        this.ViewModel.ToggleEditorModeCommand.Execute(null);
    }

    private void XPathTabHeader_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
      if (this.ViewModel.EditorMode != EditorMode.XmlUtility)
        this.ViewModel.ToggleEditorModeCommand.Execute(null);
    }

    private void JsonTreeTabHeader_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
      if (this.ViewModel.EditorMode != EditorMode.XmlUtility)
        this.ViewModel.ToggleEditorModeCommand.Execute(null);
    }

    private void YamlTreeTabHeader_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
      if (this.ViewModel.EditorMode != EditorMode.XmlUtility)
        this.ViewModel.ToggleEditorModeCommand.Execute(null);
    }

    private void MarkdownPreviewTabHeader_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
      // If pinned, clicking the tab header unpins
      if (_isPreviewPinned)
      {
        UnpinPreview();
        return;
      }

      if (this.ViewModel.EditorMode != EditorMode.XmlUtility)
        this.ViewModel.ToggleEditorModeCommand.Execute(null);
    }

    private void PinPreviewButton_Click(object sender, RoutedEventArgs e)
    {
      if (_isPreviewPinned)
        UnpinPreview();
      else
        PinPreview();
    }

    private void PinPreview()
    {
      _isPreviewPinned = true;

      // Feed current editor text to the preview
      this.ViewModel.MarkdownUtility.DocumentText = this.AvalonEditor.Text;

      // If we were in utility mode (preview-only), switch back to editor
      if (this.ViewModel.EditorMode == EditorMode.XmlUtility)
        this.ViewModel.ToggleEditorModeCommand.Execute(null);

      // Show the split layout: editor left, preview right
      this.PreviewSplitterColumn.Width = new GridLength(5);
      this.PinnedPreviewColumn.Width = new GridLength(1, GridUnitType.Star);
      this.PreviewSplitter.Visibility = Visibility.Visible;

      // Move preview panel to the pinned column and make it visible
      System.Windows.Controls.Grid.SetColumn(this.MarkdownPreviewPanel, 2);
      this.MarkdownPreviewPanel.Visibility = Visibility.Visible;
      this.MarkdownPreviewPanel.CornerRadius = new CornerRadius(6, 6, 6, 6);

      // Update pin icon to show "pinned" state
      this.PinPreviewIcon.Symbol = Wpf.Ui.Controls.SymbolRegular.PinOff24;
      this.PinPreviewIcon.Opacity = 1.0;

      // Start debounced live updates
      if (_previewDebounce == null)
      {
        _previewDebounce = new System.Windows.Threading.DispatcherTimer
        {
          Interval = TimeSpan.FromMilliseconds(400)
        };
        _previewDebounce.Tick += (s, e) =>
        {
          _previewDebounce.Stop();
          if (_isPreviewPinned && _currentContentType == DocumentContentType.Markdown)
            this.ViewModel.MarkdownUtility.DocumentText = this.AvalonEditor.Text;
        };
      }

      this.AvalonEditor.TextChanged += PinnedPreview_TextChanged;
    }

    private void UnpinPreview()
    {
      _isPreviewPinned = false;

      // Stop live updates
      this.AvalonEditor.TextChanged -= PinnedPreview_TextChanged;
      _previewDebounce?.Stop();

      // Collapse the split layout
      this.PreviewSplitterColumn.Width = new GridLength(0);
      this.PinnedPreviewColumn.Width = new GridLength(0);
      this.PreviewSplitter.Visibility = Visibility.Collapsed;

      // Move preview panel back to the overlay column and hide it
      System.Windows.Controls.Grid.SetColumn(this.MarkdownPreviewPanel, 0);
      this.MarkdownPreviewPanel.Visibility = Visibility.Collapsed;
      this.MarkdownPreviewPanel.CornerRadius = new CornerRadius(6, 0, 6, 6);

      // Update pin icon
      this.PinPreviewIcon.Symbol = Wpf.Ui.Controls.SymbolRegular.Pin24;
      this.PinPreviewIcon.Opacity = 0.6;
    }

    private void PinnedPreview_TextChanged(object sender, EventArgs e)
    {
      _previewDebounce?.Stop();
      _previewDebounce?.Start();
    }

    private void ContentTypeLabel_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
      var menu = new System.Windows.Controls.ContextMenu();
      foreach (var type in new[] { DocumentContentType.Xml, DocumentContentType.Json, DocumentContentType.Yaml, DocumentContentType.Markdown, DocumentContentType.Text })
      {
        var item = new System.Windows.Controls.MenuItem
        {
          Header = type.ToString().ToUpper(),
          IsChecked = (this.ViewModel.Editor.ContentType == type)
        };
        var capturedType = type;
        item.Click += async (s, args) => await ChangeContentTypeAsync(capturedType);
        menu.Items.Add(item);
      }
      menu.PlacementTarget = sender as System.Windows.UIElement;
      menu.Placement = System.Windows.Controls.Primitives.PlacementMode.Top;
      menu.IsOpen = true;
    }

    private async Task ChangeContentTypeAsync(DocumentContentType contentType)
    {
      string sourceText;

      if (ViewModel.Editor.HasDocumentBeenEditedSinceLoad)
      {
        var dialog = new ContentDialog(RootContentDialogPresenter)
        {
          Title = "Reprocess document",
          Content = $"The editor has been modified since it was loaded. Which text should be reprocessed as {contentType.ToString().ToUpper()}?",
          PrimaryButtonText = "Original input",
          SecondaryButtonText = "Current editor content",
          CloseButtonText = "Cancel"
        };

        var result = await dialog.ShowAsync();

        if (result == ContentDialogResult.None)
          return;

        sourceText = result == ContentDialogResult.Primary
          ? ViewModel.Editor.OriginalRawText
          : ViewModel.Editor.Document.Text;
      }
      else
      {
        sourceText = ViewModel.Editor.OriginalRawText ?? ViewModel.Editor.Document.Text;
      }

      await ViewModel.Editor.ReprocessAsContentTypeAsync(contentType, sourceText);
    }



    private void HandleContentTypeChanged(ContentTypeChangedMessage message)
    {
      _currentContentType = message.ContentType;

      // Update status bar label
      if (this.ContentTypeLabel != null)
        this.ContentTypeLabel.Text = message.ContentType.ToString().ToUpper();

      // Update app title in title bar
      if (this.AppTitleRun != null)
      {
        this.AppTitleRun.Text = message.ContentType switch
        {
          DocumentContentType.Json => "JaSON",
          DocumentContentType.Yaml => "YAMeL",
          DocumentContentType.Markdown => "MarkDown",
          DocumentContentType.Text => "TeXT",
          _ => "eXeMeL"
        };
      }

      // Update window/taskbar title
      UpdateWindowTitle();

      // Auto-unpin if switching away from Markdown
      if (_isPreviewPinned && message.ContentType != DocumentContentType.Markdown)
        UnpinPreview();

      // Show/hide appropriate utility tabs
      this.XPathTabHeader.Visibility = Visibility.Collapsed;
      this.JsonTreeTabHeader.Visibility = Visibility.Collapsed;
      this.YamlTreeTabHeader.Visibility = Visibility.Collapsed;
      this.MarkdownPreviewTabHeader.Visibility = Visibility.Collapsed;

      switch (message.ContentType)
      {
        case DocumentContentType.Xml:
          this.XPathTabHeader.Visibility = Visibility.Visible;
          break;
        case DocumentContentType.Json:
          this.JsonTreeTabHeader.Visibility = Visibility.Visible;
          break;
        case DocumentContentType.Yaml:
          this.YamlTreeTabHeader.Visibility = Visibility.Visible;
          break;
        case DocumentContentType.Markdown:
          this.MarkdownPreviewTabHeader.Visibility = Visibility.Visible;
          break;
        // Text: no utility tab
      }

      // If currently in utility mode, switch the visible panel
      if (this.ViewModel.EditorMode == EditorMode.XmlUtility)
      {
        this.XPathPanel.Visibility = Visibility.Collapsed;
        this.JsonTreePanel.Visibility = Visibility.Collapsed;
        this.YamlTreePanel.Visibility = Visibility.Collapsed;
        this.MarkdownPreviewPanel.Visibility = Visibility.Collapsed;

        switch (message.ContentType)
        {
          case DocumentContentType.Xml:
            this.XPathPanel.Visibility = Visibility.Visible;
            break;
          case DocumentContentType.Json:
            this.JsonTreePanel.Visibility = Visibility.Visible;
            break;
          case DocumentContentType.Yaml:
            this.YamlTreePanel.Visibility = Visibility.Visible;
            break;
          case DocumentContentType.Markdown:
            this.MarkdownPreviewPanel.Visibility = Visibility.Visible;
            break;
          case DocumentContentType.Text:
            // No utility — switch back to editor
            this.ViewModel.ToggleEditorModeCommand.Execute(null);
            break;
        }
      }

      // Manage markdown formatting transformer
      UpdateMarkdownTransformer();

      // Re-fold with the right strategy
      UpdateDocumentFoldings();
    }

    private void HandleEditorModeChangedMessage(EditorModeChangedMessage message)
    {
      UpdateTabVisuals(message.EditorMode);
    }

    private void UpdateTabVisuals(EditorMode mode)
    {
      if (this.EditorTabHeader == null) return;

      var activeBg = (System.Windows.Media.Brush)FindResource("EditorTintOverlayBrush");
      var activeBorder = (System.Windows.Media.Brush)FindResource("ControlStrokeColorDefaultBrush");
      var inactiveBg = System.Windows.Media.Brushes.Transparent;
      var inactiveBorder = System.Windows.Media.Brushes.Transparent;

      // Reset all to inactive
      void SetInactive(System.Windows.Controls.Border header) { header.Background = inactiveBg; header.BorderBrush = inactiveBorder; }
      void SetActive(System.Windows.Controls.Border header) { header.Background = activeBg; header.BorderBrush = activeBorder; }

      SetInactive(this.EditorTabHeader);
      SetInactive(this.XPathTabHeader);
      SetInactive(this.JsonTreeTabHeader);
      SetInactive(this.YamlTreeTabHeader);
      SetInactive(this.MarkdownPreviewTabHeader);

      // Hide all content panels (but leave pinned preview alone — it's managed separately)
      this.EditorPanel.Visibility = Visibility.Collapsed;
      this.XPathPanel.Visibility = Visibility.Collapsed;
      this.JsonTreePanel.Visibility = Visibility.Collapsed;
      this.YamlTreePanel.Visibility = Visibility.Collapsed;
      if (!_isPreviewPinned)
        this.MarkdownPreviewPanel.Visibility = Visibility.Collapsed;

      if (mode == EditorMode.Editor)
      {
        this.EditorPanel.Visibility = Visibility.Visible;
        SetActive(this.EditorTabHeader);

        // When pinned, keep the preview tab visually active too
        if (_isPreviewPinned)
          SetActive(this.MarkdownPreviewTabHeader);
      }
      else
      {
        SetInactive(this.EditorTabHeader);

        switch (_currentContentType)
        {
          case DocumentContentType.Json:
            this.JsonTreePanel.Visibility = Visibility.Visible;
            SetActive(this.JsonTreeTabHeader);
            break;
          case DocumentContentType.Yaml:
            this.YamlTreePanel.Visibility = Visibility.Visible;
            SetActive(this.YamlTreeTabHeader);
            break;
          case DocumentContentType.Markdown:
            if (!_isPreviewPinned)
            {
              this.MarkdownPreviewPanel.Visibility = Visibility.Visible;
              SetActive(this.MarkdownPreviewTabHeader);
            }
            break;
          case DocumentContentType.Text:
            // No utility panel — stay on editor
            this.EditorPanel.Visibility = Visibility.Visible;
            SetActive(this.EditorTabHeader);
            break;
          default:
            this.XPathPanel.Visibility = Visibility.Visible;
            SetActive(this.XPathTabHeader);
            break;
        }
      }
    }

    private void HandleApplicationThemeUpdatedMessage(ApplicationThemeUpdatedMessage message)
    {
      // Schedule backdrop change on next dispatcher pass to avoid
      // conflicting with FluentWindow's chrome reconfiguration
      this.Dispatcher.BeginInvoke(new Action(ApplyBackdropForCurrentTheme),
        System.Windows.Threading.DispatcherPriority.Background);
    }

    private void ApplyBackdropForCurrentTheme()
    {
      var theme = this.ViewModel?.Settings?.ApplicationTheme ?? ApplicationTheme.Dark;
      var backdropType = theme.IsGlassTheme()
        ? Wpf.Ui.Controls.WindowBackdropType.Acrylic
        : Wpf.Ui.Controls.WindowBackdropType.Mica;

      try
      {
        // Remove existing background so backdrop shows through
        Wpf.Ui.Controls.WindowBackdrop.RemoveBackground(this);
        // Apply via static method (bypasses FluentWindow chrome conflicts)
        Wpf.Ui.Controls.WindowBackdrop.ApplyBackdrop(this, backdropType);
      }
      catch
      {
        // Fallback: try the property setter
        try { this.WindowBackdropType = backdropType; } catch { }
      }

      // Re-apply our chrome settings since backdrop changes can reset them
      ApplyWindowChrome();
    }


    private void UpdateMarkdownTransformer()
    {
      if (_currentContentType == DocumentContentType.Markdown)
      {
        if (_markdownTransformer == null)
        {
          _markdownTransformer = new MarkdownFormattingTransformer();
          this.AvalonEditor.TextArea.TextView.LineTransformers.Add(_markdownTransformer);
        }
      }
      else
      {
        if (_markdownTransformer != null)
        {
          this.AvalonEditor.TextArea.TextView.LineTransformers.Remove(_markdownTransformer);
          _markdownTransformer = null;
        }
      }
    }

    private void UpdateCurrentLineHighlight()
    {
      if (this.AvalonEditor == null || this.ViewModel?.Settings == null) return;

      // Must enable the option AND set the brush
      this.AvalonEditor.Options.HighlightCurrentLine = this.ViewModel.Settings.HighlightCurrentLine;

      if (this.ViewModel.Settings.HighlightCurrentLine)
      {
        var bgBrush = new System.Windows.Media.SolidColorBrush(
          System.Windows.Media.Color.FromArgb(40, 180, 180, 180));
        bgBrush.Freeze();
        this.AvalonEditor.TextArea.TextView.CurrentLineBackground = bgBrush;

        var borderPen = new System.Windows.Media.Pen(
          new System.Windows.Media.SolidColorBrush(
            System.Windows.Media.Color.FromArgb(30, 200, 200, 200)), 1);
        borderPen.Freeze();
        this.AvalonEditor.TextArea.TextView.CurrentLineBorder = borderPen;
      }
      else
      {
        this.AvalonEditor.TextArea.TextView.CurrentLineBackground = null;
        this.AvalonEditor.TextArea.TextView.CurrentLineBorder = null;
      }
    }

    private void ApplyWindowChrome()
    {
      var chrome = System.Windows.Shell.WindowChrome.GetWindowChrome(this);
      if (chrome != null)
      {
        chrome.CaptionHeight = 46;
        chrome.UseAeroCaptionButtons = false;
      }
      else
      {
        System.Windows.Shell.WindowChrome.SetWindowChrome(this, new System.Windows.Shell.WindowChrome
        {
          CaptionHeight = 46,
          ResizeBorderThickness = new Thickness(5),
          GlassFrameThickness = new Thickness(0),
          UseAeroCaptionButtons = false
        });
      }
    }

    private void HandleSetKeyboardFocusToEditorMessage(SetKeyboardFocusToEditor obj)
    {
      this.ResetFocusCommand.Execute(null);
    }



    #region Update Toast

    private async void CheckForUpdatesOnStartup()
    {
      // Delay update check so it doesn't compete with initial content load
      await Task.Delay(5000);

#if DEBUG
      this.UpdateToastMessage.Text = "eXeMeL 2.99.0 is available! (DEBUG)";
      this.UpdateToast.Visibility = Visibility.Visible;
      return;
#endif

#pragma warning disable CS0162 // Unreachable code
      var hasUpdate = await App.CheckForUpdatesAsync(silent: true);
      if (hasUpdate && App.LatestUpdate != null)
      {
        this.UpdateToastMessage.Text = $"eXeMeL {App.LatestUpdate.TargetFullRelease.Version} is available!";
        this.UpdateToast.Visibility = Visibility.Visible;
      }
#pragma warning restore CS0162
    }

    private async void UpdateNowButton_Click(object sender, RoutedEventArgs e)
    {
#if DEBUG
      this.UpdateToastMessage.Text = "Downloading update... (DEBUG - no actual update)";
      await Task.Delay(2000);
      this.UpdateToast.Visibility = Visibility.Collapsed;
      return;
#endif

#pragma warning disable CS0162
      this.UpdateToastMessage.Text = "Downloading update...";
      await App.ApplyUpdateAsync();
#pragma warning restore CS0162
    }

    private void UpdateLaterButton_Click(object sender, RoutedEventArgs e)
    {
      this.UpdateToast.Visibility = Visibility.Collapsed;
    }

    private void UpdateDismissButton_Click(object sender, RoutedEventArgs e)
    {
      this.UpdateToast.Visibility = Visibility.Collapsed;
    }

    #endregion



    private void ChangeLogButton_Click(object sender, RoutedEventArgs e)
    {
      ShowChangeLog();
    }



    private void ShowChangeLog()
    {
      var changeLogWindow = new ChangeLogWindow(this.ViewModel.Settings.ApplicationTheme) { Owner = this };
      changeLogWindow.Show();
    }



    #region Window Position Save/Restore

    private void SaveWindowPosition()
    {
      if (this.ViewModel?.Settings == null) return;

      var settings = this.ViewModel.Settings;

      if (this.WindowState == WindowState.Normal)
      {
        settings.WindowLeft = this.Left;
        settings.WindowTop = this.Top;
        settings.WindowWidth = this.Width;
        settings.WindowHeight = this.Height;
      }
      settings.WindowState = (int)this.WindowState;
    }

    private void RestoreWindowPosition()
    {
      // ViewModel may not be set yet during construction; defer to Loaded if needed
      if (this.ViewModel?.Settings == null) return;

      var settings = this.ViewModel.Settings;

      if (!double.IsNaN(settings.WindowWidth) && settings.WindowWidth > 0)
        this.Width = settings.WindowWidth;
      if (!double.IsNaN(settings.WindowHeight) && settings.WindowHeight > 0)
        this.Height = settings.WindowHeight;
      if (!double.IsNaN(settings.WindowLeft))
        this.Left = settings.WindowLeft;
      if (!double.IsNaN(settings.WindowTop))
        this.Top = settings.WindowTop;

      if (settings.WindowState == (int)WindowState.Maximized)
        this.WindowState = WindowState.Maximized;
    }

    #endregion
  }
}
