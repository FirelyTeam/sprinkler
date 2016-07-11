using System;
using System.Linq;
using System.Reflection;
using Hl7.Fhir.Model;
using Hl7.Fhir.Rest;
using System.Collections.Generic;

namespace Furore.Fhir.Sprinkler.XunitRunner.FhirExtensions
{
    public class FhirClientFixture
    {
        public FhirClient Client { get; private set; }
        public FhirClientFixture()
        {
            Client = new FhirClient(TestConfiguration.Url);
        }

    }

    public static class FhirClientTestExtensions
    {
        public static IAutoSetupFixture<T> AutoSetupFixture<T>(this FhirClient client, T fixture) where T : Resource
        {
            return new AutoSetupFixture<T>(client, client.Create(fixture));
        }

        public static IAutoSetupFixture<Resource> AutoSetupFixture(this FhirClient client, Resource resource)
        {
            return AutoSetupFixture(client, resource, resource.GetType());
        }

        public static IAutoSetupFixture<Resource> AutoSetupFixture(this FhirClient client, Resource resource,
            Type resourceType)
        {
            if (!string.IsNullOrEmpty(resource.Id))
            {
                resource = client.Update(resource);
            }
            else
            {
                resource = client.Create(resource);

            }
            Type setupFixture = typeof (AutoSetupFixture<>);
            return
                (IAutoSetupFixture<Resource>)
                    Activator.CreateInstance(setupFixture.MakeGenericType(resourceType), new object[] {client, resource});
        }

        public static IEnumerable<Resource> CreateTagged(this FhirClient client, params Resource[] resources)
        {
            Guid guid = Guid.NewGuid();
            return resources.Select(resource => CreateTagged(client, resource, guid));
        }

        public static T CreateTagged<T>(this FhirClient client, T resource) where T : Resource
        {
            Guid guid = Guid.NewGuid();

            return CreateTagged(client, resource, guid);
        }

        public static T CreateTagged<T>(this FhirClient client, T resource, Guid tag) where T : Resource
        {
            if (resource.Meta == null)
            {
                resource.Meta = new Meta();
            }

            resource.Meta.Tag.Add(new Coding(@"http://example.org/sprinkler", tag.ToString()));
            return client.Create(resource);
        }

        public static Bundle SearchTagged<T>(this FhirClient client, Guid tag, string[] criteria = null,
            string[] includes = null, int? pageSize = default(int?), SummaryType summary = SummaryType.False) where T:Resource, new()
        {
            return SearchTagged<T>(client, tag.ToString(), criteria, includes, pageSize, summary);
        }

        public static Bundle SearchTagged<T>(this FhirClient client, Meta meta, string[] criteria = null,
           string[] includes = null, int? pageSize = default(int?), SummaryType summary = SummaryType.False) where T : Resource, new()
        {
            string tag = meta.Tag.Single(c => c.System == (@"http://example.org/sprinkler")).Code;
            return SearchTagged<T>(client, tag, criteria, includes, pageSize, summary);
        }

        public static Bundle SearchTagged<T>(this FhirClient client, string tag, string[] criteria = null,
          string[] includes = null, int? pageSize = default(int?), SummaryType summary = SummaryType.False) where T : Resource, new()
        {
            string guidCriteria = @"_tag=http://example.org/sprinkler|" + tag;

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

    public interface IAutoSetupFixture<out T> where T : Resource
    {
        T Fixture { get; }
    }

    public sealed class AutoSetupFixture<T> : IAutoSetupFixture<T>, IDisposable where T : Resource
    {
        private readonly FhirClient client;
        private bool disposed = false;

        public T Fixture { get; }

        public AutoSetupFixture(FhirClient client, T importedResource)
        {
            this.client = client;
            this.Fixture = importedResource;
        }

        public void Dispose()
        {
            Dispose(true);
        }

        private void Dispose(bool disposing)
        {
            if (disposed)
                return;

            if (disposing)
            {
                client.Delete(Fixture);
            }

            disposed = true;
        }
    }
}