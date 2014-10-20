/* 
 * Copyright (c) 2014, Furore (info@furore.com) and contributors
 * See the file CONTRIBUTORS for details.
 * 
 * This file is licensed under the BSD 3-Clause license
 * available at https://raw.github.com/furore-fhir/sprinkler/master/LICENSE
 */

using Hl7.Fhir.Model;
using Hl7.Fhir.Rest;
using Sprinkler.Framework;

namespace Sprinkler.Tests
{
    [SprinklerTestModule("ContentType")]
    public class RequestFormatTest : SprinklerTestClass
    {
        private ResourceEntry<Patient> entry;
        private string id;

        [SprinklerTest("CT05", "Adding a patient")]
        public void AddPatient()
        {
            Patient patient = Utils.NewPatient("Bach", "Johan", "Sebastian");
            entry = Client.Create(patient, null, true);
            id = entry.GetBasicId();
        }

        [SprinklerTest("CT01", "request xml using accept")]
        public void XmlAccept()
        {
            Client.PreferredFormat = ResourceFormat.Xml;
            Client.UseFormatParam = false;
            Client.Read<Patient>(id);
            Assert.ResourceResponseConformsTo(Client, Client.PreferredFormat);
        }

        [SprinklerTest("CT02", "request xml using _format")]
        public void XmlFormat()
        {
            Client.PreferredFormat = ResourceFormat.Xml;
            Client.UseFormatParam = true;
            Client.Read<Patient>(id);
            Assert.ResourceResponseConformsTo(Client, Client.PreferredFormat);
        }

        [SprinklerTest("CT03", "request json using accept")]
        public void JsonAccept()
        {
            Client.PreferredFormat = ResourceFormat.Json;
            Client.UseFormatParam = false;
            Client.Read<Patient>(id);
            Assert.ResourceResponseConformsTo(Client, Client.PreferredFormat);
        }

        [SprinklerTest("CT04", "request json using _format")]
        public void JsonFormat()
        {
            Client.PreferredFormat = ResourceFormat.Json;
            Client.UseFormatParam = true;
            Client.Read<Patient>(id);
            Assert.ResourceResponseConformsTo(Client, Client.PreferredFormat);
        }

        //TODO: Do this later and check operations returning feeds (xml/json)
        //TODO: Do this later, and check other operations (/metadata, /post, etc too)
    }
}