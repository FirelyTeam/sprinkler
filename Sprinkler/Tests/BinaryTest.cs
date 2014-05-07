/* 
 * Copyright (c) 2014, Furore (info@furore.com) and contributors
 * See the file CONTRIBUTORS for details.
 * 
 * This file is licensed under the BSD 3-Clause license
 * available at https://raw.github.com/furore-fhir/sprinkler/master/LICENSE
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;
using System.Xml;
using Newtonsoft.Json;
using Hl7.Fhir.Rest;
using Hl7.Fhir.Support;
using Hl7.Fhir.Model;
using Sprinkler.Framework;

namespace Sprinkler.Tests
{
    [SprinklerTestModule("Binary")]
    public class BinaryTest : SprinklerTestClass
    {
        private Uri binaryId = null;

        
        [SprinklerTest("BI01", "create a binary")]
        public void CreateBinary()
        {
            var bin = DemoData.GetDemoBinary();
            ResourceEntry<Binary> received = null;
            HttpTests.AssertSuccess(client, () => received = client.Create<Binary>(bin));

            HttpTests.AssertLocationPresentAndValid(client);

            var binResult = client.Read<Binary>(received.Id);

            if (binResult.Resource.ContentType != bin.ContentType)
                TestResult.Fail("Created binary of type " + bin.ContentType +
                        "but received " + client.LastResponseDetails.ContentType);

            HttpTests.AssertContentLocationValidIfPresent(client);

            compareData(bin.Content, binResult);

            binaryId = received.Id;
        }

        [SprinklerTest("BI02", "retrieve & update that binary")]
        public void RetrieveBinary()
        {
            if (binaryId == null) TestResult.Skip();

            var received = client.Read<Binary>(binaryId);
            compareData(DemoData.GetDemoBinary().Content, received);

            var data = DemoData.GetDemoBinary().Content.Reverse().ToArray();

            received.Resource.Content = data;
            client.Update(received);

            received = client.Read<Binary>(binaryId);
            compareData(data, received);
        }

        [SprinklerTest("BI03", "delete the binary")]
        public void DeleteBinary()
        {
            if (binaryId == null) TestResult.Skip();

            client.Delete(binaryId);

            HttpTests.AssertFail(client, () => client.Read<Binary>(binaryId), HttpStatusCode.Gone);
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
