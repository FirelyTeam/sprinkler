using System.Collections.Generic;
using Hl7.Fhir.Model;

namespace Furore.Fhir.Sprinkler.Framework.Framework.ResourceManagement
{
    public interface IResourceManager
    {
        Resource GetResource(string resourceKey);
        List<DomainResource> GetListofResources(string zipFile);
    }
}