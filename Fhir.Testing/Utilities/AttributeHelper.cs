using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Furore.Utilities
{
    public static class AttributeHelper
    {
        public static IEnumerable<Type> GetTypesWithAttribute<T>(this Assembly assembly)
        {
            return assembly.GetTypes().Where(type => type.GetCustomAttributes(typeof(T), true).Length > 0);
        }
    }
}
