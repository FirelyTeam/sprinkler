using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using Furore.Fhir.Sprinkler.Framework.Framework;
using Furore.Fhir.Sprinkler.Framework.Framework.Attributes;
using Furore.Fhir.Sprinkler.Framework.Utilities;

namespace Furore.Fhir.Sprinkler.Framework.Configuration
{
    public class SprinklerTestLoader : ITestLoader
    {
        private readonly IEnumerable<string> assemblies;

        public SprinklerTestLoader(IEnumerable<string> assemblies = null)
        {
            this.assemblies = assemblies;
        }

        public IEnumerable<Type> GetTestModules()
        {
            return assemblies.SelectMany(GetTestModulesFromAssemblyName);
        }

        private IEnumerable<Type> GetTestModulesFromAssemblyName(string assemblyName)
        {
            Assembly assembly = Assembly.LoadFrom(assemblyName);
            return assembly.GetTypesWithAttribute<SprinklerModule>();
        }
    }

    public class AssemblyTestLoader : ITestLoader
    {
        private readonly IList<Assembly> assemblies;

        public AssemblyTestLoader(params Assembly[] assemblies)
        {
            this.assemblies = assemblies.ToList();
        }

        public AssemblyTestLoader(IEnumerable<Assembly> assemblies)
        {
            this.assemblies = assemblies.ToList();
        }

        public IEnumerable<Type> GetTestModules()
        {
            return assemblies.SelectMany(GetTestModules);
        }

        public static IEnumerable<Type> GetTestModules(Assembly assembly)
        {
            return assembly.GetTypesWithAttribute<SprinklerModule>();
        }
    }
}