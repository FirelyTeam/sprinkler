/* 
 * Copyright (c) 2014, Furore (info@furore.com) and contributors
 * See the file CONTRIBUTORS for details.
 * 
 * This file is licensed under the BSD 3-Clause license
 * available at https://raw.github.com/furore-fhir/sprinkler/master/LICENSE
 */

using System.Collections.Generic;
using System.Net;
using Furore.Fhir.Sprinkler.Framework.Framework;
using Hl7.Fhir.Model;

namespace Furore.Fhir.Sprinkler.TestSet
{
    [SprinklerModule("Read")]
    public class ReadTest : SprinklerTestClass
    {
        public static Patient NewPatient(string family, params string[] given)
        {
            var p = new Patient();
            var n = new HumanName();
            foreach (string g in given)
            {
                n.WithGiven(g);
            }

            n.AndFamily(family);
            p.Name = new List<HumanName>();
            p.Name.Add(n);
            return p;
        }

        [SprinklerTest("RD01", "Result headers on normal read")]
        public void GetTestDataPerson()
        {
            Patient p = NewPatient("Emerald", "Caro");
            Patient entry = Client.Create(p);
            string id = entry.ResourceIdentity().MakeRelative().ToString();

            Client.Read<Patient>(id);

            Assert.HttpOk(Client);

            Assert.ValidResourceContentTypePresent(Client);
            Assert.LastModifiedPresent(Client);
            Assert.ContentLocationPresentAndValid(Client);
        }

        [SprinklerTest("RD02", "Read non-existing resource id")]
        public void TryReadNonExistingResource()
        {
            Assert.Fails(Client, () => Client.Read<Patient>("Patient/3141592unlikely"), HttpStatusCode.NotFound);
        }

        [SprinklerTest("RD03", "Read bad formatted resource id")]
        public void TryReadBadFormattedResourceId()
        {
            //Test for Spark issue #7, https://github.com/furore-fhir/spark/issues/7
            Assert.Fails(Client, () => Client.Read<Patient>("Patient/ID-may-not-contain-CAPITALS"),
                HttpStatusCode.NotFound);
        }
    }
}