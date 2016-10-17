using System.Collections.Generic;
using Furore.Fhir.Sprinkler.Xunit.ClientUtilities.ResourceManagement;
using Hl7.Fhir.Model;

namespace Furore.Fhir.Sprinkler.FhirUtilities.ResourceManagement
{
    public class ResourceFixturesProvider : IResourceFixturesProvider
    {
        private Dictionary<FixtureType, IResourceFixturesProvider> providers;

        public ResourceFixturesProvider(FixtureConfiguration configuration = null)
        {
            providers = new Dictionary<FixtureType, IResourceFixturesProvider>();
            providers.Add(FixtureType.File, new FileResourceFixtureProvider());
            providers.Add(FixtureType.ZipFile, new ZipFileResourceFixtureProvider());
        }

        private IResourceFixturesProvider GetProvider(FixtureConfiguration configuration)
        {
            return providers[configuration.FixtureType];
        }

        public Resource GetResource(FixtureConfiguration configuration, string resourcePath)
        {
            return GetProvider(configuration).GetResource(configuration, resourcePath);
        }

        public IEnumerable<Resource> GetResources(FixtureConfiguration configuration, string[] resourceKeys)
        {
            return GetProvider(configuration).GetResources(configuration, resourceKeys);
        }

        public IEnumerable<Resource> GetAllResources(FixtureConfiguration configuration)
        {
            return GetProvider(configuration).GetAllResources(configuration);
        }
    }
}