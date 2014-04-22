using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;
using Hl7.Fhir.Client;
using Hl7.Fhir.Model;
using Hl7.Fhir.Support;

namespace Sprinkler
{
    [SprinklerTestModule("Read")]
    public class ReadTest
    {
        FhirClient client;

        public ReadTest(Uri fhirUri)
        {
            client = new FhirClient(fhirUri);
        }

        [SprinklerTest("result headers on normal read")]
        public void GetTestDataPerson()
        {
            var pat = client.Read<Patient>("1");

            HttpTests.AssertHttpOk(client);

            HttpTests.AssertValidResourceContentTypePresent(client);
            HttpTests.AssertLastModifiedPresent(client);
            HttpTests.AssertContentLocationPresentAndValid(client);
        }

        [SprinklerTest("NotFound on unknown resource type")]        
        public void TryReadUnknownResourceType()
        {
            ResourceLocation rl = new ResourceLocation(client.Endpoint);
            rl.Collection = "thisreallywondexist";
            rl.Id = "1";

            HttpTests.AssertFail(client, () => client.Fetch<Patient>(rl.ToUri()), HttpStatusCode.NotFound);
        }

        [SprinklerTest("NotFound on non-existing resource id")]        
        public void TryReadNonExistingResource()
        {
            HttpTests.AssertFail(client, () => client.Read<Patient>("3141592unlikely"), HttpStatusCode.NotFound);
        }       
    }
}
