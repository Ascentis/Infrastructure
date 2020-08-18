using System;
using System.Data.Common;

namespace Ascentis.Infrastructure.DataPipeline.SourceAdapter.Sql.Generic
{
    /*
       This class represents an static "interface" providing sql command and connection
       building capabilities.

       See comments in base class ClassInterface
    */

    // ReSharper disable once InconsistentNaming
    public class IClassSqlBuilder : ClassInterface
    {
        public IClassSqlBuilder(Type targetType) : base(targetType) {}

        public static implicit operator IClassSqlBuilder(Type targetType)
        {
            return new IClassSqlBuilder(targetType);
        }

        public static implicit operator IClassSqlBuilder(SourceAdapterSqlBase obj)
        {
            return obj.GetType();
        }

        public delegate DbConnection BuildConnectionDelegate(string connectionString);
        public delegate DbCommand BuildCommandDelegate(string sqlCommandText, DbConnection connection);

        public BuildConnectionDelegate BuildConnection { get; protected set; }
        public BuildCommandDelegate BuildCommand { get; protected set; }
    }
}
