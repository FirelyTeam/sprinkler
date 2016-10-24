using System;
using System.Collections.Generic;
using System.Linq;
using Hl7.Fhir.Model;
using Hl7.Fhir.Rest;

namespace Furore.Fhir.Sprinkler.Xunit.ClientUtilities
{
    public class ResourceCleanUpRegistry
    {
        public Dictionary<Guid, List<string>> resourcesToClean;

        public ResourceCleanUpRegistry()
        {
            resourcesToClean = new Dictionary<Guid, List<string>>();
        }

        public void AddToRegistry(Guid testCollectionId, string resource)
        {
            List<string> resources;
            if (resourcesToClean.ContainsKey(testCollectionId) == false)
            {
                resources = new List<string>();
                resourcesToClean.Add(testCollectionId, resources);
            }
            else
            {
                resources = resourcesToClean[testCollectionId];
            }

            resources.Add(resource);
        }

        public IEnumerable<string> CleanUpResources(Guid testCollectionId)
        {
            IEnumerable<string> resources = Enumerable.Empty<string>();
            if (resourcesToClean.ContainsKey(testCollectionId))
            {
                resources = resourcesToClean[testCollectionId];
                resourcesToClean.Remove(testCollectionId);
                FhirClient client = FhirClientBuilder.CreateFhirClient(false);
                foreach (string resource in resources)
                {
                    client.Delete(resource);
                }
            }
            return resources;
        } 
    }
}