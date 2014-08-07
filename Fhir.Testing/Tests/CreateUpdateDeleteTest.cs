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
using System.Net;
using Hl7.Fhir.Model;
using Hl7.Fhir.Rest;
using Sprinkler.Framework;

namespace Sprinkler.Tests
{
    [SprinklerTestModule("Writing")]
    public class CreateUpdateDeleteTest : SprinklerTestClass
    {
        public CreateUpdateDeleteTest()
        {
            Versions = new List<Uri>();
        }

        public string CrudId { get; private set; }

        public string Location
        {
            get { return "Patient/" + CrudId; }
        }

        public List<Uri> Versions { get; private set; }
        public DateTimeOffset? CreateDate { get; private set; }

        [SprinklerTest("CR01", "create a patient using xml")]
        public void CreatePersonUsingXml()
        {
            TryCreatePatient(Client, ResourceFormat.Xml);
        }

        [SprinklerTest("CR02", "create a patient using json")]
        public void CreatePersonUsingJson()
        {
            TryCreatePatient(Client, ResourceFormat.Json);
        }

        [SprinklerTest("CR03", "create a patient using client-assigned id")]
        public void CreatePersonUsingClientAssignedId()
        {
            var rnd = new Random();
            CrudId = "sprink" + rnd.Next();

            Versions.Add(TryCreatePatient(Client, ResourceFormat.Xml, CrudId));

            CreateDate = DateTimeOffset.Now;
        }

        [SprinklerTest("CR04", "Create a patient with an extension")]
        public void CreatePatientWithExtension()
        {
            Patient selena = Utils.NewPatient("Gomez", "Selena");
            selena.AddAddress("Cornett", "Amanda", "United States", "Texas", "Grand Prairie");

            string qualifier = "http://hl7.org/fhir/Profile/iso-21090#qualifier";
            selena.Contact[0].Name.AddExtension(qualifier, new Code("AC"));
            

            ResourceEntry<Patient> entry = Client.Create(selena, null, false);
            string id = entry.GetBasicId();
            //entry = null;
            entry = Client.Read<Patient>(entry.GetBasicId());

            IEnumerable<Extension> extensions = entry.Resource.Contact[0].Name.GetExtensions(qualifier);

            if (extensions == null || extensions.Count() == 0)
                TestResult.Fail("Extensions have disappeared on resource " + Location);

            if (!extensions.Any(ext => ext.Value is Code && ((Code) ext.Value).Value == "AC"))
                TestResult.Fail("Resource extension was not persisted on created resource " + entry.GetBasicId());
        }

        [SprinklerTest("UP01", "update that patient (no extensions altered)")]
        public void UpdatePersonNoExt()
        {
            if (CreateDate == null) TestResult.Skip();
            ResourceEntry<Patient> entry = Client.Read<Patient>(Location);

            entry.Resource.Telecom.Add(new Contact {System = Contact.ContactSystem.Url, Value = "http://www.nu.nl"});

            Client.Update(entry);

            entry = Client.Read<Patient>(Location);

            if (!entry.Resource.Telecom.Any(
                tel => tel.System == Contact.ContactSystem.Url && tel.Value == "http://www.nu.nl"))
                TestResult.Fail(String.Format("Resource {0} unchanged after update", Location));

            Versions.Add(entry.SelfLink);
        }

        [SprinklerTest("UP02", "update that person again (alter extensions)")]
        public void UpdatePersonAndAddExtension()
        {
            if (CreateDate == null) TestResult.Skip();

            ResourceEntry<Patient> entry = Client.Read<Patient>(Location);

            HumanName name = entry.Resource.Contact[0].Name;
            string qualifier = "http://hl7.org/fhir/Profile/iso-21090#qualifier";

            Extension qExt1 = name.FamilyElement[0].GetExtension(qualifier);
            ((Code) qExt1.Value).Value = "NB";
            name.FamilyElement[0].AddExtension(qualifier, new Code("AC"));

            Client.Update(entry);

            entry = Client.Read<Patient>(Location);

            IEnumerable<Extension> extensions = entry.Resource.Contact[0].Name.FamilyElement[0].GetExtensions(qualifier);

            if (extensions == null || extensions.Count() == 0)
                TestResult.Fail("Extensions have disappeared on resource " + Location);

            if (!extensions.Any(ext => ext.Value is Code && ((Code) ext.Value).Value == "NB"))
                TestResult.Fail("Resource extension update was not persisted on resource " + Location);

            if (!extensions.Any(ext => ext.Value is Code && ((Code) ext.Value).Value == "AC"))
                TestResult.Fail("Resource extension addition was not persisted on resource " + Location);

            Versions.Add(entry.SelfLink);
        }

        [SprinklerTest("DE01", "delete that person")]
        public void DeletePerson()
        {
            if (CreateDate == null) TestResult.Skip();
            string location = "Patient/" + CrudId;
            Client.Delete(location);

            HttpTests.AssertFail(Client, () => Client.Read<Patient>(location), HttpStatusCode.Gone);
        }

        [SprinklerTest("DE02", "deletion of a non-existing resource")]
        public void DeleteNonExistingPerson()
        {
            var rnd = new Random();
            string location = "Patient/sprink" + rnd.Next();

            HttpTests.AssertFail(Client, () => Client.Delete(location), HttpStatusCode.NotFound);
        }

        private Uri TryCreatePatient(FhirClient client, ResourceFormat formatIn, string id = null)
        {
            client.PreferredFormat = formatIn;
            ResourceEntry<Patient> created = null;

            Patient demopat = DemoData.GetDemoPatient();

            if (id == null)
            {
                HttpTests.AssertSuccess(client, () => created = client.Create(demopat));
            }
            else
            {
                HttpTests.AssertSuccess(client, () => created = client.Create(demopat, id));

                var ep = new RestUrl(client.Endpoint);
                if (!ep.IsEndpointFor(created.Id))
                    TestResult.Fail("Location of created resource is not located within server endpoint");

                var rl = new ResourceIdentity(created.Id);
                if (rl.Id != id)
                    TestResult.Fail("Server refused to honor client-assigned id");
            }

            HttpTests.AssertLocationPresentAndValid(client);

            // Create bevat geen response content meer. Terecht verwijderd?:
            // EK: Niet helemaal, er is weliswaar geen data meer gereturned, maar de headers (id, versie, modified) worden
            // nog wel geupdate
            HttpTests.AssertContentLocationValidIfPresent(client);

            return created.SelfLink;
        }
    }
}