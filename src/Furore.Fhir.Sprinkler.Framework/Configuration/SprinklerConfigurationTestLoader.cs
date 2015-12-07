using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using Furore.Fhir.Sprinkler.Framework.Framework;
using Furore.Fhir.Sprinkler.Framework.Utilities;

namespace Furore.Fhir.Sprinkler.Framework.Configuration
{
    public class SprinklerConfigurationTestLoader : ITestLoader
    {
        public IEnumerable<Type> GetTestModules()
        {
            return GetAssembliesNames().SelectMany(GetTestModulesFromAssemblyName);
        }

        private IEnumerable<string> GetAssembliesNames()
        {
            return TestAssembliesConfiguration.Instance.TestAssemblies.Cast<TestAssembly>().Select(ta=> ta.AssemblyName);
        }

        private IEnumerable<Type> GetTestModulesFromAssemblyName(string assemblyName)
        {
            Assembly assembly = Assembly.LoadFrom(assemblyName);
            return assembly.GetTypesWithAttribute<SprinklerModule>();
        }
    }
}