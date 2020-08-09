using System;
using System.Collections.Generic;

namespace Ascentis.Infrastructure.DataPipeline.SourceAdapter
{
    public abstract class SourceAdapter<T> : ISourceAdapter<T>
    {
        private IEnumerable<T> _rowsEnumerable;
        private Dictionary<string, int> _columnMetadatasMap;
        private ColumnMetadata[] _dataPipelineColumnMetadata;

        public event DataPipeline<T>.RowErrorDelegate OnSourceAdapterRowReadError;
        public bool AbortOnReadException { get; set; }

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

        public virtual ColumnMetadata[] ColumnMetadatas
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
            OnSourceAdapterRowReadError?.Invoke(this, sourceData, e);
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

        public string Id { get; set; }
    }
}
