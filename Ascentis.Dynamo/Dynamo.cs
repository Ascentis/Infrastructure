using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Dynamic;
using System.Reflection;

namespace Ascentis.Infrastructure
{
    [Serializable]
    public class Dynamo : DynamicObject
    {
        private IDictionary<string, object> Properties { get; }

        public static bool IsPrimitive(Type type)
        {
            return type != null && (type.IsPrimitive || type == typeof(decimal) || type == typeof(string) || type.IsArray && IsPrimitive(type.GetElementType()));
        }
        
        public Dynamo()
        {
            Properties = new Dictionary<string, object>();
        }

        public Dynamo(object source)
        {
            CopyFrom(source);
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

        public void CopyFrom(object value)
        {
            if (value is Dynamo item)
            {
                foreach (var prop in item.GetDynamicMemberNames())
                    this[prop] = item[prop];
            }
            else
            {
                var type = value.GetType();
                foreach (var prop in type.GetProperties(BindingFlags.Instance | BindingFlags.Public))
                {
                    var val = prop.GetValue(value);
                    if (IsPrimitive(val.GetType()))
                        this[prop.Name] = val;
                }
            }
        }

        public void CopyTo(object target)
        {
            if (target is Dynamo targetItem)
            {
                foreach (var prop in GetDynamicMemberNames())
                    targetItem[prop] = this[prop];
            }
            else
            {
                var type = target.GetType();
                foreach (var prop in type.GetProperties(BindingFlags.Instance | BindingFlags.Public))
                {
                    if (!PropertyExists(prop.Name))
                        continue;
                    prop.SetValue(target, this[prop.Name]);
                }
            }
        }
    }
}