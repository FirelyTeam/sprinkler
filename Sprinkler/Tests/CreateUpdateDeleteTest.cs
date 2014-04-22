using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;
using System.Xml;
using Newtonsoft.Json;
using Hl7.Fhir.Rest;
using Hl7.Fhir.Support;
using Hl7.Fhir.Model;
using Sprinkler.Framework;
using System.Security.Cryptography.X509Certificates;
using Hl7.Fhir.Serialization;

namespace Sprinkler.Tests
{
    [SprinklerTestModule("Writing")]
    public class CreateUpdateDeleteTest : SprinklerTestClass
    {
        public string CrudId { get; private set; }
        public string Location
        {
            get
            {
                return "Patient/" + CrudId;
            }
        }
        public List<Uri> Versions { get; private set; }
        public DateTimeOffset? CreateDate { get; private set; }

        public CreateUpdateDeleteTest()
        {
            Versions = new List<Uri>();
        }

        [SprinklerTest("CR01", "create a patient using xml")]
        public void CreatePersonUsingXml()
        {
            tryCreatePatient(client, ResourceFormat.Xml);
        }

        [SprinklerTest("CR02", "create a patient using json")]
        public void CreatePersonUsingJson()
        {
            tryCreatePatient(client, ResourceFormat.Json);
        }

        [SprinklerTest("CR03", "create a patient using client-assigned id")]
        public void CreatePersonUsingClientAssignedId()
        {
            var rnd = new Random();
            CrudId = "sprink" + rnd.Next().ToString();

            Versions.Add(tryCreatePatient(client, ResourceFormat.Xml, CrudId));

            CreateDate = DateTimeOffset.Now;
        }

        [SprinklerTest("UP01", "update that patient (no extensions altered)")]
        public void UpdatePersonNoExt()
        {
            if (CreateDate == null) TestResult.Skipped();
            var patE = client.Read<Patient>(Location);

            patE.Resource.Telecom.Add( new Contact() { System = Contact.ContactSystem.Url, Value = "http://www.nu.nl" } );

            client.Update(patE);

            patE = client.Read<Patient>(Location);

            if (!patE.Resource.Telecom.Any(tel => tel.System == Contact.ContactSystem.Url &&
                                    tel.Value == "http://www.nu.nl"))
                TestResult.Fail(String.Format("Resource {0} unchanged after update", Location));

            Versions.Add(patE.SelfLink);
        }

        [SprinklerTest("UP02", "update that person again (alter extensions)")]
        public void UpdatePersonExt()
        {
            if (CreateDate == null) TestResult.Skipped();

            var patE = client.Read<Patient>(Location);

            var name = patE.Resource.Contact[0].Name;
            var qualifier = new Uri("http://hl7.org/fhir/Profile/iso-21090#qualifier");

            var qExt1 = name.FamilyElement[0].GetExtension(qualifier);
            ((Code)qExt1.Value).Value = "NB";
            name.FamilyElement[0].AddExtension(qualifier, new Code("AC"));

            client.Update(patE);

            patE = client.Read<Patient>(Location);

            var exts = patE.Resource.Contact[0].Name.FamilyElement[0].GetExtensions(qualifier);

            if (exts == null || exts.Count() == 0)
                TestResult.Fail("Extensions have disappeared on resource " + Location);

            if (!exts.Any(ext => ext.Value is Code && ((Code)ext.Value).Value == "NB"))
                TestResult.Fail("Resource extension update was not persisted on resource " + Location);

            if (!exts.Any(ext => ext.Value is Code && ((Code)ext.Value).Value == "AC"))
                TestResult.Fail("Resource extension addition was not persisted on resource " + Location);

            Versions.Add(patE.SelfLink);
        }

        [SprinklerTest("DE01", "delete that person")]
        public void DeletePerson()
        {
            if (CreateDate == null) TestResult.Skipped();
            string location = "Patient/" + CrudId;
            client.Delete(location);
            
            HttpTests.AssertFail(client, () => client.Read<Patient>(location), HttpStatusCode.Gone);
        }

        [SprinklerTest("DE02", "deletion of a non-existing resource")]
        public void DeleteNonExistingPerson()
        {
            var rnd = new Random();
            var location = "Patient/sprink" + rnd.Next().ToString();

            HttpTests.AssertFail(client, () => client.Delete(location), HttpStatusCode.NotFound);
        }

        private Uri tryCreatePatient(FhirClient client, ResourceFormat formatIn, string id = null)
        {
            client.PreferredFormat = formatIn;
            ResourceEntry<Patient> created = null;

            Patient demopat = DemoData.GetDemoPatient();

            if (id == null)
            {
                HttpTests.AssertSuccess(client, () => created = client.Create<Patient>(demopat));
            }
            else
            {
                HttpTests.AssertSuccess(client, () => created = client.Create<Patient>(demopat, id));

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
