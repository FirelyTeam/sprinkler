using System;
using System.Linq;
using Hl7.Fhir.Model;
using Hl7.Fhir.Rest;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using Hl7.Fhir.Serialization;

namespace Furore.Fhir.Sprinkler.XunitRunner.FhirExtensions
{
    public class FhirClientFixture : IDisposable
    {
        private int? threadId;
        private List<string> locations;
        public FhirClient Client { get; private set; }

        public FhirClientFixture()
        {
            Client = new FhirClient(TestConfiguration.Url);
            locations = new List<string>();
            Client.OnBeforeRequest += Client_OnBeforeRequest;
            Client.OnAfterResponse += Client_OnAfterResponse;
        }

        private void Client_OnAfterResponse(object sender, AfterResponseEventArgs e)
        {
            if (e.RawResponse.StatusCode == HttpStatusCode.Created)
            {
                AddLocation(e.RawResponse.Headers["Location"]);
            }
            else if(e.RawResponse.StatusCode == HttpStatusCode.OK && e.RawResponse.ResponseUri == Client.Endpoint)
            {
                Bundle bundle = null;
                string content = new StreamReader(new MemoryStream(e.Body)).ReadToEnd();
                if (FhirParser.ProbeIsJson(content))
                {
                    bundle = FhirParser.ParseFromJson(content) as Bundle;
                }
                else if (FhirParser.ProbeIsXml(content))
                {
                    bundle = FhirParser.ParseFromXml(content) as Bundle;
                }
                if (bundle != null)
                {
                    IEnumerable<string> locations = bundle.Entry.Where(entry => entry.Response != null
                                                                                &&
                                                                                entry.Response.Status ==
                                                                                HttpStatusCode.Created.ToString())
                        .Select(entry => entry.Response.Location);
                    foreach (string location in locations)
                    {
                        AddLocation(location);
                    }
                }
            }
        }

        private void AddLocation(string location)
        {
            locations.Add(new Uri(location).AbsolutePath);
        }

        private void Client_OnBeforeRequest(object sender, BeforeRequestEventArgs e)
        {
            if (!threadId.HasValue)
            {
                threadId = Thread.CurrentThread.ManagedThreadId;
            }
            else if (threadId != Thread.CurrentThread.ManagedThreadId)
            {
                throw new Exception("Tests in unit should not run concurrently.");
            }

            if (e.RawRequest.Method == HttpMethod.Delete.ToString())
            {
                Uri deleteAddress = e.RawRequest.Address;
                var index = locations.FindIndex(l => l.Contains(deleteAddress.AbsolutePath));
                if (index >= 0)
                {
                    locations.RemoveAt(index);
                }
            }
        }

        public virtual void Dispose()
        {
            Client.OnBeforeRequest -= Client_OnBeforeRequest;
            Client.OnAfterResponse -= Client_OnAfterResponse;
            foreach (string location in locations)
            {
                try
                {
                    Client.Delete(location);
                }
                catch (Exception)
                {
                    //resource was not deleted - log error
                    throw;
                }

            }
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

            if (!resource.Meta.Tag.Any(c => c.System == @"http://example.org/sprinkler"))
            {
                resource.Meta.Tag.Add(new Coding(@"http://example.org/sprinkler", tag.ToString()));
            }
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