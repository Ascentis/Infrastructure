using System;
using System.Data.Common;

namespace Ascentis.Infrastructure.DataPipeline.SourceAdapter.Sql.Generic
{
    /*
       This class represents an static "interface" providing sql command and connection
       building capabilities.

       Overloaded cast operator from Type reference added to mimic process of obtaining
       an interface from an object typically done by casting the object to the desired
       interface. Here we "cast" from the class Type reference to the class IClassSqlBuilder.
       The operation will create an instance of the class IClassSqlBuilder ready to be used
       to access the BuildConnection() and BuildCommand() static methods in the target class.

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
