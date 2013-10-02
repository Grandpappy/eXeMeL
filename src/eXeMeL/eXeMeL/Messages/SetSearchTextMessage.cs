using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eXeMeL.Messages
{
  public class SetSearchTextMessage
  {
    public string SearchText { get; private set; }

    public SetSearchTextMessage(string findText)
    {
      this.SearchText = findText;
    }
  }
}
