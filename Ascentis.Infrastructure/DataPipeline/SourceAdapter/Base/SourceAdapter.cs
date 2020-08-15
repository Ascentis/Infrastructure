using System;
using System.Collections.Generic;
using Ascentis.Infrastructure.DataPipeline.SourceAdapter.Utils;

namespace Ascentis.Infrastructure.DataPipeline.SourceAdapter.Base
{
    public abstract class SourceAdapter
    {
        protected Dictionary<string, int> ColumnMetadatasMap;
        public bool AbortOnReadException { get; set; }
        public virtual ColumnMetadataList ColumnMetadatas { get; set; }

        public virtual int FieldCount
        {
            get
            {
                ArgsChecker.CheckForNull<NullReferenceException>(ColumnMetadatas, nameof(ColumnMetadatas));
                return ColumnMetadatas.Count;
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
                for (var i = 0; i < ColumnMetadatas.Count; i++)
                    ColumnMetadatasMap.Add(ColumnMetadatas[i].ColumnName, i);
                return ColumnMetadatasMap;
            }
        }

        public int Id { get; set; }
    }
}
