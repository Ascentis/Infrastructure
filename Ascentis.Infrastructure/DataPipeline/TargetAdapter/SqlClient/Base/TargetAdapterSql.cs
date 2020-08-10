using System.Collections.Generic;
using Ascentis.Infrastructure.DataPipeline.TargetAdapter.Generic;

namespace Ascentis.Infrastructure.DataPipeline.TargetAdapter.SqlClient.Base
{
    public abstract class TargetAdapterSql : TargetAdapter<PoolEntry<object[]>>
    {
        public IEnumerable<string> AnsiStringParameters {get; set;}
        public bool UseTakeSemantics { get; set; }
    }
}
