using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Furore.Fhir.Sprinkler.ClientUtilities.ResourceManagement;
using Hl7.Fhir.Model;
using Hl7.Fhir.Rest;
using Xunit;
using Xunit.Sdk;

namespace Furore.Fhir.Sprinkler.XunitRunner.FhirExtensions
{
    public class FixtureAttribute : DataAttribute
    {
        private readonly bool autocreate;
        private string[] fileNames;

        public FixtureAttribute(bool autocreate = true,  params string[] fileNames)
        {
            this.autocreate = autocreate;
            this.fileNames = fileNames;
        }

        public FixtureAttribute(bool autocreate = true)
        {
            this.autocreate = autocreate;
        }

        public override IEnumerable<object[]> GetData(MethodInfo testMethod)
        {
            FixtureConfiguration configuration = GetFixtureConfiguration(testMethod);
            var parameterTypes = testMethod.GetParameters().Select(p => p.ParameterType).ToArray();
            int paramsCount = parameterTypes.Count();
            var resources = GetResources(configuration, testMethod);
            if (autocreate)
            {
                FhirClient client = new FhirClient(TestConfiguration.Url);
               
                var paramaterValues = new object[parameterTypes.Length];
                for (int i = 0; i < parameterTypes.Length; i++)
                {
                    paramaterValues[i] = client.AutoSetupFixture(resources[i],
                        parameterTypes[i].GenericTypeArguments[0]);
                }

                return paramaterValues.BatchArray(paramsCount);
            }
            else
            {
                var x = resources.BatchArray(paramsCount).ToList();

                return x;
            }
        }

        private Resource[] GetResources(FixtureConfiguration configuration, MethodInfo testMethod)
        {
            ResourceFixturesProvider fixturesProvider = new ResourceFixturesProvider();
            if (fileNames == null)
            {
                configuration.KeyProvider = KeyProvider.MatchFixtureType;
                Type[] x = testMethod.GetParameters()[0].ParameterType.GetGenericParameterConstraints();
                fileNames =
                    Assembly.GetAssembly(typeof(Resource))
                        .GetTypes()
                        .Where(t => x.All(z => z.IsAssignableFrom(t)))
                        .Select(t => t.Name).ToArray();

            }
            return
                fixturesProvider.GetResources(configuration, fileNames)
                    .ToArray();

        }

        private FixtureConfiguration GetFixtureConfiguration(MethodInfo testMethod)
        {
            FixtureConfigurationAttribute configurationAttribute =
                testMethod.GetCustomAttributes<FixtureConfigurationAttribute>().SingleOrDefault()??
                    testMethod.ReflectedType.GetCustomAttributes<FixtureConfigurationAttribute>().SingleOrDefault();

            if (configurationAttribute != null)
                return configurationAttribute.GetFixtureConfiguration();

            return null;
        }
    }
}