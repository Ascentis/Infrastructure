using System.Data.Common;
using System.Diagnostics.CodeAnalysis;
using Ascentis.Infrastructure.DataPipeline.SourceAdapter.Sql.Generic;
using Oracle.ManagedDataAccess.Client;

namespace Ascentis.Infrastructure.Sql.DataPipeline.SourceAdapter.Sql.Oracle
{
    [SuppressMessage("ReSharper", "SuggestBaseTypeForParameter")]
    public class OracleSourceAdapter : SourceAdapterSqlBase
    { 
        public OracleSourceAdapter(OracleDataReader oracleDataReader, int rowsPoolCapacity) : base(oracleDataReader, rowsPoolCapacity) { }
        public OracleSourceAdapter(OracleDataReader oracleDataReader) : base(oracleDataReader) { } 
        public OracleSourceAdapter(
            string connectionString,
            string oracleCommandText,
            int rowsPoolCapacity) : base(connectionString, oracleCommandText, rowsPoolCapacity) { }
        public OracleSourceAdapter(
            string connectionString,
            string oracleCommandText) : this(connectionString, oracleCommandText, DefaultRowsCapacity) { }

        public static DbConnection BuildConnection(string connectionString)
        {
            return new OracleConnection(connectionString);
        }

        public static DbCommand BuildCommand(string oracleCommandText, DbConnection connection)
        {
            var oraCmd = new OracleCommand(oracleCommandText, (OracleConnection) connection) {UseEdmMapping = true};
            return oraCmd;
        }
    }
}
