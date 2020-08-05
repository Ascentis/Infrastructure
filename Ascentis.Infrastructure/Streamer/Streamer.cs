using System;

// ReSharper disable once CheckNamespace
namespace Ascentis.Infrastructure
{
    public class Streamer
    {
        public void Run(IStreamerSourceAdapter source, IStreamerTargetFormatter streamerTargetFormatter, object target)
        {
            streamerTargetFormatter.Prepare(source, target);
            try
            {
                var writingConveyor = new Conveyor<object[]>(row =>
                {
                    streamerTargetFormatter.Process(row, target);
                    source.ReleaseRow(row);
                });
                writingConveyor.Start();
                var sourceRows = source.GetRowsEnumerable();
                foreach(var row in sourceRows)
                    writingConveyor.InsertPacket(row);

                writingConveyor.StopAndWait();
                streamerTargetFormatter.UnPrepare(target);
            }
            catch (Exception e)
            {
                streamerTargetFormatter.AbortedWithException(e);
                throw;
            }
        }
    }
}
