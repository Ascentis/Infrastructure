using System.Collections.Generic;

// ReSharper disable once CheckNamespace
namespace Ascentis.Infrastructure
{
    public interface IStreamerAdapter
    {
        void ReleaseRow(object[] row);
        IEnumerable<object[]> GetEnumerable();
        int FieldCount { get; }
    }
}
