using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Yoakke.Dependency.Internal
{
    public class DependencyValueStorage
    {
        private Dictionary<object, IDependencyValue> storage = new Dictionary<object, IDependencyValue>();

        public T GetValue<T>(object key, [CallerMemberName] string memberName = "")
        {
            if (storage.TryGetValue(key, out var valueStorage)) return valueStorage.GetValue<T>();
            throw new InvalidOperationException($"Tried to retrieve {memberName}({key}) before it was ever set!");
        }

        public void SetValue(DependencySystem system, object key, object value)
        {
            if (storage.TryGetValue(key, out var valueStorage))
            {
                ((InputDependencyValue)valueStorage).SetValue(system, value);
            }
            else
            {
                var newValueStorage = new InputDependencyValue();
                newValueStorage.SetValue(system, value);
                storage.Add(key, newValueStorage);
            }
        }
    }
}
