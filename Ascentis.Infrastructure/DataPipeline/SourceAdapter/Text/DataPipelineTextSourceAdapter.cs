using System;
using System.Collections.Generic;
using System.IO;

namespace Ascentis.Infrastructure.DataPipeline.SourceAdapter.Text
{
    public abstract class DataPipelineTextSourceAdapter : IDataPipelineSourceAdapter<object[]>
    {
        protected TextReader Reader { get; }

        protected DataPipelineTextSourceAdapter(TextReader textReader)
        {
            Reader = textReader;
        }

        public void ReleaseRow(object[] row)
        {
        }

        public abstract IEnumerable<object[]> RowsEnumerable { get; }

        public int FieldCount {
            get
            {
                ArgsChecker.CheckForNull<NullReferenceException>(ColumnMetadatas, nameof(ColumnMetadatas));
                return ColumnMetadatas.Length;
            }
        }

        public DataPipelineColumnMetadata[] ColumnMetadatas { get; set; }
    }
}
