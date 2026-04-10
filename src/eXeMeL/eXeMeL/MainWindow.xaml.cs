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
using Wpf.Ui.Controls;

namespace eXeMeL
{
  public partial class MainWindow : FluentWindow
  {
    private FoldingManager FoldingManager { get; set; }
    private XmlFoldingStrategy FoldingStrategy { get; set; }
    public MainViewModel ViewModel => this.DataContext as MainViewModel;
    private PropertyObserver<TextDocument> TextDocumentObserver { get; set; }
    private bool IgnoreNextTextChange { get; set; }

    public ICommand FocusOnFindControlCommand { get; private set; }
    public ICommand ResetFocusCommand { get; private set; }

    public ICommand FoldLevelCommand { get; private set; }
    public ICommand UnFoldLevelCommand { get; private set; }

    private bool _isSettingsOpen;


    public MainWindow()
    {
      UIThread.Initialize(this.Dispatcher, false);

      this.Closing += MainWindow_Closing;
      this.Loaded += MainWindow_Loaded;
      this.DataContextChanged += MainWindow_DataContextChanged;
      this.AllowDrop = true;
      this.Drop += MainWindow_Drop;
      this.FocusOnFindControlCommand = new RelayCommand(FocusOnFindControlCommand_Executed);
      this.ResetFocusCommand = new RelayCommand(ResetFocusCommand_Executed);
      this.FoldLevelCommand = new RelayCommand<string>(l => FoldSections(l, true));
      this.UnFoldLevelCommand = new RelayCommand<string>(l => FoldSections(l, false));

      InitializeComponent();

      this.AvalonEditor.PreviewKeyDown += AvalonEditor_PreviewKeyDown;
      this.AvalonEditor.TextArea.DocumentChanged += TextArea_DocumentChanged;
      this.AvalonEditor.TextArea.TextView.LineTransformers.Add(new AllSelectionColorizer(this.AvalonEditor, this.ViewModel.Settings));
      this.AvalonEditor.TextArea.SelectionChanged += (sender, args) => this.AvalonEditor.TextArea.TextView.Redraw();
      this.AvalonEditor.TextArea.Caret.PositionChanged += AvalonEditor_CaretPositionChanged;
      this.AvalonEditor.TextChanged += AvalonEditor_TextChanged;

      this.FoldingManager = FoldingManager.Install(this.AvalonEditor.TextArea);
      this.FoldingStrategy = new XmlFoldingStrategy();

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

      this.ViewModel.Editor.RefreshComplete += Editor_RefreshComplete;
      this.ViewModel.Editor.PropertyChanging += Editor_PropertyChanging;

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
    }



    private void HandleChangedDocumentText(TextDocument document)
    {
      UpdateDocumentFoldings();
    }



    private void UpdateDocumentFoldings()
    {
      if (this.FoldingManager != null && this.FoldingStrategy != null)
      {
        this.FoldingStrategy.UpdateFoldings(this.FoldingManager, this.AvalonEditor.Document);
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
      {
        WeakReferenceMessenger.Default.Send<SetSearchTextMessage>(new SetSearchTextMessage(this.AvalonEditor.SelectedText));
      }

      this.EditorFindControl.Focus();
    }



    private void ResetFocusCommand_Executed()
    {
      this.AvalonEditor.Focus();
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



    private void HandleApplicationThemeUpdatedMessage(ApplicationThemeUpdatedMessage message)
    {
      // Theme updated — WPF-UI and eXeMeL resource dictionaries are both swapped by ApplicationThemeManager
    }

    private void HandleSetKeyboardFocusToEditorMessage(SetKeyboardFocusToEditor obj)
    {
      this.ResetFocusCommand.Execute(null);
    }



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
