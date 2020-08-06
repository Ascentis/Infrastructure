using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using Ascentis.Infrastructure.DataPipeline.Exceptions;

namespace Ascentis.Infrastructure.DataPipeline.SourceAdapter.Text
{
    public class DataPipelineFixedLengthSourceAdapter : DataPipelineTextSourceAdapter
    {
        public DataPipelineFixedLengthSourceAdapter(TextReader textReader) : base(textReader) { }

        private Regex _regexParser;
        private TextToObject[] _textToObjects;
        private bool _prepared;

        public override void Prepare()
        {
            ArgsChecker.CheckForNull<NullReferenceException>(ColumnMetadatas, nameof(ColumnMetadatas));

            base.Prepare();

            var regExPattern = "^";
            // ReSharper disable once LoopCanBeConvertedToQuery
            foreach (var columnMeta in ColumnMetadatas)
                regExPattern += $"(.{{{columnMeta.ColumnSize}}})";
            regExPattern += "$";
            _regexParser = new Regex(regExPattern, RegexOptions.Compiled | RegexOptions.CultureInvariant);
            _textToObjects = BuildConversionArray();

            _prepared = true;
        }

        public override void UnPrepare()
        {
            base.UnPrepare();
            _prepared = false;
        }

        public override IEnumerable<object[]> RowsEnumerable
        {
            get
            {
                if (!_prepared)
                    throw new DataPipelineException("DataPipelineFixedLengthSourceAdapter not prepared");
                while(true)
                {
                    var s = Reader.ReadLine();
                    if (string.IsNullOrEmpty(s))
                        yield break;

                    var match = _regexParser.Match(s);
                    if (match.Value == "")
                        throw new DataPipelineException("No match parsing fixed length line");
                    if (match.Groups.Count != FieldCount + 1)
                        throw new DataPipelineException("Number of data elements read in fixed length streamer don't match layout");
                    var values = RowsPool.Acquire();
                    for (var i = 1; i < match.Groups.Count; i++)
                        values[i - 1] = _textToObjects[i - 1](match.Groups[i].Value);

                    yield return values;
                }
            }
        }
    }
}
