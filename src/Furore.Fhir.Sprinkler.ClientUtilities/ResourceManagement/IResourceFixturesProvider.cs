using System;
using System.Collections.Generic;
using Hl7.Fhir.Model;

namespace Furore.Fhir.Sprinkler.ClientUtilities.ResourceManagement
{
    public interface IResourceFixturesProvider
    {
        Resource GetResource(FixtureConfiguration configuration, string resourceKey);
        IEnumerable<Resource> GetResources(FixtureConfiguration configuration, string[] resourceKeys);
        IEnumerable<Resource> GetAllResources(FixtureConfiguration configuration);
    }
}