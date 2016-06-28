using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.IO.Compression;
using Hl7.Fhir.Model;
using Hl7.Fhir.Serialization;

namespace Furore.Fhir.Sprinkler.Framework.Framework.ResourceManagement
{
    public class ResourceManager : IResourceManager
    {
        private readonly ResourceConfiguration configuration;

        public ResourceManager(ResourceConfiguration configuration = null)
        {
            this.configuration = configuration;
        }

        public Resource GetResource(string resourceKey)
        {
            Resource resource = null;
            using (
                FileStream file = new FileStream(GetFileName(resourceKey), FileMode.Open, FileAccess.Read,
                    FileShare.Read))
            {
                StreamReader reader = new StreamReader(file);
                resource = FhirParser.ParseResourceFromXml(reader.ReadToEnd());

            }
            return resource;
        }

        private string GetFileName(string resourceKey)
        {
            return resourceKey;
        }
     
        public  List<DomainResource> GetListofResources(string zipFile)
        {
            string file = Path.Combine(configuration.ResourcesPath, zipFile);

            List<DomainResource> resources = new List<DomainResource>();

            using (FileStream zipFileToOpen = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.Read))

            using (ZipArchive archive = new ZipArchive(zipFileToOpen, ZipArchiveMode.Read))
            {
                ReadOnlyCollection<ZipArchiveEntry> entries = archive.Entries;
                foreach (ZipArchiveEntry e in entries)
                {
                    StreamReader reader = new StreamReader(e.Open());
                    Resource resource = FhirParser.ParseResourceFromXml(reader.ReadToEnd());
                    if (resource is DomainResource)
                    //if (resource.ResourceType == ResourceType.DomainResource)
                    {
                        DomainResource domresource = (DomainResource)resource;
                        resources.Add(domresource);
                    }
                }
            }
            return resources;
        }
    }
}