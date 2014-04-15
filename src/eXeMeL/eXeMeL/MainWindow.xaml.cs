using eXeMeL.Messages;
using eXeMeL.View;
using eXeMeL.ViewModel;
using GalaSoft.MvvmLight.Messaging;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Folding;
using MahApps.Metro.Controls;
using MvvmFoundation.Wpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.ComponentModel;
using System.Xml;
using System.IO;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;
using eXeMeL.View.ChangeLog;

namespace eXeMeL
{
  /// <summary>
  /// Interaction logic for MainWindow.xaml
  /// </summary>
  public partial class MainWindow : MetroWindow
  {
    private FoldingManager FoldingManager { get; set; }
    private XmlFoldingStrategy FoldingStrategy { get; set; }
    private MainViewModel ViewModel { get { return this.DataContext as MainViewModel; } }
    private PropertyObserver<TextDocument> TextDocumentObserver { get; set; }
    private PropertyObserver<EditorViewModel> EditorObserver { get; set; }
    private bool IgnoreNextTextChange { get; set; }

    public ICommand FocusOnFindControlCommand { get; private set; }
    public ICommand ResetFocusCommand { get; private set; }


    public MainWindow()
    {
      this.Closing += MainWindow_Closing;
      this.Loaded += MainWindow_Loaded;
      this.DataContextChanged += MainWindow_DataContextChanged;
      this.AllowDrop = true;
      this.Drop += MainWindow_Drop;
      this.FocusOnFindControlCommand = new RelayCommand(FocusOnFindControlCommand_Executed);
      this.ResetFocusCommand = new RelayCommand(ResetFocusCommand_Executed);

      InitializeComponent();

      this.AvalonEditor.PreviewKeyDown += AvalonEditor_PreviewKeyDown;
      this.AvalonEditor.TextArea.DocumentChanged += TextArea_DocumentChanged;
      this.AvalonEditor.TextArea.Caret.PositionChanged += AvalonEditor_CaretPositionChanged;
      this.AvalonEditor.TextChanged += AvalonEditor_TextChanged;
      this.AvalonEditor.SyntaxHighlighting = GetSyntaxHighlighting();
      
      this.FoldingManager = FoldingManager.Install(this.AvalonEditor.TextArea);
      this.FoldingStrategy = new XmlFoldingStrategy();

      this.IgnoreNextTextChange = false;
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



    private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
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



    private IHighlightingDefinition GetSyntaxHighlighting()
    {
      using (Stream stream = this.GetType().Assembly.GetManifestResourceStream("eXeMeL.Assets.XmlSyntaxHighlighting.xshd"))
      {
        using (XmlTextReader reader = new XmlTextReader(stream))
        {
          return HighlightingLoader.Load(reader, HighlightingManager.Instance);
        }
      }
    }

    private void ChangeLogButton_Click(object sender, RoutedEventArgs e)
    {
      var changeLogWindow = new ChangeLogWindow() { Owner = this };
      changeLogWindow.ShowDialog();
    }

  }
}
