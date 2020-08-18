using System;
using System.Data.Common;

namespace Ascentis.Infrastructure.DataPipeline.SourceAdapter.Sql.Generic
{
    // ReSharper disable once InconsistentNaming
    public class IClassSqlBuilder : ClassInterface
    {
        public IClassSqlBuilder(Type targetType) : base(targetType) {}

        public delegate DbConnection BuildConnectionDelegate(string connectionString);
        public delegate DbCommand BuildCommandDelegate(string sqlCommandText, DbConnection connection);

        public BuildConnectionDelegate BuildConnection { get; protected set; }
        public BuildCommandDelegate BuildCommand { get; protected set; }
    }
}
