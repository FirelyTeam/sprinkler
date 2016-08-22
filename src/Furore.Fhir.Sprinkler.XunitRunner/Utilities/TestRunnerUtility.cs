using System;
using Hl7.Fhir.Model;
using Hl7.Fhir.Rest;

namespace Furore.Fhir.Sprinkler.XunitRunner.Utilities
{
    internal class TestRunnerUtility
    {
        private void CountResources(string url)
        {
            FhirClient client = new FhirClient(url);
            int resources = 0;
            foreach (ResourceType resourceType in Enum.GetValues(typeof(ResourceType)))
            {
                if (resourceType != ResourceType.Resource && resourceType != ResourceType.DomainResource)

                {
                    try
                    {
                        Bundle bundle = client.Search(resourceType.ToString());
                        if (bundle.Total.HasValue && bundle.Total.Value > 0)
                        {
                            resources += bundle.Total.Value;
                            System.Console.WriteLine("There are {0} resources of type {1}", bundle.Total, resourceType);
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Console.WriteLine(ex.ToString());
                    }
                }
            }
            System.Console.WriteLine("{0} total resources were found in the system.", resources);
        }
    }
}