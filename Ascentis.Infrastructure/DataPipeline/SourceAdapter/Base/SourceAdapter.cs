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

        public virtual string DownConvertToText(object obj)
        {
            /* This down-conversion use the data-types supported by SQLite. Can't dummy down things more than this */
            return obj switch
            {
                string s => s,
                DateTime dateTime => $"{dateTime:yyyy-MM-dd HH:mm:ss.FFFFFFF}",
                DateTimeOffset dateTimeOffset => $"{dateTimeOffset:yyyy-MM-dd HH:mm:ss.FFFFFFFzzz}",
                TimeSpan timeSpan => $"{timeSpan:d.hh:mm:ss.fffffff}",
                bool b => $"{(b ? 1 : 0)}",
                Guid guid => $"{guid:00000000-0000-0000-0000-000000000000}",
                _ => obj is DBNull ? "" : obj.ToString()
            };
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

        public virtual IEnumerable<string> ColumnNames()
        {
            if (ColumnMetadatas == null)
                throw new InvalidOperationException("ColumnMetadatas is null trying to enumerate column names");

            foreach (var meta in ColumnMetadatas)
                yield return meta.ColumnName;
        }
    }
}
