using eXeMeL.Messages;
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



    public MainWindow()
    {
      this.Closing += MainWindow_Closing;
      this.Loaded += MainWindow_Loaded;
      this.DataContextChanged += MainWindow_DataContextChanged;

      InitializeComponent();

      //DataObject.AddPastingHandler(this.AvalonEditor, PasteHandler);

      this.AvalonEditor.PreviewKeyDown += AvalonEditor_PreviewKeyDown;
      //this.AvalonEditor.KeyDown += AvalonEditor_KeyDown;

      this.FoldingManager = FoldingManager.Install(this.AvalonEditor.TextArea);
      this.FoldingStrategy = new XmlFoldingStrategy();
    }



    private void MainWindow_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
      //this.ViewModel.PropertyChanged += ViewModel_PropertyChanged;

      this.TextDocumentObserver = 
        new PropertyObserver<TextDocument>(this.ViewModel.Editor.Document)
          .RegisterHandler(x => x.Text, HandleChangedDocumentText);

      HandleChangedDocumentText(this.ViewModel.Editor.Document);
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
  }
}
