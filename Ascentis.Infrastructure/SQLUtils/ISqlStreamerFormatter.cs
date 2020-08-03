using System;
using System.Data.SqlClient;
using System.IO;

// ReSharper disable once CheckNamespace
namespace Ascentis.Infrastructure
{
    public interface ISqlStreamerFormatter
    {
        void Prepare(SqlDataReader reader, Stream stream);
        void Process(object[] row, Stream stream);
        void UnPrepare(Stream stream);
        void AbortedWithException(Exception e);
    }
}
