using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using eXeMeL.Messages;
using eXeMeL.Model;
using eXeMeL.Utilities;
using eXeMeL.ViewModel;
using GalaSoft.MvvmLight.Messaging;

namespace eXeMeL.View
{
  /// <summary>
  /// Interaction logic for XmlUtilityView.xaml
  /// </summary>
  public partial class XmlUtilityView : UserControl, INotifyPropertyChanged
  {
    public XmlUtilityView()
    {
      Messenger.Default.Register<EditorModeChangedMessage>(this, HandleEditorModeChangedMessage);

      this.DataContextChanged += XmlUtilityView_DataContextChanged;
      InitializeComponent();
    }



    private void HandleEditorModeChangedMessage(EditorModeChangedMessage message)
    {
      if (message.EditorMode == EditorMode.XmlUtility)
      {
        // Since we don't know when this is called when compaired to a visual state change, queue up the work
        // on the UI thread after whatever's running

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



    private void ElementStartToggleButton_OnLoaded(object sender, RoutedEventArgs eventArgs)
    {
      var toggleButton = sender as ToggleButton;
      var vm = toggleButton?.DataContext as ElementViewModel;

      // I think this is going to cause a memory leak.  This should probably be pulled into a UserControl made 
      // specifically for the element
      vm.BringIntoView += (s, e) =>
      {
        UIThread.Run(() =>
        {
          toggleButton.BringIntoView();
        });
      };
    }



    //private void ElementOnBringIntoView(object sender, EventArgs eventArgs)
    //{
      
    //}



    //private void ElementStartToggleButton_OnUnloaded(object sender, RoutedEventArgs e)
    //{
      
    //}
  }



  //public static class ToggleButtonBringIntoViewBehavior
  //{
  //  #region IsBroughtIntoViewWhenSelected
  //  //public static bool GetIsBroughtIntoViewWhenSelected(XmlNodeViewModel node)
  //  //{
  //  //  return (bool)node.GetValue(IsBroughtIntoViewWhenSelectedProperty);
  //  //}

  //  //public static void SetIsBroughtIntoViewWhenSelected(TreeViewItem treeViewItem, bool value)
  //  //{
  //  //  treeViewItem.SetValue(IsBroughtIntoViewWhenSelectedProperty, value);
  //  //}

  //  public static readonly DependencyProperty IsBroughtIntoViewWhenSelectedProperty =
  //   DependencyProperty.RegisterAttached(
  //   "IsBroughtIntoViewWhenSelected",
  //   typeof(bool),
  //   typeof(ToggleButtonBringIntoViewBehavior),
  //   new UIPropertyMetadata(false, OnIsBroughtIntoViewWhenSelectedChanged));

  //  static void OnIsBroughtIntoViewWhenSelectedChanged(DependencyObject depObj, DependencyPropertyChangedEventArgs e)
  //  {
  //    ToggleButton item = depObj as ToggleButton;
  //    if (item == null)
  //      return;

  //    if (e.NewValue is bool == false)
  //      return;

  //    if ((bool)e.NewValue)
  //      item.Selected += OnTreeViewItemSelected;
  //    else
  //      item.Selected -= OnTreeViewItemSelected;
  //  }

  //  static void OnTreeViewItemSelected(object sender, RoutedEventArgs e)
  //  {
  //    // Only react to the Selected event raised by the TreeViewItem
  //    // whose IsSelected property was modified. Ignore all ancestors
  //    // who are merely reporting that a descendant's Selected fired.
  //    if (!Object.ReferenceEquals(sender, e.OriginalSource))
  //      return;

  //    TreeViewItem item = e.OriginalSource as TreeViewItem;
  //    if (item != null)
  //      item.BringIntoView();
  //  }

  //  #endregion // IsBroughtIntoViewWhenSelected
  //}
}
