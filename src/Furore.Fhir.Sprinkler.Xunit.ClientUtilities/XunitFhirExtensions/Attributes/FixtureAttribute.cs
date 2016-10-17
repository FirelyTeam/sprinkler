using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Furore.Fhir.Sprinkler.Xunit.ClientUtilities.FhirClientTestExtensions;
using Hl7.Fhir.Model;
using Hl7.Fhir.Rest;
using Xunit.Sdk;

namespace Furore.Fhir.Sprinkler.Xunit.ClientUtilities.XunitFhirExtensions.Attributes
{
    public class FixtureAttribute : DataAttribute
    {
        private readonly bool autocreate;
        private TestMethodResourceProvider resourceProvider;

        public FixtureAttribute(bool autocreate = true, params string[] fileNames)
        {
            this.autocreate = autocreate;
            resourceProvider = new TestMethodResourceProvider(fileNames);
        }

        public FixtureAttribute(bool autocreate = true, bool includeAll = false, params ResourceType[] resourceTypes)
        {
            this.autocreate = autocreate;
            resourceProvider = new TestMethodResourceProvider(resourceTypes);
            resourceProvider.IncludeAll = includeAll;
        }

        public FixtureAttribute(bool autocreate = true, bool includeAll = false)
        {
            this.autocreate = autocreate;
            resourceProvider = new TestMethodResourceProvider();
            resourceProvider.IncludeAll = includeAll;
        }

        public override IEnumerable<object[]> GetData(MethodInfo testMethod)
        {
            var resources = resourceProvider.GetResources(testMethod);
            var parameterTypes = testMethod.GetParameters().Select(p => p.ParameterType).ToArray();
            int paramsCount = parameterTypes.Count();
            if (autocreate)
            {
                FhirClient client = FhirClientBuilder.CreateFhirClient();

                return resources.Select((r, index) => 
                            client.AutoSetupFixture(r, parameterTypes[index%paramsCount].GenericTypeArguments[0]))
                    .BatchArray(paramsCount).ToList();
            }

            return resources.BatchArray(paramsCount).ToList();
        }

    }
}