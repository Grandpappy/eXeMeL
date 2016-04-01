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


    /// <summary>
    /// Example: string description = targetLevel.GetAttributeValue<DescriptionAttribute, string>(x => x.Description);
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="Expected"></typeparam>
    /// <param name="enumeration"></param>
    /// <param name="expression"></param>
    /// <returns></returns>
    public static Expected GetAttributeValue<T, Expected>(this Enum enumeration, Func<T, Expected> expression)
        where T : Attribute
    {
      T attribute = enumeration.GetType().GetMember(enumeration.ToString())[0].GetCustomAttributes(typeof(T), false).Cast<T>().SingleOrDefault();

      if (attribute == null)
        return default(Expected);

      return expression(attribute);
    }



    public static IEnumerable<T> GetAttributes<T>(this Enum enumeration)
        where T : Attribute
    {
      return enumeration.GetType().GetMember(enumeration.ToString())[0].GetCustomAttributes(typeof(T), false).Cast<T>();
    }
  }
}
