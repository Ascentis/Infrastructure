using System;
using System.Collections.Generic;
using System.Data.Common;
using Ascentis.Infrastructure.DataPipeline.TargetAdapter.Generic;

namespace Ascentis.Infrastructure.DataPipeline.TargetAdapter.Base
{
    public abstract class TargetAdapterSql : TargetAdapter<PoolEntry<object[]>>
    {
        public event CommandDelegate BeforeCommandPrepare;
        public IEnumerable<string> AnsiStringParameters {get; set;}
        public bool UseTakeSemantics { get; set; }
        public bool UseNativeTypeConvertor { get; set; }

        public abstract DbCommand TakeCommand();

        public virtual object GetNativeValue(object value)
        {
            return value;
        }

        protected object SourceValueToParamValue(int columnIndex, IReadOnlyList<object> row)
        {
            var value = columnIndex >= 0 ? UseNativeTypeConvertor ? GetNativeValue(row[columnIndex]) : row[columnIndex] : null;
            return value ?? DBNull.Value;
        }

        protected void InvokeBeforeCommandPrepare(DbCommand cmd)
        {
            BeforeCommandPrepare?.Invoke(this, cmd);
        }
    }
}
