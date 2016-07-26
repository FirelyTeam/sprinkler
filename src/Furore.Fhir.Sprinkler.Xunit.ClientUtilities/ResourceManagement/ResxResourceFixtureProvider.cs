using System.Collections.Generic;
using Hl7.Fhir.Model;

namespace Furore.Fhir.Sprinkler.FhirUtilities.ResourceManagement
{
    public class ResxResourceFixtureProvider : IResourceFixturesProvider
    {
        public Resource GetResource(FixtureConfiguration configuration, string resourceKey)
        {
            throw new System.NotImplementedException();
        }

        public IEnumerable<Resource> GetResources(FixtureConfiguration configuration, string[] resourceKeys)
        {
            throw new System.NotImplementedException();
        }

        public IEnumerable<Resource> GetAllResources(FixtureConfiguration configuration)
        {
            throw new System.NotImplementedException();
        }
    }
}