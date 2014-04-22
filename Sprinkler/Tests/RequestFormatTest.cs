using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;
using Hl7.Fhir.Rest;
using Hl7.Fhir.Support;
using Hl7.Fhir.Model;
using Sprinkler.Framework;


namespace Sprinkler.Tests
{
    [SprinklerTestModule("ContentType")]
    public class RequestFormatTest : SprinklerTestClass
    {
        ResourceEntry<Patient> entry;
        string id;

        [SprinklerTest("TR01", "Adding a patient")]
        public void AddPatient()
        {
            Patient patient = Utils.NewPatient("Bach", "Johan", "Sebastian");
            entry = client.Create<Patient>(patient, null, true);
            id = entry.GetBasicId();
        }

        [SprinklerTest("CT01", "request xml using accept")]
        public void XmlAccept()
        {
            client.PreferredFormat = ResourceFormat.Xml;
            client.UseFormatParam = false;
            client.Read<Patient>(id);
            HttpTests.AssertResourceResponseConformsTo(client, client.PreferredFormat);
        }

        [SprinklerTest("CT02", "request xml using _format")]
        public void XmlFormat()
        {
            client.PreferredFormat = ResourceFormat.Xml;
            client.UseFormatParam = true;
            client.Read<Patient>(id);
            HttpTests.AssertResourceResponseConformsTo(client, client.PreferredFormat);
        }

        [SprinklerTest("CT03", "request json using accept")]
        public void JsonAccept()
        {
            client.PreferredFormat = ResourceFormat.Json;
            client.UseFormatParam = false;
            client.Read<Patient>(id);
            HttpTests.AssertResourceResponseConformsTo(client, client.PreferredFormat);
        }

        [SprinklerTest("CT04", "request json using _format")]
        public void JsonFormat()
        {
            client.PreferredFormat = ResourceFormat.Json;
            client.UseFormatParam = true;
            client.Read<Patient>(id);
            HttpTests.AssertResourceResponseConformsTo(client, client.PreferredFormat);
        }

        //TODO: Do this later and check operations returning feeds (xml/json)
        //TODO: Do this later, and check other operations (/metadata, /post, etc too)
    }
}
   
