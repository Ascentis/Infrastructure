using Ascentis.Infrastructure.DataPipeline.TargetAdapter.Generic;

namespace Ascentis.Infrastructure.DataPipeline.TargetAdapter.PassThru
{
    public class PassThruTargetAdapter<TRow> : TargetAdapter<TRow>
    {
        public event DataPipeline<TRow>.RowDelegate OnProcess;

        public PassThruTargetAdapter(DataPipeline<TRow>.RowDelegate onProcess)
        {
            OnProcess = onProcess;
        }

        public override void Process(TRow row)
        {
            InvokeBeforeTargetAdapterProcessRowEvent(row); 
            OnProcess?.Invoke(this, row);
            InvokeAfterTargetAdapterProcessRowEvent(row);
        }
    }
}
