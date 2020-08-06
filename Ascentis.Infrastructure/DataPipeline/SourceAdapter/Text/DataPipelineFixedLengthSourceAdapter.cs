using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
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

        [SuppressMessage("ReSharper", "PossibleInvalidOperationException")]
        public override void Prepare()
        {
            ArgsChecker.CheckForNull<NullReferenceException>(ColumnMetadatas, nameof(ColumnMetadatas));

            base.Prepare();

            var regExPattern = "^";
            var prevPosition = 0;
            var prevColumnSize = 0;

            foreach (var columnMeta in ColumnMetadatas)
            {
                ArgsChecker.CheckForNull<NullReferenceException>(columnMeta.ColumnSize, nameof(columnMeta.ColumnSize));

                var currentPosition = columnMeta.StartPosition ?? prevPosition + prevColumnSize;
                if (currentPosition < prevPosition + prevColumnSize)
                    throw new DataPipelineException("Field position can't be < than previous field position + previous column size");
                if (currentPosition > prevPosition + prevColumnSize)
                    regExPattern += $".{{{currentPosition - prevPosition - prevColumnSize}}}";
                regExPattern += $"(.{{{columnMeta.ColumnSize}}})";
                prevPosition = currentPosition;
                prevColumnSize = (int) columnMeta.ColumnSize;
            }

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
                while (true)
                {
                    var values = RowsPool.Acquire();
                    var s = Reader.ReadLine();
                    try
                    {
                        if (string.IsNullOrEmpty(s))
                            yield break;

                        var match = _regexParser.Match(s);
                        if (match.Value == "")
                            throw new DataPipelineException("No match parsing fixed length line");
                        if (match.Groups.Count != FieldCount + 1)
                            throw new DataPipelineException("Number of individual data elements read in fixed length streamer line don't match layout");
                        for (var i = 1; i < match.Groups.Count; i++)
                            values[i - 1] = _textToObjects[i - 1](match.Groups[i].Value);
                    }
                    catch (Exception e)
                    {
                        InvokeRowReadErrorEvent(s, e);
                        if (AbortOnReadException)
                            throw;
                        continue;
                    }
                    yield return values;
                }
            }
        }
    }
}
