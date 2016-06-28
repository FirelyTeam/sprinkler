using System;
using System.Collections.Generic;
using Furore.Fhir.Sprinkler.Framework.Framework.Attributes;
using Furore.Fhir.Sprinkler.Framework.Framework.ResourceManagement;
using Hl7.Fhir.Model;
using Hl7.Fhir.Rest;

namespace Furore.Fhir.Sprinkler.Framework.Framework.TestExecution
{
    public class ResourcePrerequisitesHandler
    {
        private readonly IEnumerable<ResourcePrerequisiteAttribute> attributes;
        private readonly IResourceManager manager;
        private readonly ResourcePrerequisiteAttribute attribute;
        private List<Resource> resources;
        private FhirClient client;

        public ResourcePrerequisitesHandler(IResourceManager manager, ResourcePrerequisiteAttribute attribute)
        {
            this.manager = manager;
            this.attribute = attribute;
        }

        public ResourcePrerequisitesHandler(IEnumerable<ResourcePrerequisiteAttribute> attributes)
        {
            this.attributes = attributes;
            this.manager = new ResourceManager();
        }

        public IEnumerable<Resource> HandlePrerequisities()
        {
            Resource resource = null;
            foreach (ResourcePrerequisiteAttribute prerequisiteAttribute in attributes)
            {
                resource = manager.GetResource(attribute.ResourceFile);
                client.Create(resource);
                resources.Add(resource);
                yield return resource;
            }
        }

        public void CleanupPrerequisities()
        {
            foreach (Resource resource in resources)
            {
                try
                {
                    client.Delete(resource);

                }
                catch (Exception)
                {
                    
                    //log undeleted resources
                }
            }
        }
    }
}