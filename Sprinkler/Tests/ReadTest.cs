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
using Hl7.Fhir.Rest;
using Hl7.Fhir.Model;
using Hl7.Fhir.Support;
using Sprinkler.Framework;

namespace Sprinkler.Tests
{
    [SprinklerTestModule("Read")]
    public class ReadTest : SprinklerTestClass
    {
        public static Patient NewPatient(string family, params string[] given)
        {
            Patient p = new Patient();
            HumanName n = new HumanName();
            foreach (string g in given) { n.WithGiven(g); }

            n.AndFamily(family);
            p.Name = new List<HumanName>();
            p.Name.Add(n);
            return p;
        }
        
        [SprinklerTest("R001", "Result headers on normal read")]
        public void GetTestDataPerson()
        {
            Patient p = NewPatient("Emerald", "Caro");
            ResourceEntry<Patient> entry = client.Create(p, null, false);
            string id = entry.GetBasicId();

            var pat = client.Read<Patient>(id);

            HttpTests.AssertHttpOk(client);

            HttpTests.AssertValidResourceContentTypePresent(client);
            HttpTests.AssertLastModifiedPresent(client);
            HttpTests.AssertContentLocationPresentAndValid(client);
        }

        [SprinklerTest("R002", "Read unknown resource type")]        
        public void TryReadUnknownResourceType()
        {
            ResourceIdentity id = ResourceIdentity.Build(client.Endpoint, "thisreallywondexist", "1");
            HttpTests.AssertFail(client, () =>  client.Read<Patient>(id), HttpStatusCode.NotFound);
            
            // todo: if the Content-Type header was not set by the server, this generates an abstract exception:
            // "The given key was not present in the dictionary";
        }

        [SprinklerTest("R003", "Read non-existing resource id")]        
        public void TryReadNonExistingResource()
        {
            HttpTests.AssertFail(client, () => client.Read<Patient>("3141592unlikely"), HttpStatusCode.NotFound);
        }

        [SprinklerTest("R004", "Read bad formatted resource id")]
        public void TryReadBadFormattedResourceId()
        {
            //Test for Spark issue #7, https://github.com/furore-fhir/spark/issues/7
            HttpTests.AssertFail(client, () => client.Read<Patient>("ID-may-not-contain-CAPITALS"), HttpStatusCode.BadRequest);
        }
    }
}
