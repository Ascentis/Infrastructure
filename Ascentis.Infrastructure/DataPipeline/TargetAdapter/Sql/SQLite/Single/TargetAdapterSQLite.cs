using System.Collections.Generic;
using System.Data.SQLite;
using Ascentis.Infrastructure.DataPipeline.TargetAdapter.Sql.Generic;
using Ascentis.Infrastructure.DataPipeline.TargetAdapter.Sql.SQLite.Utils;

namespace Ascentis.Infrastructure.DataPipeline.TargetAdapter.Sql.SQLite.Single
{
    // ReSharper disable once InconsistentNaming
    public class TargetAdapterSQLite : TargetAdapterSqlBase<SQLiteCommand, SQLiteTransaction, SQLiteConnection> 
    {
        private static readonly ColumnMetadataToSQLiteDbTypeMapper ParamMapper = new ColumnMetadataToSQLiteDbTypeMapper();

        public TargetAdapterSQLite(SQLiteCommand cmd) : base(cmd) { }

        protected override IList<string> ParseParameters()
        {
            return Cmd.ParseParameters();
        }

        protected override void MapParams(Dictionary<string, int> paramToMetaIndex)
        {
            ParamMapper.Map(paramToMetaIndex, Source.ColumnMetadatas, AnsiStringParameters, Cmd.Parameters);
        }
    }
}
