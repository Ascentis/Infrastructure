using System;

// ReSharper disable once CheckNamespace
namespace Ascentis.Infrastructure
{
    public class IndexedProperty<TIndex, TValue>
    {
        readonly Action<TIndex, TValue> _setAction;
        readonly Func<TIndex, TValue> _getFunc;

        public IndexedProperty(Func<TIndex, TValue> getFunc, Action<TIndex, TValue> setAction)
        {
            _getFunc = getFunc;
            _setAction = setAction;
        }

        public TValue this[TIndex i]
        {
            get => _getFunc(i);
            set => _setAction(i, value);
        }
    }
}
