using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using Hl7.Fhir.Model;
using Hl7.Fhir.Rest;
using Hl7.Fhir.Serialization;

namespace Furore.Fhir.Sprinkler.Xunit.ClientUtilities.XunitFhirExtensions.ClassFixtures
{
    /// <summary>
    /// Fixture wrapping a FhirClient that can be used to automatically delete all resources created with the FhirClient
    /// </summary>
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
            else if (e.RawResponse.StatusCode == HttpStatusCode.OK && e.RawResponse.ResponseUri == Client.Endpoint)
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

}