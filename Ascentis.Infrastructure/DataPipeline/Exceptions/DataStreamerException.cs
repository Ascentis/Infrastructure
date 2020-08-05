using System;

namespace Ascentis.Infrastructure.DataStreamer.Exceptions
{
    public class DataStreamerException : Exception
    {
        public DataStreamerException(string msg) : base(msg) {}
    }
}
