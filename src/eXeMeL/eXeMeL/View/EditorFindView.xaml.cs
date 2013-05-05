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

namespace eXeMeL.View
{
  public partial class EditorFindView : UserControl
  {

    public EditorFindView()
    {
      InitializeComponent();
    }



    public new void Focus()
    {
      this.FindSearchText.Focus();
    }



    private void FindSearchText_PreviewKeyDown(object sender, KeyEventArgs e)
    {
      if (e.Key == Key.Enter || e.Key == Key.Return)
      {
        this.FindNextButton.Command.Execute(null);
      }
    }
  }
}
