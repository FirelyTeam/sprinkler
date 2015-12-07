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
    [SprinklerTestModule("ContentType")]
    public class RequestFormatTest
    {
        FhirClient client;

        public RequestFormatTest(Uri fhirUri)
        {
            client = new FhirClient(fhirUri);
        }

        [SprinklerTest("request xml using accept")]
        public void XmlAccept()
        {
            client.PreferredFormat = ContentType.ResourceFormat.Xml;
            client.UseFormatParam = false;
            client.Read<Patient>("1");
            HttpTests.AssertResourceResponseConformsTo(client, client.PreferredFormat);
        }

        [SprinklerTest("request xml using _format")]
        public void XmlFormat()
        {
            client.PreferredFormat = ContentType.ResourceFormat.Xml;
            client.UseFormatParam = true;
            client.Read<Patient>("1");
            HttpTests.AssertResourceResponseConformsTo(client, client.PreferredFormat);
        }

        [SprinklerTest("request json using accept")]
        public void JsonAccept()
        {
            client.PreferredFormat = ContentType.ResourceFormat.Json;
            client.UseFormatParam = false;
            client.Read<Patient>("1");
            HttpTests.AssertResourceResponseConformsTo(client, client.PreferredFormat);
        }

        [SprinklerTest("request json using _format")]
        public void JsonFormat()
        {
            client.PreferredFormat = ContentType.ResourceFormat.Json;
            client.UseFormatParam = true;
            client.Read<Patient>("1");
            HttpTests.AssertResourceResponseConformsTo(client, client.PreferredFormat);
        }

        //TODO: Do this later and check operations returning feeds (xml/json)
        //TODO: Do this later, and check other operations (/metadata, /post, etc too)
    }
}
   
