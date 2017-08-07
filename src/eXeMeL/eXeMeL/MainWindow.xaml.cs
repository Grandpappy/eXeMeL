using eXeMeL.Messages;
using eXeMeL.ViewModel;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Folding;
using MahApps.Metro.Controls;
using MvvmFoundation.Wpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.ComponentModel;
using eXeMeL.View.ChangeLog;
using eXeMeL.Model;
using eXeMeL.Utilities;

namespace eXeMeL
{
  /// <summary>
  /// Interaction logic for MainWindow.xaml
  /// </summary>
  public partial class MainWindow : MetroWindow
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
      SetWindowGlow();
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

      GalaSoft.MvvmLight.Messaging.Messenger.Default.Register<SelectTextInEditorMessage>(this, HandleSelectTextInEditorMessage);
      GalaSoft.MvvmLight.Messaging.Messenger.Default.Register<UnselectTextInEditorMessage>(this, HandleUnselectTextInEditorMessage);
      GalaSoft.MvvmLight.Messaging.Messenger.Default.Register<DocumentTextReplacedMessage>(this, HandleDocumentTextReplacedMessage);
      GalaSoft.MvvmLight.Messaging.Messenger.Default.Register<ApplicationThemeUpdatedMessage>(this, HandleApplicationThemeUpdatedMessage);
      GalaSoft.MvvmLight.Messaging.Messenger.Default.Register<SetKeyboardFocusToEditor>(this, HandleSetKeyboardFocusToEditorMessage);

      this.ViewModel.Editor.RefreshComplete += Editor_RefreshComplete;
      this.ViewModel.Editor.PropertyChanging += Editor_PropertyChanging;

      //this.SelectionColorizer.Settings = this.ViewModel.Settings;

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


      if (!ApplicationVersionControl.CurrentVersionIsDifferentFromLastRunVersion())
      {
        ShowChangeLog();
        ApplicationVersionControl.WriteCurrentVersionToRegistry();
      }
    }



    private void Editor_RefreshComplete(object sender, EventArgs e)
    {
      this.AvalonEditor.CaretOffset = 0;
      this.AvalonEditor.Focus();
    }



    private void AvalonEditor_PreviewKeyDown(object sender, KeyEventArgs e)
    {
      //if (e.Key == Key.V && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
      //{
      //  var cleanText = this.ViewModel.Editor.CleanXmlIfPossible(Clipboard.GetText());
      //  Clipboard.SetText(cleanText);
      //}
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
      GalaSoft.MvvmLight.Messaging.Messenger.Default.Send<ApplicationClosingMessage>(new ApplicationClosingMessage());
    }



    private void OpenSettingsButton_Click(object sender, RoutedEventArgs e)
    {
      this.SettingsFlyout.IsOpen = !this.SettingsFlyout.IsOpen;
    }



    private void FocusOnFindControlCommand_Executed()
    {
      if (!string.IsNullOrEmpty(this.AvalonEditor.SelectedText))
      { 
        GalaSoft.MvvmLight.Messaging.Messenger.Default.Send<SetSearchTextMessage>(new SetSearchTextMessage(this.AvalonEditor.SelectedText));
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
      SetWindowGlow();
    }

    private void HandleSetKeyboardFocusToEditorMessage(SetKeyboardFocusToEditor obj)
    {
      this.ResetFocusCommand.Execute(null);
    }
    

    private void SetWindowGlow()
    {
      this.GlowBrush.Color = (Color)FindResource("WindowGlowColor");
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
  }
}
