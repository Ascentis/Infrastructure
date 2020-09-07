using System.Collections.Generic;
using System.Data.SQLite;
using Ascentis.Infrastructure.DataPipeline.TargetAdapter.Sql.Generic;
using Ascentis.Infrastructure.DataPipeline.TargetAdapter.Sql.Utils;
using Ascentis.Infrastructure.Sql.DataPipeline.TargetAdapter.Sql.SQLite.Utils;

namespace Ascentis.Infrastructure.Sql.DataPipeline.TargetAdapter.Sql.SQLite.Single
{
    // ReSharper disable once InconsistentNaming
    public class SQLiteTargetAdapterCommand : TargetAdapterSqlBase<SQLiteCommand, SQLiteTransaction, SQLiteConnection> 
    {
        private static readonly ColumnMetadataToSQLiteDbTypeMapper ParamMapper = new ColumnMetadataToSQLiteDbTypeMapper();

        public SQLiteTargetAdapterCommand(SQLiteCommand cmd) : base(cmd) { }

        protected override IList<string> ParseParameters()
        {
            return Cmd.ParseParameters();
        }

        protected override void MapParams(IEnumerable<string> columnNames, Dictionary<string, int> paramToMetaIndex)
        {
            var metaToParamSettings = new MetaToParamSettings
            {
                Columns = columnNames,
                Metadatas = Source.ColumnMetadatas,
                AnsiStringParameters = AnsiStringParameters,
                ColumnToIndexMap = paramToMetaIndex,
                Target = Cmd.Parameters,
                UseShortParam = false
            };
            ParamMapper.Map(metaToParamSettings);
        }

        public override object GetNativeValue(object value)
        {
            return SQLiteUtils.GetNativeValue(value);
        }
    }
}
