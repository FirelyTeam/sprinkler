using System.Collections.Generic;
using System.Reflection;

namespace Furore.Fhir.Sprinkler.Framework.Framework.TestExecution
{
    public interface IDynamicTestGenerator
    {
        IEnumerable<MethodInfo> GetTestMethods();
    }
}