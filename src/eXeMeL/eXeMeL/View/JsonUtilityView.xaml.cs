using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using eXeMeL.Model;
using eXeMeL.ViewModel.JsonUtility;

namespace eXeMeL.View
{
  public partial class JsonUtilityView : UserControl, INotifyPropertyChanged
  {
    public JsonUtilityView()
    {
      InitializeComponent();
      this.DataContextChanged += (s, e) =>
      {
        OnPropertyChanged("ViewModel");
        OnPropertyChanged("Settings");
      };
    }

    public JsonUtilityViewModel ViewModel => this.DataContext as JsonUtilityViewModel;
    public Settings Settings => this.ViewModel?.Settings;
    public event PropertyChangedEventHandler PropertyChanged;

    protected virtual void OnPropertyChanged(string propertyName)
    {
      this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    private void NodeHeader_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
      if (sender is FrameworkElement fe && fe.DataContext is JsonNodeViewModel vm && vm.HasChildren)
      {
        vm.IsExpanded = !vm.IsExpanded;
        e.Handled = true;
      }
    }
  }
}
