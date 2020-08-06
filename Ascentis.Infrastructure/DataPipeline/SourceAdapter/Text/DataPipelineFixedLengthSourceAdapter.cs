using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using Ascentis.Infrastructure.DataPipeline.Exceptions;

namespace Ascentis.Infrastructure.DataPipeline.SourceAdapter.Text
{
    public class DataPipelineFixedLengthSourceAdapter : DataPipelineTextSourceAdapter
    {
        public DataPipelineFixedLengthSourceAdapter(TextReader textReader) : base(textReader)
        {
        }

        public override IEnumerable<object[]> RowsEnumerable
        {
            get
            {
                var regExPattern = "^";
                foreach (var columnMeta in ColumnMetadatas)
                    regExPattern += $"(.{{{columnMeta.ColumnSize}}})";
                regExPattern += "$";
                var regEx = new Regex(regExPattern, RegexOptions.Compiled | RegexOptions.CultureInvariant);
                while(true)
                {
                    var s = Reader.ReadLine();
                    if (string.IsNullOrEmpty(s))
                        yield break;
                    var match = regEx.Match(s);
                    if (match.Value == "")
                        throw new DataPipelineException("No match parsing fixed length line");
                    if (match.Groups.Count != FieldCount + 1)
                        throw new DataPipelineException("Number of data elements read in fixed length streamer doesn't match layout");
                    var values = new object[FieldCount];
                    for (var i = 1; i < match.Groups.Count; i++)
                        if (ColumnMetadatas[i - 1].DataType == typeof(int))
                            values[i - 1] = int.Parse(match.Groups[i].Value.Trim());
                        else
                            values[i - 1] = match.Groups[i];
                    yield return values;
                }
            }
        }
    }
}
