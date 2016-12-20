using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Hl7.Fhir.Model;
using Hl7.Fhir.Rest;
using Xunit.Sdk;

namespace Furore.Fhir.Sprinkler.Xunit.ClientUtilities.XunitFhirExtensions.Attributes
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class FixtureAttribute : DataAttribute
    {
        public bool AutomaticCreateDelete { get; set; }
        private TestMethodResourceProvider resourceProvider;

        public FixtureAttribute(params string[] fileNames)
        {
            resourceProvider = new TestMethodResourceProvider(fileNames);
        }

        public FixtureAttribute(params ResourceType[] resourceTypes)
        {
            resourceProvider = new TestMethodResourceProvider(resourceTypes);
        }

        public FixtureAttribute(bool includeAllMatches = false)
        {
            resourceProvider = new TestMethodResourceProvider(includeAllMatches);
        }

        public override IEnumerable<object[]> GetData(MethodInfo testMethod)
        {
            var resources = resourceProvider.GetResources(testMethod);
            var parameterTypes = testMethod.GetParameters().Select(p => p.ParameterType).ToArray();
            int paramsCount = parameterTypes.Count();

            if (AutomaticCreateDelete)
            {
                FhirClient client = FhirClientBuilder.CreateFhirClient();
                string identifier = testMethod.DeclaringType.Assembly.FullName;

                for (int i = 0; i < resources.Length; i++)
                {
                    resources[i] = string.IsNullOrEmpty(resources[i].Id) == false
                        ? client.Update(resources[i])
                        : client.Create(resources[i]);
                    TestConfiguration.ResourceCleanUpRegistry.AddToRegistry(identifier, resources[i].GetReferenceId());
                }
            }

            return resources.BatchArray(paramsCount).ToList();
        }
    }
}