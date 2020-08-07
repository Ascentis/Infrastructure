using System.Data;
using System.Data.SqlClient;

namespace Ascentis.Infrastructure.DataPipeline.TargetAdapter.SqlClient
{
    public class DataPipelineTargetAdapterSql : DataPipelineTargetAdapter<PoolEntry<object[]>>
    {
        private readonly SqlCommand _cmd;

        public DataPipelineTargetAdapterSql(SqlCommand cmd)
        {
            _cmd = cmd;
        }

        public override void Prepare(IDataPipelineSourceAdapter<PoolEntry<object[]>> source)
        {
            base.Prepare(source);

            for (var i = 0; i < source.FieldCount; i++)
            {
                var param = _cmd.Parameters.Add(source.ColumnMetadatas[i].ColumnName, TypeToSqlDbType.From(source.ColumnMetadatas[i].DataType));
                switch (param.DbType)
                {
                    case DbType.AnsiString:
                    case DbType.AnsiStringFixedLength:
                    case DbType.Binary:
                    case DbType.String:
                    case DbType.StringFixedLength:
                    case DbType.Xml:
                        param.Size = source.ColumnMetadatas[i].ColumnSize??0;
                        break;
                    case DbType.Decimal:
                        param.Precision = (byte) (source.ColumnMetadatas[i].NumericPrecision??0);
                        param.Scale = (byte) (source.ColumnMetadatas[i].NumericScale??0);
                        break;
                }
            }
            _cmd.Prepare();
        }

        public override void Process(PoolEntry<object[]> row)
        {
            for (var i = 0; i < Source.FieldCount; i++)
                _cmd.Parameters[i].Value = row.Value[i];
            _cmd.ExecuteNonQuery();
        }
    }
}
