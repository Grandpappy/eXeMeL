using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
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
using eXeMeL.Model;
using eXeMeL.ViewModel;

namespace eXeMeL.View
{
  /// <summary>
  /// Interaction logic for XmlUtilityView.xaml
  /// </summary>
  public partial class XmlUtilityView : UserControl, INotifyPropertyChanged
  {
    public XmlUtilityView()
    {
      this.DataContextChanged += XmlUtilityView_DataContextChanged;
      InitializeComponent();
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
  }
}
