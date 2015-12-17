using System.Collections.Generic;
using System.Linq;

namespace Furore.Fhir.Sprinkler.Framework.Configuration
{
    public class ConfigurationAssembliesNameProvider : IAssembliesNameProvider
    {
        public IEnumerable<string> GetTestAssemblies()
        {
            return TestAssembliesConfiguration.Instance.TestAssemblies.Cast<TestAssembly>().Select(ta => ta.AssemblyName);
        }
    }
}