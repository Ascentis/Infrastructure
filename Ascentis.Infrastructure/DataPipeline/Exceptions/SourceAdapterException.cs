using System;

namespace Ascentis.Infrastructure.DataPipeline.Exceptions
{
    public class SourceAdapterException : DataPipelineException
    {
        public object SourceData { get; }
        public Exception InnerException { get;  }

        public SourceAdapterException(object sourceData, string msg, Exception innerException = null) : base(msg)
        {
            SourceData = sourceData;
            InnerException = innerException;
        }
    }
}
