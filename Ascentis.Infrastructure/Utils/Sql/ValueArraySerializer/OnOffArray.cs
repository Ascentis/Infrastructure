using System.Collections;

namespace Ascentis.Infrastructure.Utils.Sql.ValueArraySerializer
{
    internal class OnOffArray : IOnOffArray
    {
        private readonly BitArray _flags;

        public OnOffArray(int count)
        {
            _flags = new BitArray(count, true);
        }

        public bool this[int index]
        {
            get => _flags[index];
            set => _flags[index] = value;
        }

        public int Count => _flags.Count;
    }
}
