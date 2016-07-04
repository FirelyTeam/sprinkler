using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using Hl7.Fhir.Model;

namespace Furore.Fhir.Sprinkler.ClientUtilities.ResourceManagement
{
    public class ZipFileResourceFixtureProvider : IResourceFixturesProvider
    {
        private readonly Dictionary<KeyProvider, Func<IEnumerable<ZipArchiveEntry>, string[], IEnumerable<Resource>>> algorithm;

        public ZipFileResourceFixtureProvider()
        {
            algorithm = new Dictionary<KeyProvider, Func<IEnumerable<ZipArchiveEntry>, string[], IEnumerable<Resource>>>();
            algorithm.Add(KeyProvider.MatchFixtureName, MatchNameProvider);
            algorithm.Add(KeyProvider.MatchFixtureType, MatchTypeProvider);
        } 
        public Resource GetResource(FixtureConfiguration configuration, string resourceKey)
        {
            using (FileStream zipFileToOpen = new FileStream(configuration.FixturesRootPath, FileMode.Open, FileAccess.Read, FileShare.Read))
            using (ZipArchive archive = new ZipArchive(zipFileToOpen, ZipArchiveMode.Read))
            {
                return algorithm[configuration.KeyProvider](archive.Entries, new[] {resourceKey}).Single();
            }
        }
        public IEnumerable<Resource> GetResources(FixtureConfiguration configuration, string[] resourceKeys)
        {
            return GetResourcesInternal(configuration, resourceKeys);
        }

        public IEnumerable<Resource> GetAllResources(FixtureConfiguration configuration)
        {
            return GetResourcesInternal(configuration);
        }
        private Resource ReadZipEntry(ZipArchiveEntry zipEntry)
        {
            using (FhirStreamReader reader = new FhirStreamReader(zipEntry.Open()))
            {
                return reader.ReadResource();
            }
        }
        private IEnumerable<Resource> ReadZipEntries(IEnumerable<ZipArchiveEntry> zipEntries)
        {
            foreach (ZipArchiveEntry zipEntry in zipEntries)
            {
                yield return ReadZipEntry(zipEntry);
            }
        }
        private IEnumerable<Resource> GetResourcesInternal(FixtureConfiguration configuration, string[] resourceKeys = null)
        {
            using (FileStream zipFileToOpen = new FileStream(configuration.FixturesRootPath,
                                            FileMode.Open, FileAccess.Read, FileShare.Read))
            using (ZipArchive archive = new ZipArchive(zipFileToOpen, ZipArchiveMode.Read))
            {
                if (resourceKeys == null)
                {
                    return MatctAllProvider(archive.Entries).ToList();
                }

                return algorithm[configuration.KeyProvider](archive.Entries, resourceKeys).ToList();
            }
        }


        private IEnumerable<Resource> MatchTypeProvider(IEnumerable<ZipArchiveEntry> zipEntries, string[] resourceKeys)
        {
            Dictionary<string, Resource> foundInAdvancedResource = new Dictionary<string, Resource>();

           Dictionary<string, IEnumerable<ZipArchiveEntry>> conventionSplitEntries =
                resourceKeys.Select(key=> key.ToLowerInvariant()).ToDictionary(k=>k, k => zipEntries.Where(z => z.FullName.Contains(k)));

            foreach (KeyValuePair<string, IEnumerable<ZipArchiveEntry>> entry in conventionSplitEntries)
            {
                if (foundInAdvancedResource.ContainsKey(entry.Key))
                {
                    yield return foundInAdvancedResource[entry.Key];
                    break;
                }
                
                foreach (var zipEntry in entry.Value)
                {
                    Resource r = ReadZipEntry(zipEntry);
                    if (r.ResourceType.ToString().ToLowerInvariant() == entry.Key)
                    {
                        yield return r;
                        break;
                    }
                    if (!foundInAdvancedResource.ContainsKey(r.ResourceType.ToString().ToLowerInvariant()))
                    {
                        foundInAdvancedResource.Add(r.ResourceType.ToString().ToLowerInvariant(), r);
                    }
                }       
            }

        }

        private IEnumerable<Resource> MatchNameProvider(IEnumerable<ZipArchiveEntry> zipEntries, string[] resourceKeys)
        {
            return resourceKeys.Select(k => zipEntries.FirstOrDefault(z => z.FullName == k))
                               .Select(ReadZipEntry);
        }


        private IEnumerable<Resource> MatctAllProvider(IEnumerable<ZipArchiveEntry> zipEntries)
        {
            return  zipEntries.Select(ReadZipEntry);
        }
    }
}