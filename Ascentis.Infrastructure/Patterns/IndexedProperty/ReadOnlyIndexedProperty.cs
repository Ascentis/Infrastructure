using System;

// ReSharper disable once CheckNamespace
namespace Ascentis.Infrastructure
{
    public class ReadOnlyIndexedProperty<TIndex, TValue>
    {
        readonly Func<TIndex, TValue> _getFunc;

        public ReadOnlyIndexedProperty(Func<TIndex, TValue> getFunc)
        {
            _getFunc = getFunc;
        }

        public TValue this[TIndex i] => _getFunc(i);
    }
}
