using eXeMeL.Messages;
using eXeMeL.ViewModel;
using GalaSoft.MvvmLight.Messaging;
using MahApps.Metro.Controls;
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
    private MainViewModel ViewModel { get { return this.DataContext as MainViewModel; } }



    public MainWindow()
    {
      this.Closing += MainWindow_Closing;
      InitializeComponent();

      //DataObject.AddPastingHandler(this.AvalonEditor, PasteHandler);

      this.AvalonEditor.PreviewKeyDown += AvalonEditor_PreviewKeyDown;
      //this.AvalonEditor.KeyDown += AvalonEditor_KeyDown;
    }



    //private void PasteHandler(object sender, DataObjectPastingEventArgs e)
    //{
    //  //var formats = e.DataObject.GetFormats(true);
    //  //formats[0] == 
    //}


    //void AvalonEditor_KeyDown(object sender, KeyEventArgs e)
    //{
      
    //}



    private void AvalonEditor_PreviewKeyDown(object sender, KeyEventArgs e)
    {
      if (e.Key == Key.V && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
      {
        var cleanText = this.ViewModel.Editor.CleanXmlIfPossible(Clipboard.GetText());
        Clipboard.SetText(cleanText);
      }
    }



    private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
    {
      Messenger.Default.Send<ApplicationClosingMessage>(new ApplicationClosingMessage());
    }



    private void OpenSettingsButton_Click(object sender, RoutedEventArgs e)
    {
      this.SettingsFlyout.IsOpen = !this.SettingsFlyout.IsOpen;
    }
  }
}
