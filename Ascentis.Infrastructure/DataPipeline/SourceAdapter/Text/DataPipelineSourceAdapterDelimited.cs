using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using Ascentis.Infrastructure.DataPipeline.Exceptions;

namespace Ascentis.Infrastructure.DataPipeline.SourceAdapter.Text
{
    public class DataPipelineSourceAdapterDelimited : DataPipelineSourceAdapterText
    {
        public const string DefaultDelimiter = ",";

        private bool _prepared;

        public string Delimiter { get; set; } = DefaultDelimiter;

        public DataPipelineSourceAdapterDelimited(TextReader textReader) : base(textReader) { }

        public override void Prepare()
        {
            ArgsChecker.CheckForNull<NullReferenceException>(ColumnMetadatas, nameof(ColumnMetadatas));

            base.Prepare();

            var regExPattern = "^";
            var currentColumnId = 0;
            while (currentColumnId++ < ColumnMetadatas.Length)
                regExPattern += $"(.*){Delimiter}";
            regExPattern = regExPattern.Remove(regExPattern.Length - Delimiter.Length, Delimiter.Length);
            regExPattern += "$";
            RegexParser = new Regex(regExPattern, RegexOptions.Compiled | RegexOptions.CultureInvariant);

            _prepared = true;
        }

        public override void UnPrepare()
        {
            base.UnPrepare();
            _prepared = false;
        }

        public override IEnumerable<PoolEntry<object[]>> RowsEnumerable
        {
            get
            {
                if (!_prepared)
                    throw new DataPipelineException($"{GetType().Name} not prepared");
                return base.RowsEnumerable;
            }
        }
    }
}
