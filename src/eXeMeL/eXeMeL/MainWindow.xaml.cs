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

    public ICommand FocusOnFindControlCommand { get; private set; }
    public ICommand ResetFocusCommand { get; private set; }


    public MainWindow()
    {
      this.Closing += MainWindow_Closing;
      this.Loaded += MainWindow_Loaded;
      this.DataContextChanged += MainWindow_DataContextChanged;
      this.FocusOnFindControlCommand = new RelayCommand(FocusOnFindControlCommand_Executed);
      this.ResetFocusCommand = new RelayCommand(ResetFocusCommand_Executed);

      InitializeComponent();

      this.AvalonEditor.PreviewKeyDown += AvalonEditor_PreviewKeyDown;

      this.FoldingManager = FoldingManager.Install(this.AvalonEditor.TextArea);
      this.FoldingStrategy = new XmlFoldingStrategy();
    }



    private void MainWindow_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
      this.TextDocumentObserver = 
        new PropertyObserver<TextDocument>(this.ViewModel.Editor.Document)
          .RegisterHandler(x => x.Text, HandleChangedDocumentText);

      GalaSoft.MvvmLight.Messaging.Messenger.Default.Register<SelectTextInEditorMessage>(this, HandleSelectTextInEditorMessage);
      GalaSoft.MvvmLight.Messaging.Messenger.Default.Register<UnselectTextInEditorMessage>(this, HandleUnselectTextInEditorMessage);

      this.ViewModel.Editor.RefreshComplete += Editor_RefreshComplete;

      HandleChangedDocumentText(this.ViewModel.Editor.Document);
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


    
    private void HandleChangedDocumentText(TextDocument document)
    {
      if (this.FoldingManager != null && this.FoldingStrategy != null)
      { 
        this.FoldingStrategy.UpdateFoldings(this.FoldingManager, this.AvalonEditor.Document);
      }
    }



    private void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
      this.ViewModel.Editor.RefreshCommand.Execute(null);
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
      this.EditorFindControl.Focus();
    }



    private void ResetFocusCommand_Executed()
    {
      this.AvalonEditor.Focus();
    }
  }
}
