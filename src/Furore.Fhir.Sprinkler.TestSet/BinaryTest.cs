/* 
 * Copyright (c) 2014, Furore (info@furore.com) and contributors
 * See the file CONTRIBUTORS for details.
 * 
 * This file is licensed under the BSD 3-Clause license
 * available at https://raw.github.com/furore-fhir/sprinkler/master/LICENSE
 */

using System.Linq;
using System.Net;
using Furore.Fhir.Sprinkler.FhirUtilities;
using Furore.Fhir.Sprinkler.Framework.Framework;
using Furore.Fhir.Sprinkler.Framework.Framework.Attributes;
using Furore.Fhir.Sprinkler.Framework.Utilities;
using Hl7.Fhir.Model;
using Hl7.Fhir.Rest;

namespace Furore.Fhir.Sprinkler.TestSet
{
    [SprinklerModule("Binary")]
    public class BinaryTest : SprinklerTestClass
    {
        private string _binaryId;
        private Binary referenceBinary;

        [ModuleInitialize]
        public void Initialize()
        {
            referenceBinary = DemoData.GetDemoBinary();
        }

        [SprinklerTest("BI01", "Create a binary")]
        public void CreateBinary()
        {
            Client.PreferredFormat = ResourceFormat.Xml;
            Client.ReturnFullResource = true;

            Binary received = Client.Create(referenceBinary);
            CheckBinary(received);
          
            _binaryId = @"Binary/" + received.Id;
        }

        [SprinklerTest("BI02", "Read binary as xml")]
        public void ReadBinaryAsXml()
        {
            if (_binaryId == null) Assert.Skip();

            Client.PreferredFormat = ResourceFormat.Xml;
            Client.UseFormatParam = true;
            Client.ReturnFullResource = true;

            Binary result = Client.Read<Binary>(_binaryId);
            Assert.ResourceResponseConformsTo(Client, Client.PreferredFormat);

            CheckBinary(result);

            referenceBinary = result;
        }

        [SprinklerTest("BI03", "Read binary as json")]
        public void ReadBinaryAsJson()
        {
            if (_binaryId == null) Assert.Skip();

            Client.PreferredFormat = ResourceFormat.Json;
            Client.UseFormatParam = false;
            Client.ReturnFullResource = true;

            Binary result = Client.Read<Binary>(_binaryId);
            Assert.ResourceResponseConformsTo(Client, Client.PreferredFormat);

            CheckBinary(result);

            referenceBinary = result;
        }   

        [SprinklerTest("BI04", "Update binary - This might fail because FHIR.API doesn't send binary resources in a resource envelope and the documentation is not clear if FHIR servers should accept it like that.")]
        public void UpdateBinary()
        {
            if (_binaryId == null || referenceBinary.Id == null) Assert.Skip();

            referenceBinary.Content = referenceBinary.Content.Reverse().ToArray();
            Binary result = Client.Update(referenceBinary);

            CheckBinary(result);
        }

        [SprinklerTest("BI05", "Delete binary")]
        public void DeleteBinary()
        {
            Assert.SkipWhen(_binaryId == null);

            Client.Delete(_binaryId);

            Assert.Fails(Client, () => Client.Read<Binary>(_binaryId), HttpStatusCode.Gone);
        }

        private void CheckBinary(Binary result)
        {
            Assert.LocationPresentAndValid(Client);
            Assert.IsTrue(referenceBinary.ContentType == result.ContentType, "ContentType of the received binary is not correct");
            CompareData(referenceBinary.Content, result);

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