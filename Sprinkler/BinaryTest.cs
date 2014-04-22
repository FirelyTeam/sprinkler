using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;
using System.Xml;
using Newtonsoft.Json;
using Hl7.Fhir.Client;
using Hl7.Fhir.Support;
using Hl7.Fhir.Model;


namespace Sprinkler
{
    [SprinklerTestModule("Binary")]
    public class BinaryTest
    {
        FhirClient client;

        public BinaryTest(Uri fhirUri)
        {
            client = new FhirClient(fhirUri);
        }

        private string bId = null;

        [SprinklerTest("create a binary")]
        public void CreateBinary()
        {
            var bin = DemoData.GetDemoBinary();
            ResourceEntry<Binary> received = null;
            HttpTests.AssertSuccess(client, () => received = client.Create<Binary>(bin));

            HttpTests.AssertLocationPresentAndValid(client);
            
            if(received.Resource.ContentType != bin.ContentType)
                TestResult.Fail("Created binary of type " + bin.ContentType +
                        "but received " + client.LastResponseDetails.ContentType);

            HttpTests.AssertContentLocationValidIfPresent(client);

            compareData(bin.Content, received);

            bId = ResourceLocation.GetIdFromResourceId(received.Id);
        }

        [SprinklerTest("retrieve & update that binary")]
        public void RetrieveBinary()
        {
            if (bId == null) TestResult.Skipped();

            var received = client.Read<Binary>(bId);
            compareData(DemoData.GetDemoBinary().Content, received);

            var data = DemoData.GetDemoBinary().Content.Reverse().ToArray();

            received.Resource.Content = data;
            client.Update(received);

            received = client.Read<Binary>(bId);
            compareData(data, received);
        }

        [SprinklerTest("delete the binary")]
        public void DeleteBinary()
        {
            if (bId == null) TestResult.Skipped();

            client.Delete<Binary>(bId);

            HttpTests.AssertFail(client, () => client.Read<Binary>(bId), HttpStatusCode.Gone);
        }


        private static void compareData(byte[] data, ResourceEntry<Binary> received)
        {
            if (data.Length != received.Resource.Content.Length)
                TestResult.Fail("Binary data returned has a different size");
            for (int pos = 0; pos < data.Length; pos++)
                if (data[pos] != received.Resource.Content[pos])
                    TestResult.Fail("Binary data returned differs from original");
        }

    }
}
