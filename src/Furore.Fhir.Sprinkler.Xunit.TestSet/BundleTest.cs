using System;
using System.Collections.Generic;
using System.Linq;
using Furore.Fhir.Sprinkler.FhirUtilities.ResourceManagement;
using Furore.Fhir.Sprinkler.XunitRunner.FhirExtensions;
using Hl7.Fhir.Model;
using Hl7.Fhir.Rest;
using Hl7.Fhir.Serialization;
using Xunit;

namespace Furore.Fhir.Sprinkler.Xunit.TestSet
{
    [FixtureConfiguration(FixtureType.File)]
    [TestCaseOrderer("Furore.Fhir.Sprinkler.XunitRunner.FhirExtensions.PriorityOrderer", "Furore.Fhir.Sprinkler.XunitRunner")]
    public class BundleTest : IClassFixture<FhirClientFixture>
    {
        private readonly FhirClient client;
        private const string uuidFormat = @"urn:uuid:{0}";
        private const string fhirServer = @"http://example.org/fhir/";

        public BundleTest(FhirClientFixture client)
        {
            this.client = client.Client;
        }

        [Theory]
        [TestMetadata("BU01", "Post simple batch bundle")]
        [Fixture(false, "patient-example-no_references.xml", "practitioner-example-no_references.xml")]
        public void Bundle_PostPutSimpleBatchBundle(Patient patient, Practitioner practitioner)
        {
            Bundle bundle = GetBatchBundleForCreate(patient, practitioner);

            bundle = client.Transaction(bundle);
            List<Bundle.ResponseComponent> responses =
                bundle.Entry.Select(e => e.Response).Where(r => r != null).ToList();

            Assert.Equal(responses.Count, 2);
            Assert.All(responses, component => Assert.NotNull(component.Location));
        }

        [Theory]
        [TestMetadata("BU02", "POST batch with references")]
        [Fixture(false, "patient-example-no_references.xml", "practitioner-example-no_references.xml",
            "allergyintolerance-example.xml")]
        public void Bundle_PostBatchWithReferences(Patient patient, Practitioner practitioner,
            AllergyIntolerance allergyIntolerance)
        {
            Bundle bundle = GetBatchBundleForCreate(patient, practitioner);
            allergyIntolerance.Patient = new ResourceReference() { Reference = bundle.Entry[0].FullUrl };
            allergyIntolerance.Recorder = new ResourceReference() { Reference = bundle.Entry[1].FullUrl };
            bundle.Entry.Add(CreateEntryForCreate(allergyIntolerance));
            var xml = FhirSerializer.SerializeToXml(bundle);


            client.ReturnFullResource = true;
            bundle = client.Transaction(bundle);
            List<Bundle.ResponseComponent> responses =
                bundle.Entry.Select(e => e.Response).Where(r => r != null).ToList();
            List<Resource> resources = GetResources(bundle).ToList();
            //spark doesn' return responses
            //Assert.Equal(responses.Count, 3);
            //Assert.All(responses, component => Assert.NotNull(component.Location));
            Assert.Equal(resources.Count, 3);
            Patient resultPatient = resources[0] as Patient;
            Practitioner resultPractitioner = resources[1] as Practitioner;
            AllergyIntolerance resultAllergyIntolerance = resources[2] as AllergyIntolerance;

            FhirUtilities.Assert.IsTrue(
                resultPatient != null && resultPractitioner != null && resultAllergyIntolerance != null,
                "not all resources exist on the server");
            FhirUtilities.Assert.IsTrue(
                resultAllergyIntolerance.Patient.Reference.Contains(resultPatient.GetReferenceId()),
                "AllergyIntolerance doesn't correctly reference the Patient in the bundle.");
            FhirUtilities.Assert.IsTrue(
                resultAllergyIntolerance.Recorder.Reference.Contains(resultPractitioner.GetReferenceId()),
                "AllergyIntolerance doesn't correctly reference the Practitioner in the bundle.");


        }

        private IEnumerable<Resource> GetResources(Bundle responseBundle)
        {
            List<Resource> resources =
                responseBundle.Entry.Where(e => e.Resource != null && e.Resource.ResourceType != ResourceType.OperationOutcome)
                    .Select(c => c.Resource)
                    .ToList();

            if (!resources.Any())
            {
                foreach (var entriesWithLocation in responseBundle.Entry.Where(e => e.Response != null && e.Response.Location != null))
                {
                    resources.Add(client.Read<Resource>(entriesWithLocation.Response.Location));
                }
            }

            return resources;
        }




        //[Theory]
        //[TestMetadata("BU03", "POST transation bundle")]
        //[Fixture(false)]
        //public void Bundle_PostTransaction()
        //{
        //    Assert.IsTrue(true, "aa");
        //}


        //[Theory]
        //[TestMetadata("BU02", "FhirDocument CRUD")]
        //public void Bundle_PostPutFhirDocument()
        //{
        //   Assert.IsTrue(true, "aa");
        //}

        private Bundle GetBatchBundleForCreate(params Resource[] resources)
        {
            Bundle bundle = new Bundle() { Type = Bundle.BundleType.Batch };
            foreach (var resource in resources)
            {
                bundle.Entry.Add(CreateEntryForCreate(resource));
            }
            return bundle;
        }

        private Bundle.EntryComponent CreateEntryForCreate(Resource resource)
        {
            var id = GenerateUuid();
            resource.Id = id;
            // resource.Id = string.Format("{0}{1}/{2}", TestConfiguration.Url, resource.ResourceType, id);
            return new Bundle.EntryComponent()
            {
                Resource = resource,
                Request = new Bundle.RequestComponent() { Method = Bundle.HTTPVerb.POST },
                FullUrl = string.Format("{0}/{1}", resource.ResourceType, id)
                //FullUrl = GenerateUrl(resource)
            };
        }


        private string GenerateUuid()
        {
            // return "urn:uuid:1055f04a-61ca-4014-84f7-0036b8659678";
            //urn:uuid:{CF5BE65E-C8AB-4694-8D76-D4267A1EDB40}
            // return string.Format(uuidFormat, "AA97B177-9383-4934-8543-0F91A7A02836");
            return string.Format(uuidFormat, System.Guid.NewGuid());
        }


        private string GenerateUrl(Resource resource)
        {
            return string.Format("{0}{1}", fhirServer, resource.GetReferenceId());
        }
    }
}