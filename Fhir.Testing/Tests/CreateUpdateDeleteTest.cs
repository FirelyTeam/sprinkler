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
        public string SimplePatientCrudId { get; private set; }

        public string LocationSimplePatient
        {
            get { return String.IsNullOrEmpty(SimplePatientCrudId) ? null : "Patient/" + SimplePatientCrudId; }
        }

        [SprinklerTest("CR01", "create a patient using xml")]
        public void CreatePersonUsingXml()
        {
            SimplePatientCrudId = TryCreatePatient(ResourceFormat.Xml).Id;
        }

        [SprinklerTest("CR02", "create a patient using json")]
        public void CreatePersonUsingJson()
        {
            SimplePatientCrudId = TryCreatePatient(ResourceFormat.Json).Id;
        }

        [SprinklerTest("CR03", "create a patient using client-assigned id")]
        public void CreatePersonUsingClientAssignedId()
        {
            var rnd = new Random();
            string assignedId = "sprink" + rnd.Next();
            Patient patient = CreatePatientWithClientAssignedId(ResourceFormat.Xml, assignedId);

            var ep = new RestUrl(Client.Endpoint);
            if (!ep.IsEndpointFor(patient.ResourceIdentity()))
                Assert.Fail("Location of created resource is not located within server endpoint");

            var rl = new ResourceIdentity(patient.ResourceIdentity());
            if (rl.Id != assignedId)
                Assert.Fail("Server refused to honor client-assigned id");

            Assert.AssertStatusCode(Client, HttpStatusCode.Created);
            SimplePatientCrudId = assignedId;
        }

        [SprinklerTest("CR04", "Create a patient with manually assigned attributes")]
        public void CreatePatientWithAttributes()
        {
            Patient selena = new Patient();

            var name = new HumanName();
            name.GivenElement.Add(new FhirString("Selena"));
            name.FamilyElement.Add(new FhirString("Gomez"));
            selena.Name.Add(name);

            var address = new Address();
            address.LineElement.Add(new FhirString("Cornett"));
            address.CityElement = new FhirString("Amanda");
            address.CountryElement = new FhirString("United States");
            address.StateElement = new FhirString("Texas");
            selena.Address.Add(address);
           
            var contact = new Patient.ContactComponent();
            var contactname = new HumanName();
            contactname.GivenElement.Add(new FhirString("Martijn"));
            contactname.FamilyElement.Add(new FhirString("Harthoorn"));
            contact.Name = contactname;
            selena.Contact.Add(contact);

            selena.Gender = AdministrativeGender.Female;

            var contactpoint = new ContactPoint();
            contactpoint.System = ContactPoint.ContactPointSystem.Email;
            contactpoint.Value = "selena_the_best@hotmail.com";
            selena.Telecom.Add(contactpoint);

            var resource = Client.Create(selena);

            if (resource.Address.Count() != 1)
            {
                Assert.Fail("Address component has disappeared on resource");
            }

            if (resource.Name.Count() != 1)
            {
                Assert.Fail("Name component has disappeared on resource");
            }

            if (resource.Contact.Count() != 1)
            {
                Assert.Fail("Contact component has disappeared on resource");
            }

            if (resource.Telecom.Count() != 1)
            {
                Assert.Fail("Telecom component has disappeared on resource");
            }
            SimplePatientCrudId = resource.Id;
        }

        [SprinklerTest("CR05", "Create and update a patient with an extension")]
        //TODO: Make this more maintainable
        public void CreateUpdatePatientWithExtension()
        {
               string qualifier = "http://hl7.org/fhir/Profile/iso-21090#qualifier";
            string extensionCode = "AC";
            Patient selena = new Patient();
            Location location = null;

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
 
         
            var contact = new Patient.ContactComponent();
            var contactname = new HumanName();
            contactname.GivenElement.Add(new FhirString("Martijn"));
            contactname.FamilyElement.Add(new FhirString("Harthoorn"));
            contactname.AddExtension(qualifier, new Code(extensionCode));
            contact.Name = contactname;
            selena.Contact.Add(contact);
            
            var resource = Client.Create(selena);

            resource = Client.Read<Patient>(resource.ResourceIdentity());


            CheckHumanNameExtensions(resource.Contact[0].Name, qualifier, new[] { extensionCode },
                "Incorrect extensions found after creating resource" + resource.Id);


            Extension qExt1 = resource.Contact[0].Name.GetExtension(qualifier);

            ((Code)qExt1.Value).Value = "NB";
            resource.Contact[0].Name.AddExtension(qualifier, new Code("RT"));

            resource = Client.Update(resource);

            resource = Client.Read<Patient>(resource.ResourceIdentity());

            CheckHumanNameExtensions(resource.Contact[0].Name, qualifier, new[] { "NB", "RT" },
             "Incorrect extensions found after updating resource" + resource.Id);                        
        }

        [SprinklerTest("CR06", "update that patient (no extensions altered)")]
        public void UpdatePersonNoExt()
        {
            Assert.SkipWhen(LocationSimplePatient == null);
            Patient pat = Client.Read<Patient>(LocationSimplePatient);

            pat.Telecom.Add(new ContactPoint{System = ContactPoint.ContactPointSystem.Other, Value = "http://www.nu.nl"});           

            Client.Update(pat);

            pat = Client.Read<Patient>(LocationSimplePatient);

            if (!pat.Telecom.Any(
                tel => tel.System == ContactPoint.ContactPointSystem.Other && tel.Value == "http://www.nu.nl"))
                Assert.Fail(String.Format("Resource {0} unchanged after update", LocationSimplePatient));
        }



        private void CheckHumanNameExtensions(HumanName name, string qualifier,
           IEnumerable<string> expectedValues, string errorMessage)
        {
             IList<Extension> extensions = name.GetExtensions(qualifier).ToList();
            IList<string> extensionValues = extensions.Select(s => s.Value).OfType<Code>().Select(c => c.Value).ToList();
            foreach (string value in expectedValues)
            {
                if (!extensionValues.Contains(value))
                {
                    Assert.Fail(errorMessage);
                }
            }
        }

        [SprinklerTest("CR08", "delete that person")]
        public void DeletePerson()
        {
            Assert.SkipWhen(LocationSimplePatient == null);
            Client.Delete(LocationSimplePatient);

            Assert.Fails(Client, () => Client.Read<Patient>(LocationSimplePatient), HttpStatusCode.Gone);
        }

        [SprinklerTest("CR09", "deletion of a non-existing resource")]
        public void DeleteNonExistingPerson()
        {
            var rnd = new Random();
            string location = "Patient/sprink" + rnd.Next();

            Assert.Fails(Client, () => Client.Delete(location), HttpStatusCode.NotFound);
        }

        private Patient TryCreatePatient(ResourceFormat formatIn)
        {
            Client.PreferredFormat = formatIn;
            Patient demopat = DemoData.GetDemoPatient();
            Patient created = null;

            Assert.Success(Client, () => created = Client.Create(demopat));

            Assert.LocationPresentAndValid(Client);

            // Create bevat geen response content meer. Terecht verwijderd?:
            // EK: Niet helemaal, er is weliswaar geen data meer gereturned, maar de headers (id, versie, modified) worden
            // nog wel geupdate
            Assert.ContentLocationValidIfPresent(Client);

            return created;
        }

        private Patient CreatePatientWithClientAssignedId(ResourceFormat formatIn, string id)
        {
            Client.PreferredFormat = formatIn;
            Patient demopat = DemoData.GetDemoPatient();
            demopat.Id = id;
            return Client.Update(demopat);
        }
    }
}