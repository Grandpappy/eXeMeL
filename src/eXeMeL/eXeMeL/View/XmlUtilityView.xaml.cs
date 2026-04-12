using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using eXeMeL.Messages;
using eXeMeL.Model;
using eXeMeL.Utilities;
using eXeMeL.ViewModel;
using CommunityToolkit.Mvvm.Messaging;

namespace eXeMeL.View
{
  public partial class XmlUtilityView : UserControl, INotifyPropertyChanged
  {
    public XmlUtilityView()
    {
      WeakReferenceMessenger.Default.Register<EditorModeChangedMessage>(this, (r, m) => HandleEditorModeChangedMessage(m));

      this.DataContextChanged += XmlUtilityView_DataContextChanged;
      InitializeComponent();
    }



    private void HandleEditorModeChangedMessage(EditorModeChangedMessage message)
    {
      if (message.EditorMode == EditorMode.XmlUtility)
      {
        UIThread.Queue(() => this.XPathTextBox.Focus());
      }
    }



    private void XmlUtilityView_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
      OnPropertyChanged("ViewModel");
      OnPropertyChanged("Settings");
    }



    public XmlUtilityViewModel ViewModel => this.DataContext as XmlUtilityViewModel;
    public Settings Settings => this.ViewModel?.Settings;
    public event PropertyChangedEventHandler PropertyChanged;



    protected virtual void OnPropertyChanged(string propertyName)
    {
      this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }



    private void ElementHeader_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
      if (sender is FrameworkElement fe && fe.DataContext is ElementViewModel vm)
      {
        vm.IsExpanded = !vm.IsExpanded;
        e.Handled = true;
      }
    }
  }
}
