using System.Collections.Generic;
using System.Dynamic;

namespace Ascentis.Infrastructure
{
    public class Dynamo : DynamicObject
    {
        private IDictionary<string, object> Properties { get; }

        public Dynamo()
        {
            Properties = new Dictionary<string, object>();
        }

        public int Count => Properties.Keys.Count;

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            if (!Properties.ContainsKey(binder.Name))
                return base.TryGetMember(binder, out result); //means result = null and return = false

            result = Properties[binder.Name];
            return true;
        }

        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            if (!Properties.ContainsKey(binder.Name))
                Properties.Add(binder.Name, value);
            else
                Properties[binder.Name] = value;

            return true;
        }

        public override bool TryDeleteMember(DeleteMemberBinder binder)
        {
            if (!Properties.ContainsKey(binder.Name))
                return base.TryDeleteMember(binder);

            Properties.Remove(binder.Name);
            return true;
        }

        public override IEnumerable<string> GetDynamicMemberNames()
        {
            return Properties.Keys;
        }

        public bool PropertyExists(string name)
        {
            return Properties.ContainsKey(name);
        }

        public object this[string key]
        {
            get => Properties[key];
            set => Properties[key] = value;
        }
    }
}