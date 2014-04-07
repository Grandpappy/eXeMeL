using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using ICSharpCode.AvalonEdit.Document;

namespace eXeMeL.ViewModel
{
  public class DocumentSnapshot : ViewModelBase
  {
    public TextDocument Document { get; set; }
    private string _Identifier;
    public string Identifier
    {
      get { return _Identifier; }
      set { this.Set(() => Identifier, ref _Identifier, value); }
    }



    public DocumentSnapshot(TextDocument document, string identifier = null)
    {
      this.Identifier = identifier;
      this.Document = document;
    }
  }
}
