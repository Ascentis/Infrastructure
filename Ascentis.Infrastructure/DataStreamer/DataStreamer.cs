using System;
using System.Data;

namespace Ascentis.Infrastructure.DataStreamer
{
    public class DataStreamer<TTarget, TRow>
    {
        public void Run(IDataStreamerSourceAdapter<TRow> dataStreamerSourceAdapter, IDataStreamerTargetFormatter<TTarget, TRow> dataStreamerTargetFormatter, TTarget target)
        {
            dataStreamerTargetFormatter.Prepare(dataStreamerSourceAdapter, target);
            try
            {
                var targetFormatterConveyor = new Conveyor<TRow>(row =>
                {
                    dataStreamerTargetFormatter.Process(row);
                    dataStreamerSourceAdapter.ReleaseRow(row);
                });
                targetFormatterConveyor.Start();

                var sourceRows = dataStreamerSourceAdapter.RowsEnumerable;
                foreach(var row in sourceRows)
                    targetFormatterConveyor.InsertPacket(row);

                targetFormatterConveyor.StopAndWait();
                dataStreamerTargetFormatter.UnPrepare();
            }
            catch (Exception e)
            {
                dataStreamerTargetFormatter.AbortedWithException(e);
                throw;
            }
        }
    }
}
