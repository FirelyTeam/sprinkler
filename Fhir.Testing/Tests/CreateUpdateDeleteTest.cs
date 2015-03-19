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
    [SprinklerModule("CRUD")]
    public class CreateUpdateDeleteTest : SprinklerTestClass
    {
        public CreateUpdateDeleteTest()
        {
            Versions = new List<string>();
        }

        public string CrudId { get; private set; }

        public string Location
        {
            get { return "Patient/" + CrudId; }
        }

        public List<string> Versions { get; private set; }
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

            Versions.Add(TryCreatePatient(Client, ResourceFormat.Xml, CrudId).ToString());
            
            CreateDate = DateTimeOffset.Now;
        }

        [SprinklerTest("CR04", "Create a patient with an extension")]
        public void CreatePatientWithExtension()
        {
            Patient selena = new Patient();

            var name = new HumanName();
            name.GivenElement.Add(new FhirString("selena"));
            name.FamilyElement.Add(new FhirString("Gomez"));
            selena.Name.Add(name);

            var address = new Address();
            address.LineElement.Add(new FhirString("Cornett"));
            address.CityElement = new FhirString("Amanda");
            address.CountryElement = new FhirString("United States");
            address.StateElement = new FhirString("Texas");
            selena.Address.Add(address);
 
            string qualifier = "http://hl7.org/fhir/Profile/iso-21090#qualifier";
            var contact = new Patient.ContactComponent();
            var contactname = new HumanName();
            contactname.AddExtension(qualifier, new Code("AC"));
            contact.Name = contactname;
            selena.Contact.Add(contact);
            
            var resource = Client.Create(selena);
            string id = resource.Id;
            //entry = null;
            resource = Client.Read<Patient>(resource.ResourceIdentity());

            IEnumerable<Extension> extensions = from contacts in resource.Contact
                                                where contacts.Name.GetExtension(qualifier) != null
                                                select contacts.Name.GetExtension(qualifier);
                                                

            if (extensions == null || extensions.Count() == 0)
                Assert.Fail("Extensions have disappeared on resource " + Location);

            if (!extensions.Any(ext => ext.Value is Code && ((Code) ext.Value).Value == "AC"))
                Assert.Fail("Resource extension was not persisted on created resource " + resource.Id);
        }

        [SprinklerTest("CR05", "update that patient (no extensions altered)")]
        public void UpdatePersonNoExt()
        {
            Assert.SkipWhen(CreateDate == null);
            Patient pat = Client.Read<Patient>(Location);

            pat.Telecom.Add(new ContactPoint{System = ContactPoint.ContactPointSystem.Url, Value = "http://www.nu.nl"});           

            Client.Update(pat);

            pat = Client.Read<Patient>(Location);

            if (!pat.Telecom.Any(
                tel => tel.System == ContactPoint.ContactPointSystem.Url && tel.Value == "http://www.nu.nl"))
                Assert.Fail(String.Format("Resource {0} unchanged after update", Location));

            Versions.Add(pat.VersionId);
        }

        [SprinklerTest("CR06", "update that person again (alter extensions)")]
        public void UpdatePersonAndAddExtension()
        {
            Assert.SkipWhen(CreateDate == null);

            Patient pat= Client.Read<Patient>(Location);

            HumanName name = pat.Contact[0].Name;
            string qualifier = "http://hl7.org/fhir/Profile/iso-21090#qualifier";

            Extension qExt1 = name.FamilyElement[0].GetExtension(qualifier);
            ((Code) qExt1.Value).Value = "NB";
            name.FamilyElement[0].AddExtension(qualifier, new Code("AC"));

            Client.Update(pat);

            pat = Client.Read<Patient>(Location);

            IEnumerable<Extension> extensions = pat.Contact[0].Name.FamilyElement[0].GetExtensions(qualifier);

            if (extensions == null || extensions.Count() == 0)
                Assert.Fail("Extensions have disappeared on resource " + Location);

            if (!extensions.Any(ext => ext.Value is Code && ((Code) ext.Value).Value == "NB"))
                Assert.Fail("Resource extension update was not persisted on resource " + Location);

            if (!extensions.Any(ext => ext.Value is Code && ((Code) ext.Value).Value == "AC"))
                Assert.Fail("Resource extension addition was not persisted on resource " + Location);

            Versions.Add(pat.VersionId);
        }

        [SprinklerTest("CR07", "delete that person")]
        public void DeletePerson()
        {
            if (CreateDate == null) Assert.Skip();
            string location = "Patient/" + CrudId;
            Client.Delete(location);

            Assert.Fails(Client, () => Client.Read<Patient>(location), HttpStatusCode.Gone);
        }

        [SprinklerTest("CR08", "deletion of a non-existing resource")]
        public void DeleteNonExistingPerson()
        {
            var rnd = new Random();
            string location = "Patient/sprink" + rnd.Next();

            Assert.Fails(Client, () => Client.Delete(location), HttpStatusCode.NotFound);
        }

        private string TryCreatePatient(FhirClient client, ResourceFormat formatIn, string id = null)
        {
            client.PreferredFormat = formatIn;
            Patient created = null;
            Patient demopat = DemoData.GetDemoPatient();
            demopat.Id = "23";
            var resourceid = demopat.ResourceIdentity();
            Assert.Success(client, () => created = client.Create(demopat));

            if (demopat.Id != null)
            {
                var ep = new RestUrl(client.Endpoint);
                if (!ep.IsEndpointFor(created.ResourceIdentity()))
                    Assert.Fail("Location of created resource is not located within server endpoint");

                var rl = new ResourceIdentity(created.ResourceIdentity());
                if (rl.Id != id)
                    Assert.Fail("Server refused to honor client-assigned id");
            }

            Assert.LocationPresentAndValid(client);

            // Create bevat geen response content meer. Terecht verwijderd?:
            // EK: Niet helemaal, er is weliswaar geen data meer gereturned, maar de headers (id, versie, modified) worden
            // nog wel geupdate
            Assert.ContentLocationValidIfPresent(client);

            return created.VersionId;
        }
    }
}