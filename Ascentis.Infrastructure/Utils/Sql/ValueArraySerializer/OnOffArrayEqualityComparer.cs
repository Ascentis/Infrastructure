using System.Collections.Generic;

namespace Ascentis.Infrastructure.Utils.Sql.ValueArraySerializer
{
    public class OnOffArrayEqualityComparer : IEqualityComparer<IOnOffArray>
    {
        public bool Equals(IOnOffArray x, IOnOffArray y)
        {
            if (x == null && y == null)
                return true;
            if (x != null && y == null || x == null)
                return false;
            if (x.Count != y.Count)
                return false;

            for (var i = 0; i < x.Count; i++)
            {
                if (x[i] != y[i])
                    return false;
            }

            return true;
        }

        public int GetHashCode(IOnOffArray obj)
        {
            var hash = 0;
            // ReSharper disable once ConstantConditionalAccessQualifier
            // ReSharper disable once ConstantNullCoalescingCondition
            for (var i = 0; i < (obj?.Count ?? 0); i++)
                hash ^= obj[i].GetHashCode() * i;
            return hash;
        }
    }
}
