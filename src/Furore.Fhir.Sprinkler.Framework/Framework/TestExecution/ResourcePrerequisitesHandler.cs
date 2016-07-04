using System;
using System.Collections.Generic;
using Furore.Fhir.Sprinkler.ClientUtilities.ResourceManagement;
using Furore.Fhir.Sprinkler.Framework.Framework.Attributes;
using Hl7.Fhir.Model;
using Hl7.Fhir.Rest;

namespace Furore.Fhir.Sprinkler.Framework.Framework.TestExecution
{
    //public class ResourcePrerequisitesHandler
    //{
    //    private readonly IEnumerable<ResourcePrerequisiteAttribute> attributes;
    //    private readonly IResourceFixturesProvider fixturesProvider;
    //    private readonly ResourcePrerequisiteAttribute attribute;
    //    private List<Resource> resources;
    //    private FhirClient client;

    //    public ResourcePrerequisitesHandler(IResourceFixturesProvider fixturesProvider, ResourcePrerequisiteAttribute attribute)
    //    {
    //        this.fixturesProvider = fixturesProvider;
    //        this.attribute = attribute;
    //    }

    //    public ResourcePrerequisitesHandler(IEnumerable<ResourcePrerequisiteAttribute> attributes)
    //    {
    //        this.attributes = attributes;
    //        this.fixturesProvider = new ResourceFixturesProvider();
    //    }

    //    public IEnumerable<Resource> HandlePrerequisities()
    //    {
    //        Resource resource = null;
    //        foreach (ResourcePrerequisiteAttribute prerequisiteAttribute in attributes)
    //        {
    //            resource = fixturesProvider.GetResource(attribute.ResourceFile);
    //            client.Create(resource);
    //            resources.Add(resource);
    //            yield return resource;
    //        }
    //    }

    //    public void CleanupPrerequisities()
    //    {
    //        foreach (Resource resource in resources)
    //        {
    //            try
    //            {
    //                client.Delete(resource);

    //            }
    //            catch (Exception)
    //            {
                    
    //                //log undeleted resources
    //            }
    //        }
    //    }
    //}
}