namespace Ascentis.Infrastructure.DataPipeline.Exceptions.DataPipelineComparer
{
    public class DataPipelineComparerDataMismatch : DataPipelineComparerException
    {
        public object FirstValue { get; }
        public object SecondValue { get; }
        public string FieldName { get; }

        public DataPipelineComparerDataMismatch(object firstValue, object secondValue, string fieldName) : base(
            $"First value {firstValue.GetType().Name}:'{firstValue}' different than {secondValue.GetType().Name}:'{secondValue}'. FieldName: {fieldName}")
        {
            FirstValue = firstValue;
            SecondValue = secondValue;
            FieldName = fieldName;
        }
    }
}
