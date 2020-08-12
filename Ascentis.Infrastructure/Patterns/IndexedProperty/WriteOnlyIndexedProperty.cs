using System;

// ReSharper disable once CheckNamespace
namespace Ascentis.Infrastructure
{
    public class WriteOnlyIndexedProperty<TIndex, TValue>
    {
        readonly Action<TIndex, TValue> _setAction;

        public WriteOnlyIndexedProperty(Action<TIndex, TValue> setAction)
        {
            _setAction = setAction;
        }

        public TValue this[TIndex i]
        {
            set => _setAction(i, value);
        }
    }
}
