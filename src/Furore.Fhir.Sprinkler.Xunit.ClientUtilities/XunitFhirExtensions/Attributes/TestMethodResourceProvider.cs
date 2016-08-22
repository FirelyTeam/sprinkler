using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Furore.Fhir.Sprinkler.FhirUtilities.ResourceManagement;
using Hl7.Fhir.Model;

namespace Furore.Fhir.Sprinkler.Xunit.ClientUtilities.XunitFhirExtensions.Attributes
{
    public class TestMethodResourceProvider
    {
        private readonly ResourceType[] resourceTypes;
        private string[] fileNames;

        public TestMethodResourceProvider(params string[] fileNames)
        {
            this.fileNames = fileNames;
        }
        public TestMethodResourceProvider(params ResourceType[] resourceTypes)
        {
            this.resourceTypes = resourceTypes;
        }

        public TestMethodResourceProvider()
        {
            
        }
        public Resource[] GetResources(MethodInfo testMethod)
        {
            FixtureConfiguration configuration = GetFixtureConfiguration(testMethod);
            ResourceFixturesProvider fixturesProvider = new ResourceFixturesProvider();
            List<string> resourceKeys = new List<string>();
            if (fileNames == null)
            {
                configuration.KeyProvider = KeyProvider.MatchFixtureType;
                if (resourceTypes != null)
                {
                    resourceKeys.AddRange(resourceTypes.Select(r => r.ToString()).ToArray());
                }
                else
                {
                    ParameterInfo[] parameterInfos = testMethod.GetParameters();
                    foreach (ParameterInfo parameterInfo in parameterInfos)
                    {
                        if (parameterInfo.ParameterType.IsGenericParameter)
                        {
                            //hack - this doesn't work for methods with multiple parameters(either all generic or combinations of some generic, some non-generic)
                            Type[] x = parameterInfo.ParameterType.GetGenericParameterConstraints();
                            resourceKeys.AddRange(Assembly.GetAssembly(typeof(Resource))
                                .GetTypes()
                                .Where(t => x.All(z => z.IsAssignableFrom(t)))
                                .Select(t => t.Name).ToArray());
                        }
                        else
                        {
                            resourceKeys.Add(parameterInfo.ParameterType.Name);
                        }
                    }
                }
            }
            else
            {
                resourceKeys.AddRange(fileNames);
            }
            var xc =


                fixturesProvider.GetResources(configuration, resourceKeys.ToArray())
                    .ToArray();


            return xc;
        }

        private FixtureConfiguration GetFixtureConfiguration(MethodInfo testMethod)
        {
            FixtureConfigurationAttribute configurationAttribute =
                testMethod.GetCustomAttributes<FixtureConfigurationAttribute>().SingleOrDefault() ??
                testMethod.ReflectedType.GetCustomAttributes<FixtureConfigurationAttribute>().SingleOrDefault();

            if (configurationAttribute != null)
                return configurationAttribute.GetFixtureConfiguration();

            return null;
        }
    }
}