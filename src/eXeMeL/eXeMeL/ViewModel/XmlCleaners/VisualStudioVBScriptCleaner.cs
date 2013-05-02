using System.Linq;
using System.Text.RegularExpressions;

namespace eXeMeL.ViewModel.XmlCleaners
{
  internal class VisualStudioVBScriptCleaner : XmlCleanerBase
  {
    private const string DOUBLE_QUOTE_REGEX = "(=)?(\"\")(\\s)?";
    private const string DOUBLE_QUOTE = "\"\"";
    private const string SINGLE_QUOTE = "\"";

    public override void CleanXml(XmlCleanerContext context)
    {
      context.XmlToClean = Regex.Replace(context.XmlToClean, DOUBLE_QUOTE_REGEX, Evaluator);
    }

    private static string Evaluator(Match match)
    {
      bool allGroupsMatched = match.Groups.Cast<Group>().Aggregate(true, (current, g) => current && g.Success);

      return allGroupsMatched ? match.Value : match.Value.Replace(DOUBLE_QUOTE, SINGLE_QUOTE);
    }
  }
}
