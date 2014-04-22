using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;
using System.Xml;
using Newtonsoft.Json;
using Hl7.Fhir.Client;
using Hl7.Fhir.Support;
using Hl7.Fhir.Model;


namespace Sprinkler
{
    [SprinklerTestModule("CRUD")]
    public class CreateUpdateDeleteTest
    {
        FhirClient client;

        public string CrudId { get; private set; }        
        public List<Uri> Versions { get; private set; }
        public DateTimeOffset? CreateDate { get; private set; }


        public CreateUpdateDeleteTest(Uri fhirUri)
        {
            client = new FhirClient(fhirUri);
            Versions = new List<Uri>();
        }

        [SprinklerTest("create a person using xml")]
        public void CreatePersonUsingXml()
        {
            tryCreatePerson(client, ContentType.ResourceFormat.Xml);
        }

        [SprinklerTest("create a person using json")]
        public void CreatePersonUsingJson()
        {
            tryCreatePerson(client, ContentType.ResourceFormat.Json);
        }


        [SprinklerTest("create a person using client-assigned id")]
        public void CreatePersonUsingClientAssignedId()
        {
            var rnd = new Random();
            CrudId = "sprink" + rnd.Next().ToString();
            Versions.Add(tryCreatePerson(client, ContentType.ResourceFormat.Xml, CrudId));

            CreateDate = DateTimeOffset.Now;
        }

        [SprinklerTest("update that person (no extensions altered)")]
        public void UpdatePersonNoExt()
        {
            if (CreateDate == null) TestResult.Skipped();

            var patE = client.Read<Patient>(CrudId);

            patE.Resource.Telecom.Add( new Contact() { System = Contact.ContactSystem.Url,
                         Value = "http://www.nu.nl" } );

            client.Update(patE);

            patE = client.Read<Patient>(CrudId);

            if (!patE.Resource.Telecom.Any(tel => tel.System == Contact.ContactSystem.Url &&
                                    tel.Value == "http://www.nu.nl"))
                TestResult.Fail(String.Format("Resource {0} unchanged after update",CrudId));

            Versions.Add(patE.SelfLink);
        }

        [SprinklerTest("update that person again (alter extensions)")]
        public void UpdatePersonExt()
        {
            if (CreateDate == null) TestResult.Skipped();

            var patE = client.Read<Patient>(CrudId);

            var name = patE.Resource.Contact[0].Name;
            var qualifier = new Uri("http://hl7.org/fhir/profile/@iso-21090#qualifier");

            var qExt1 = name.FamilyElement[0].GetExtension(qualifier);
            ((Code)qExt1.Value).Value = "NB";
            name.FamilyElement[0].AddExtension(qualifier, new Code("AC"));

            client.Update(patE);

            patE = client.Read<Patient>(CrudId);

            var exts = patE.Resource.Contact[0].Name.FamilyElement[0].GetExtensions(qualifier);

            if (exts == null || exts.Count() == 0)
                TestResult.Fail("Extensions have disappeared on resource " + CrudId);
            if (!exts.Any(ext => ext.Value is Code && ((Code)ext.Value).Value == "NB"))
                TestResult.Fail("Resource extension update was not persisted on resource " + CrudId);
            if (!exts.Any(ext => ext.Value is Code && ((Code)ext.Value).Value == "AC"))
                TestResult.Fail("Resource extension addition was not persisted on resource " + CrudId);

            Versions.Add(patE.SelfLink);
        }

        [SprinklerTest("delete that person")]
        public void DeletePerson()
        {
            if (CreateDate == null) TestResult.Skipped();

            client.Delete<Patient>(CrudId);

            HttpTests.AssertFail(client, () => client.Read<Patient>(CrudId), HttpStatusCode.Gone);
        }


        [SprinklerTest("deletion of a non-existing resource")]
        public void DeleteNonExistingPerson()
        {
            var rnd = new Random();
            var someId = "sprink" + rnd.Next().ToString();

            HttpTests.AssertFail(client, () => client.Delete<Patient>(someId), HttpStatusCode.NotFound);
        }

        private Uri tryCreatePerson(FhirClient client, ContentType.ResourceFormat formatIn, string id = null)
        {
            client.PreferredFormat = formatIn;
            ResourceEntry<Patient> created = null;
            
            if(id == null)
                HttpTests.AssertSuccess(client, () => created = client.Create<Patient>(DemoData.GetDemoPatient()));
            else
            {                
                HttpTests.AssertSuccess(client, () => created = client.Create<Patient>(DemoData.GetDemoPatient(),id));

                var rl = new ResourceLocation(created.Id);
                if (rl.Id != id)
                    TestResult.Fail("Server refused to honor client-assigned id");
            }
            
            HttpTests.AssertLocationPresentAndValid(client);
            HttpTests.AssertValidResourceContentTypePresent(client);
            HttpTests.AssertContentLocationValidIfPresent(client);
            
            if( ContentType.GetResourceFormatFromContentType(client.LastResponseDetails.ContentType) != formatIn )
                TestResult.Fail("Asked for " + formatIn.ToString() + " but got " + client.LastResponseDetails.ContentType);

            return created.SelfLink;
        }
    }
}
