using System;
using System.Collections.Generic;
using System.Linq;
using Hl7.Fhir.Model;
using Hl7.Fhir.Rest;

namespace Furore.Fhir.Sprinkler.Xunit.ClientUtilities.FhirClientTestExtensions
{
    /// Extension methods for creating/searching for resources using a sprinkler meta Tag. 
    /// Used for uniquely identifying resources on a FHIR Server.
    public static class FhirClientTaggedExtensions
    {
        public const string SystemValue = @"http://example.org/sprinkler";
        public const string SearchFormat = @"_tag={0}|{1}";

        public static T CreateTagged<T>(this FhirClient client, T resource) where T : Resource
        {
            return CreateTagged(client, resource, Guid.NewGuid());
        }
        public static T CreateTagged<T>(this FhirClient client, T resource, Guid tag) where T : Resource
        {
            Utils.AddSprinklerTag(resource, tag);
            return client.Create(resource);
        }
        public static IEnumerable<Resource> CreateTagged(this FhirClient client, params Resource[] resources)
        {
            Guid guid = Guid.NewGuid();
            return resources.Select(resource => CreateTagged(client, resource, guid));
        }
        public static Bundle SearchTagged<T>(this FhirClient client, Guid tag, string[] criteria = null,
            string[] includes = null, int? pageSize = default(int?), SummaryType summary = SummaryType.False) where T : Resource, new()
        {
            return SearchTagged<T>(client, tag.ToString(), criteria, includes, pageSize, summary);
        }
        public static Bundle SearchTagged<T>(this FhirClient client, Meta meta, string[] criteria = null,
           string[] includes = null, int? pageSize = default(int?), SummaryType summary = SummaryType.False) where T : Resource, new()
        {
            string tag = meta.Tag.Single(c => c.System == (SystemValue)).Code;
            return SearchTagged<T>(client, tag, criteria, includes, pageSize, summary);
        }
        public static Bundle SearchTagged<T>(this FhirClient client, string tag, string[] criteria = null,
          string[] includes = null, int? pageSize = default(int?), SummaryType summary = SummaryType.False) where T : Resource, new()
        {
            string guidCriteria = string.Format(SearchFormat, SystemValue, tag);

            if (criteria != null)
            {
                List<string> criteriasWithTag = new List<string>(criteria);
                criteriasWithTag.Add(guidCriteria);
                criteria = criteriasWithTag.ToArray();
            }
            else
            {
                criteria = new[] { guidCriteria };
            }

            return client.Search<T>(criteria, includes, pageSize, summary);
        }

    }
}