namespace Ascentis.Infrastructure.DataPipeline.Exceptions.DataPipelineComparer
{
    public class DataPipelineComparerDataMismatch : DataPipelineComparerException
    {
        public object FirstValue { get; }
        public object SecondValue { get; }

        public DataPipelineComparerDataMismatch(object firstValue, object secondValue) : base(
            $"First value {firstValue} different than {secondValue}")
        {
            FirstValue = firstValue;
            SecondValue = secondValue;
        }
    }
}
