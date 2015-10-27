using System.Collections.Generic;
using System.Reflection;

namespace Fhir.Testing.Framework
{
    public interface IDynamicTestGenerator
    {
        IEnumerable<MethodInfo> GetTestMethods();
    }
}