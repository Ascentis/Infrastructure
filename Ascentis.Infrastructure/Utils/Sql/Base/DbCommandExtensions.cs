using System.Collections.Generic;
using System.Data.Common;
using System.Text.RegularExpressions;

// ReSharper disable once CheckNamespace
namespace Ascentis.Infrastructure
{
    public static class DbCommandExtensions
    {
        public static IList<string> ParseParameters(this DbCommand cmd)
        {
            const string rxPattern = @"[@|:](?:[\w#_$]{1,128}|(?:(\[)|"").{1,128}?(?(1)]|""))";

            var parameterMatches = Regex.Matches(cmd.CommandText, rxPattern, RegexOptions.Compiled);
            var list = new List<string>();
            for (var i = 0; i < parameterMatches.Count; i++)
                list.Add(parameterMatches[i].Value.Remove(0, 1));

            return list;
        }
    }
}
