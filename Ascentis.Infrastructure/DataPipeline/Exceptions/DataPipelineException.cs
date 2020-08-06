using System;

namespace Ascentis.Infrastructure.DataPipeline.Exceptions
{
    public class DataPipelineException : Exception
    {
        public DataPipelineException(string msg) : base(msg) {}
    }
}
