using System;
using System.Collections.Generic;
using System.Data.Common;
using Ascentis.Infrastructure.DataPipeline.TargetAdapter.Base;

namespace Ascentis.Infrastructure.DataPipeline.TargetAdapter.Sql.Generic
{
    public abstract class TargetAdapterSqlBase<TCmd, TTran, TCon> : TargetAdapterSql 
        where TCmd : DbCommand 
        where TTran : DbTransaction 
        where TCon : DbConnection
    {
        protected readonly TCmd Cmd;
        private int[] _paramToMetaMap;

        protected TargetAdapterSqlBase(TCmd cmd)
        {
            Cmd = cmd;
        }

        public virtual TTran Transaction
        {
            get => (TTran)Cmd.Transaction;
            set => Cmd.Transaction = value;
        }

        public virtual TCon Connection => (TCon)Cmd.Connection;

        protected abstract IList<string> ParseParameters();

        protected abstract void MapParams(Dictionary<string, int> paramToMetaIndex);

        public override void Prepare(ISourceAdapter<PoolEntry<object[]>> source)
        {
            base.Prepare(source);

            var parameters = ParseParameters();
            var paramToMetaIndex = new Dictionary<string, int>();
            _paramToMetaMap = new int[parameters.Count];
            var i = 0;
            foreach (var parameter in parameters)
            {
                var metaIndex = Source.MetadatasColumnToIndexMap.TryGetValue(parameter, out var index) ? index : -1;
                paramToMetaIndex.Add(parameter, metaIndex);
                _paramToMetaMap[i++] = metaIndex;
            }

            MapParams(paramToMetaIndex);
            Cmd.Prepare();
        }

        public override void Process(PoolEntry<object[]> row)
        {
            if (InvokeBeforeTargetAdapterProcessRowEvent(row) == BeforeProcessRowResult.Abort)
                return;

            if (UseTakeSemantics && !row.Take())
                return;

            for (var i = 0; i < Cmd.Parameters.Count; i++)
                Cmd.Parameters[i].Value = row.Value[_paramToMetaMap[i]];
            try
            {
                Cmd.ExecuteNonQuery();
                InvokeAfterTargetAdapterProcessRowEvent(row);
            }
            catch (Exception e)
            {
                if (AbortOnProcessException ?? false)
                    throw;
                InvokeProcessErrorEvent(row, e);
            }
        }
    }
}
