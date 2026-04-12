using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ICSharpCode.AvalonEdit.Document;

namespace eXeMeL.ViewModel
{
  public class DocumentSnapshot : ObservableObject
  {
    public TextDocument Document { get; set; }
    private string _Identifier;
    public string Identifier
    {
      get { return _Identifier; }
      set { SetProperty(ref _Identifier, value); }
    }



    public DocumentSnapshot(TextDocument document, string identifier = null)
    {
      this.Identifier = identifier;
      this.Document = document;
    }
  }
}
