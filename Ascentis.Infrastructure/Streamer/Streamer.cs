using System;

// ReSharper disable once CheckNamespace
namespace Ascentis.Infrastructure
{
    public class Streamer
    {
        private readonly IStreamerFormatter _streamerFormatter;

        public Streamer(IStreamerFormatter streamerFormatter)
        {
            _streamerFormatter = streamerFormatter;
        }

        public void Run(IStreamerAdapter source, object target)
        {
            _streamerFormatter.Prepare(source, target);
            try
            {

                var writingConveyor = new Conveyor<object[]>(row =>
                {
                    _streamerFormatter.Process(row, target);
                    source.ReleaseRow(row);
                });
                writingConveyor.Start();
                var sourceRows = source.GetEnumerable();
                foreach(var row in sourceRows)
                    writingConveyor.InsertPacket(row);

                writingConveyor.StopAndWait();
                _streamerFormatter.UnPrepare(target);
            }
            catch (Exception e)
            {
                _streamerFormatter.AbortedWithException(e);
                throw;
            }
        }
    }
}
