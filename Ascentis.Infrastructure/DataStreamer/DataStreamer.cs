using System;

namespace Ascentis.Infrastructure.DataStreamer
{
    public class DataStreamer
    {
        public void Run(IDataStreamerSourceAdapter dataStreamerSourceAdapter, IDataStreamerTargetFormatter dataStreamerTargetFormatter, object target)
        {
            dataStreamerTargetFormatter.Prepare(dataStreamerSourceAdapter, target);
            try
            {
                var writingConveyor = new Conveyor<object[]>(row =>
                {
                    dataStreamerTargetFormatter.Process(row);
                    dataStreamerSourceAdapter.ReleaseRow(row);
                });
                writingConveyor.Start();

                var sourceRows = dataStreamerSourceAdapter.RowsEnumerable;
                foreach(var row in sourceRows)
                    writingConveyor.InsertPacket(row);

                writingConveyor.StopAndWait();
                dataStreamerTargetFormatter.UnPrepare(target);
            }
            catch (Exception e)
            {
                dataStreamerTargetFormatter.AbortedWithException(e);
                throw;
            }
        }
    }
}
