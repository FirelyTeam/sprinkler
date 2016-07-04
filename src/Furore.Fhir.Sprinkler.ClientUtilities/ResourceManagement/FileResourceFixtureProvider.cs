using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Hl7.Fhir.Model;

namespace Furore.Fhir.Sprinkler.ClientUtilities.ResourceManagement
{
    public class FileResourceFixtureProvider : IResourceFixturesProvider
    {

        public Resource GetResource(FixtureConfiguration configuration, string resourceKey)
        {
            Resource resource = null;
            using (FileStream file = 
                new FileStream(GetFilePath(configuration, resourceKey), FileMode.Open,
                    FileAccess.Read, FileShare.Read))
            using (FhirStreamReader reader = new FhirStreamReader(file))
            {
                resource = reader.ReadResource();
            }
            return resource;
        }

        public IEnumerable<Resource> GetResources(FixtureConfiguration configuration, Func<IEnumerable<string>, IEnumerable<string>> resourceKeySelector)
        {
            IEnumerable<string> files = resourceKeySelector(Directory.EnumerateFiles(configuration.FixturesRootPath));
            return GetResources(configuration, files.ToArray());
        }


        public IEnumerable<Resource> GetResources(FixtureConfiguration configuration, string[] resourceKeys)
        {
           return resourceKeys.Select(k => GetResource(configuration, k));
        }

        public IEnumerable<Resource> GetAllResources(FixtureConfiguration configuration)
        {
            IEnumerable<string> files = Directory.EnumerateFiles(configuration.FixturesRootPath);
            return GetResources(configuration, files.ToArray());
        }

        private string GetFilePath(FixtureConfiguration configuration, string resourceKey)
        {
            return Path.Combine(configuration.FixturesRootPath, resourceKey);
        }
    }
}