using System;

namespace Ascentis.Infrastructure.DataPipeline.Exceptions
{
    public class DataPipelineSourceAdapterException : DataPipelineException
    {
        public object SourceData { get; }
        public Exception InnerException { get;  }

        public DataPipelineSourceAdapterException(object sourceData, Exception innerException) : base("Data pipeline source adapter error")
        {
            SourceData = sourceData;
            InnerException = innerException;
        }
    }
}
