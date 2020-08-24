using System.Collections;

namespace Ascentis.Infrastructure.Utils.Sql.ValueArraySerializer
{
    public class OnOffArray : IOnOffArray
    {
        private readonly BitArray _flags;

        public int DisabledCount { get; private set; }

        public OnOffArray(int count)
        {
            _flags = new BitArray(count, true);
        }

        public bool this[int index]
        {
            get => _flags[index];
            set
            {
                if (value == _flags[index])
                    return;
                if (!value)
                    DisabledCount++;
                else
                    DisabledCount--;

                _flags[index] = value;
            }
        }

        public int Count => _flags.Count;
    }
}
