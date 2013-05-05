using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eXeMeL.Messages
{
  public class SelectTextInEditorMessage
  {
    public int Index { get; set; }
    public int Length { get; set; }



    public SelectTextInEditorMessage(int index, int length)
    {
      this.Index = index;
      this.Length = length;
    }
  }
}
