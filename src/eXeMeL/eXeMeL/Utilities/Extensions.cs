using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eXeMeL.Utilities
{
  public static class Extensions
  {
    public static TimeSpan Milliseconds(this int value)
    {
      return TimeSpan.FromMilliseconds(value);
    }
  }
}
