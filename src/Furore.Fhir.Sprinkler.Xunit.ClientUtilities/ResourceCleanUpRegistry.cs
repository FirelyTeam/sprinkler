using System;
using System.Collections.Generic;
using System.Linq;
using Hl7.Fhir.Rest;
using Xunit.Abstractions;

namespace Furore.Fhir.Sprinkler.Xunit.ClientUtilities
{
    public class ResourceCleanUpRegistry
    {
        public Dictionary<string, List<string>> resourcesToClean;

        public ResourceCleanUpRegistry()
        {
            resourcesToClean = new Dictionary<string, List<string>>();
        }

        public void AddToRegistry(string identifier, string resource)
        {
            List<string> resources;
            if (!resourcesToClean.ContainsKey(identifier))
            {
                resources = new List<string>();
                resourcesToClean.Add(identifier, resources);
            }
            else
            {
                resources = resourcesToClean[identifier];
            }

            resources.Add(resource);
        }

        public IEnumerable<string> CleanUpResources(string identifier)
        {
            IEnumerable<string> resources = Enumerable.Empty<string>();
            if (resourcesToClean.ContainsKey(identifier))
            {
                resources = resourcesToClean[identifier];
                resourcesToClean.Remove(identifier);
                FhirClient client = FhirClientBuilder.CreateFhirClient(false);
                foreach (string resource in resources)
                {
                    try
                    {
                        client.Delete(resource);
                    }
                    catch (Exception)
                    {
                        // TODO: add logging
                    }
                }
            }
            return resources;
        }
    }
}