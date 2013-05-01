using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eXeMeL.Messages
{
  public class DisplayApplicationStatusMessage
  {
    public string NewStatus { get; private set; }

    public DisplayApplicationStatusMessage(string message)
    {
      this.NewStatus = message;
    }
  }
}
