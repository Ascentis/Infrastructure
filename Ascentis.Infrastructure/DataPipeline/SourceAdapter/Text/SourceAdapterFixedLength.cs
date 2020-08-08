using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text.RegularExpressions;
using Ascentis.Infrastructure.DataPipeline.Exceptions;

namespace Ascentis.Infrastructure.DataPipeline.SourceAdapter.Text
{
    public class SourceAdapterFixedLength : SourceAdapterText
    {
        private bool _prepared;

        public SourceAdapterFixedLength(TextReader textReader) : base(textReader) { }

        [SuppressMessage("ReSharper", "PossibleInvalidOperationException")]
        public override void Prepare()
        {
            ArgsChecker.CheckForNull<NullReferenceException>(ColumnMetadatas, nameof(ColumnMetadatas));

            base.Prepare();

            var regExPattern = "^";
            var prevPosition = 0;
            var prevColumnSize = 0;
            var currentColumnId = 0;

            foreach (var columnMeta in ColumnMetadatas)
            {
                ArgsChecker.CheckForNull<NullReferenceException>(columnMeta.ColumnSize, $"{nameof(columnMeta.ColumnSize)}[{currentColumnId}]");

                var currentPosition = columnMeta.StartPosition ?? prevPosition + prevColumnSize;
                if (currentPosition < prevPosition + prevColumnSize)
                    throw new DataPipelineException("Field position can't be < than previous field position + previous column size");
                if (currentPosition > prevPosition + prevColumnSize)
                    regExPattern += $".{{{currentPosition - prevPosition - prevColumnSize}}}";
                regExPattern += $"(.{{{columnMeta.ColumnSize}}})";
                prevPosition = currentPosition;
                prevColumnSize = (int) columnMeta.ColumnSize;
                currentColumnId++;
            }

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
