using System;

namespace Ascentis.Infrastructure.DataStreamer
{
    public class DataStreamerException : Exception
    {
        public DataStreamerException(string msg) : base(msg) {}
    }
}
