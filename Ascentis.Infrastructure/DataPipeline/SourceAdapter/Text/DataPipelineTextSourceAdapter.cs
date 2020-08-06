using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using Ascentis.Infrastructure.DataPipeline.Exceptions;

namespace Ascentis.Infrastructure.DataPipeline.SourceAdapter.Text
{
    public abstract class DataPipelineTextSourceAdapter : DataPipelineSourceAdapter<object[]>
    {
        protected Regex RegexParser { get; set; }
        private TextToObject[] _textToObjects;

        public const int DefaultRowsPoolCapacity = 1000;

        public int RowsPoolCapacity { get; set; } = DefaultRowsPoolCapacity;

        protected delegate object TextToObject(string text);

        protected TextReader Reader { get; }

        protected Pool<object[]> RowsPool { get; private set; }

        protected DataPipelineTextSourceAdapter(TextReader textReader)
        {
            Reader = textReader;
        }

        protected TextToObject[] BuildConversionArray()
        {
            var result = new TextToObject[ColumnMetadatas.Length];
            for (var i = 0; i < ColumnMetadatas.Length; i++)
            {
                if (ColumnMetadatas[i].DataType == typeof(string))
                    result[i] = text => text.Trim();
                else if (ColumnMetadatas[i].DataType == typeof(char))
                    result[i] = text => text.Trim() != "" ? text.Trim()[0] : ' ';
                else if (ColumnMetadatas[i].DataType == typeof(int))
                    result[i] = text => int.Parse(text.Trim());
                else if (ColumnMetadatas[i].DataType == typeof(short))
                    result[i] = text => short.Parse(text.Trim());
                else if (ColumnMetadatas[i].DataType == typeof(byte))
                    result[i] = text => byte.Parse(text.Trim());
                else if (ColumnMetadatas[i].DataType == typeof(long))
                    result[i] = text => long.Parse(text.Trim());
                else if (ColumnMetadatas[i].DataType == typeof(bool))
                    result[i] = text => bool.Parse(text.Trim());
                else if (ColumnMetadatas[i].DataType == typeof(double))
                    result[i] = text => double.Parse(text.Trim());
                else if (ColumnMetadatas[i].DataType == typeof(float))
                    result[i] = text => float.Parse(text.Trim());
                else if (ColumnMetadatas[i].DataType == typeof(DateTime))
                    result[i] = text => DateTime.Parse(text.Trim());
                else
                    result[i] = text => text.Trim();
            }

            return result;
        }

        public override void ReleaseRow(object[] row)
        {
            RowsPool.Release(row);
        }

        public override void Prepare()
        {
            base.Prepare();
            RowsPool = new Pool<object[]>(RowsPoolCapacity, () => new object[FieldCount]);
            _textToObjects = BuildConversionArray();
        }

        public override int FieldCount {
            get
            {
                ArgsChecker.CheckForNull<NullReferenceException>(ColumnMetadatas, nameof(ColumnMetadatas));
                return ColumnMetadatas.Length;
            }
        }

        public override IEnumerable<object[]> RowsEnumerable
        {
            get
            {
                while (true)
                {
                    var values = RowsPool.Acquire();
                    var s = Reader.ReadLine();
                    try
                    {
                        if (string.IsNullOrEmpty(s))
                            yield break;

                        var match = RegexParser.Match(s);
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
