namespace Ascentis.Infrastructure.DataPipeline.Exceptions.DataPipelineComparer
{
    public class DataPipelineComparerColumnCountMismatch : DataPipelineComparerException
    {
        public int ColumnCount1 { get; }
        public int ColumnCount2 { get; }

        public DataPipelineComparerColumnCountMismatch(int columnCount1, int columnCount2) : base(
            $"Column count mismatch. First adapter had {columnCount1} and second had {columnCount2}")
        {
            ColumnCount1 = columnCount1;
            ColumnCount2 = columnCount2;
        }
    }
}
