using System;

// ReSharper disable once CheckNamespace
namespace Ascentis.Infrastructure
{
    public class SqlStreamerFormatterException : Exception
    {
        public SqlStreamerFormatterException(string msg) : base(msg) {}
    }
}
