/* 
 * Copyright (c) 2014, Furore (info@furore.com) and contributors
 * See the file CONTRIBUTORS for details.
 * 
 * This file is licensed under the BSD 3-Clause license
 * available at https://raw.github.com/furore-fhir/sprinkler/master/LICENSE
 */

using System;
using System.Linq;
using System.Net;
using Hl7.Fhir.Model;
using Sprinkler.Framework;

namespace Sprinkler.Tests
{
    [SprinklerModule("Binary")]
    public class BinaryTest : SprinklerTestClass
    {
        private string _binaryId;


        [SprinklerTest("BI01", "create a binary")]
        public void CreateBinary()
        {
            Binary bin = DemoData.GetDemoBinary();
            Binary received = null;
            Assert.Success(Client, () => received = Client.Create(bin));

            Assert.LocationPresentAndValid(Client);

            Binary binResult = Client.Read<Binary>(received.Id);

            if (binResult.ContentType != bin.ContentType)
                Assert.Fail("Created binary of type " + bin.ContentType +
                                "but received " + binResult.ContentType);

            Assert.ContentLocationValidIfPresent(Client);

            CompareData(bin.Content, binResult);

            _binaryId = received.Id;
            
        }

        [SprinklerTest("BI02", "retrieve & update that binary")]
        public void RetrieveBinary()
        {
            if (_binaryId == null) Assert.Skip();

            Binary received = Client.Read<Binary>(_binaryId);
            CompareData(DemoData.GetDemoBinary().Content, received);

            byte[] data = DemoData.GetDemoBinary().Content.Reverse().ToArray();

            received.Content = data;
            Client.Update(received);

            received = Client.Read<Binary>(_binaryId);
            CompareData(data, received);
        }

        [SprinklerTest("BI03", "delete the binary")]
        public void DeleteBinary()
        {
            Assert.SkipWhen(_binaryId == null);

            Client.Delete(_binaryId);

            Assert.Fails(Client, () => Client.Read<Binary>(_binaryId), HttpStatusCode.Gone);
        }


        private static void CompareData(byte[] data, Binary received)
        {
            if (data.Length != received.Content.Length)
                Assert.Fail("Binary data returned has a different size");
            for (int pos = 0; pos < data.Length; pos++)
                if (data[pos] != received.Content[pos])
                    Assert.Fail("Binary data returned differs from original");
        }
    }
}