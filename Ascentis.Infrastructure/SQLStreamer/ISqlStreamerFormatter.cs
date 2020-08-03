using System;
using System.Data.SqlClient;
using System.IO;

// ReSharper disable once CheckNamespace
namespace Ascentis.Infrastructure
{
    public interface ISqlStreamerFormatter
    {
        public void Prepare(SqlDataReader reader, Stream stream);
        public void Process(object[] row, Stream stream);
        public void UnPrepare(Stream stream);
        public void AbortedWithException(Exception e);
    }
}
