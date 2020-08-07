using System;
using System.Collections.Generic;

namespace Ascentis.Infrastructure.DataPipeline.SourceAdapter
{
    public abstract class DataPipelineSourceAdapter<T> : IDataPipelineSourceAdapter<T>
    {
        private IEnumerable<T> _rowsEnumerable;
        private Dictionary<string, int> _columnMetadatasMap;
        private DataPipelineColumnMetadata[] _dataPipelineColumnMetadata;

        public event DataPipeline<T>.RowErrorDelegate OnSourceAdapterRowReadError;
        public bool AbortOnReadException { get; set; }
        public abstract int FieldCount { get; }
        public int ParallelLevel { get; set; }

        public virtual DataPipelineColumnMetadata[] ColumnMetadatas
        {
            get => _dataPipelineColumnMetadata;
            set
            {
                if (_dataPipelineColumnMetadata == value)
                    return;
                _dataPipelineColumnMetadata = value;
                _columnMetadatasMap = null;
            }
        }

        protected void InvokeRowReadErrorEvent(object sourceData, Exception e)
        {
            OnSourceAdapterRowReadError?.Invoke(sourceData, e);
        }

        public virtual void UnPrepare() { }
        public virtual void ReleaseRow(T row) { }
        public abstract IEnumerable<T> RowsEnumerable { get; }
        public virtual void Prepare() { }

        public Dictionary<string, int> MetadatasColumnToIndexMap
        {
            get
            {
                if (_columnMetadatasMap != null)
                    return _columnMetadatasMap;
                _columnMetadatasMap = new Dictionary<string, int>();
                for (var i = 0; i < ColumnMetadatas.Length; i++)
                    _columnMetadatasMap.Add(ColumnMetadatas[i].ColumnName, i);
                return _columnMetadatasMap;
            }
        }
    }
}
