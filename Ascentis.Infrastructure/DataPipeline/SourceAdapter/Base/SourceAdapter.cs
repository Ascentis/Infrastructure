using System;
using System.Collections.Generic;
using Ascentis.Infrastructure.DataPipeline.SourceAdapter.Utils;

namespace Ascentis.Infrastructure.DataPipeline.SourceAdapter.Base
{
    public abstract class SourceAdapter
    {
        protected Dictionary<string, int> ColumnMetadatasMap;
        protected ColumnMetadata[] DataPipelineColumnMetadata;
        public bool AbortOnReadException { get; set; }

        public virtual ColumnMetadata[] ColumnMetadatas
        {
            get => DataPipelineColumnMetadata;
            set
            {
                if (DataPipelineColumnMetadata == value)
                    return;
                DataPipelineColumnMetadata = value;
                ColumnMetadatasMap = null;
            }
        }

        public virtual int FieldCount
        {
            get
            {
                ArgsChecker.CheckForNull<NullReferenceException>(ColumnMetadatas, nameof(ColumnMetadatas));
                return ColumnMetadatas.Length;
            }
        }

        public int ParallelLevel { get; set; }
        public virtual int RowsPoolSize
        {
            get => 0;
            set => throw new NotImplementedException();
        }
        
        public virtual void UnPrepare() { }

        public virtual void Prepare() { }

        public Dictionary<string, int> MetadatasColumnToIndexMap
        {
            get
            {
                if (ColumnMetadatasMap != null)
                    return ColumnMetadatasMap;
                ColumnMetadatasMap = new Dictionary<string, int>();
                for (var i = 0; i < ColumnMetadatas.Length; i++)
                    ColumnMetadatasMap.Add(ColumnMetadatas[i].ColumnName, i);
                return ColumnMetadatasMap;
            }
        }

        public string Id { get; set; }
    }
}
