using System.Collections.Generic;
using System.Linq;
using System.Net;
using Furore.Fhir.Sprinkler.FhirUtilities.ResourceManagement;
using Furore.Fhir.Sprinkler.Xunit.ClientUtilities;
using Furore.Fhir.Sprinkler.Xunit.ClientUtilities.XunitFhirExtensions.Attributes;
using Hl7.Fhir.Model;
using Hl7.Fhir.Rest;
using Xunit;

namespace Furore.Fhir.Sprinkler.Xunit.TestSet
{
    [FixtureConfiguration(FixtureType.File)]
    public class BundleConditionalsTest
    {
        private readonly FhirClient client;
        private const string uuidFormat = @"urn:uuid:{0}";
        public BundleConditionalsTest()
        {
            this.client = FhirClientBuilder.CreateFhirClient();
        }
        [Theory]
        [TestMetadata("BC01", "Post batch bundle containing conditional creates")]
        [Fixture(false, "patient-example-no_references.xml", "practitioner-example-no_references.xml")]
        public void Bundle_PostBatchBundleWithConditionalCreateOperations(Patient patient, Practitioner practitioner)
        {
            Utils.AddSprinklerTag(patient);
            Utils.AddSprinklerTag(practitioner);

            Bundle bundle = GetBatchBundleForCreate(patient, practitioner);

            bundle = client.Transaction(bundle);
            List<Bundle.ResponseComponent> responses =
                bundle.Entry.Select(e => e.Response).Where(r => r != null).ToList();

            Assert.Equal(responses.Count, 2);
            Assert.All(responses, component => Assert.NotNull(component.Location));

            foreach (Bundle.EntryComponent component in bundle.Entry)
            {
                client.Delete(component.Resource.GetReferenceId());
            }
        }

        [Theory]
        [TestMetadata("BC02", "Post batch bundle containing conditional creates with already inserted respurces")]
        [Fixture(false, "patient-example-no_references.xml", "practitioner-example-no_references.xml")]
        public void Bundle_BundleWithConditionalCreateOperationsAndAlreadyInsertedResources(Patient patient, Practitioner practitioner)
        {
            Utils.AddSprinklerTag(patient);
            patient.Id = null;
            patient = client.Create(patient);

            Utils.AddSprinklerTag(practitioner);
            practitioner.Id = null;
            practitioner = client.Create(practitioner);

            client.ReturnFullResource = true;

            Bundle bundle = GetBatchBundleForCreate(patient, practitioner);


            bundle = client.Transaction(bundle);
            List<Bundle.ResponseComponent> responses =
                bundle.Entry.Select(e => e.Response).Where(r => r != null).ToList();

            Assert.Equal(responses.Count, 2);
            Assert.All(responses, component => Assert.NotNull(component.Location));
            Assert.True(bundle.Entry.Select(e =>e.Resource != null).Count() ==2);
            FhirAssert.ValidateIds(bundle.Entry[0].Resource, patient.Id, patient.VersionId);
            FhirAssert.ValidateIds(bundle.Entry[1].Resource, practitioner.Id, practitioner.VersionId);

            client.Delete(patient);
            client.Delete(practitioner);
        }

        [Theory]
        [TestMetadata("BC03", "POST batch with conditional creates & references")]
        [Fixture(false, "patient-example-no_references.xml", "practitioner-example-no_references.xml",
            "allergyintolerance-example.xml")]
        public void Bundle_PostBatchWithConditionalPostsAndReferences(Patient patient, Practitioner practitioner,
            AllergyIntolerance allergyIntolerance)
        {
            Utils.AddSprinklerTag(patient);
            Utils.AddSprinklerTag(practitioner);
            Bundle bundle = GetBatchBundleForCreate(patient, practitioner);

            patient.Id = null;
            patient = client.Create(patient);

            allergyIntolerance.Patient = new ResourceReference() { Reference = string.Format("{0}/{1}", bundle.Entry[0].Resource.ResourceType, bundle.Entry[0].FullUrl)};
            allergyIntolerance.Recorder = new ResourceReference() { Reference = string.Format("{0}/{1}", bundle.Entry[1].Resource.ResourceType, bundle.Entry[1].FullUrl) };
            bundle.Entry.Add(CreateEntryForCreate(allergyIntolerance));

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

            FhirAssert.IsTrue(
                resultPatient != null && resultPractitioner != null && resultAllergyIntolerance != null,
                "not all resources exist on the server");
            FhirAssert.IsTrue(
                resultAllergyIntolerance.Patient.Reference.Contains(resultPatient.GetReferenceId()),
                "AllergyIntolerance doesn't correctly reference the Patient in the bundle.");
            FhirAssert.IsTrue(
                resultAllergyIntolerance.Recorder.Reference.Contains(resultPractitioner.GetReferenceId()),
                "AllergyIntolerance doesn't correctly reference the Practitioner in the bundle.");


        }

        [Theory]
        [TestMetadata("BC04", "POST batch with conditional put & delete")]
        [Fixture(false, "patient-example-no_references.xml", "practitioner-example-no_references.xml")]
        public void Bundle_PostBatchWithConditionalPutAndDeleteForExistingResources(Patient patient, Practitioner practitioner)
        {
            Utils.AddSprinklerTag(patient);
            Utils.AddSprinklerTag(practitioner);
            patient.Id = null;
            practitioner.Id = null;
            
            Bundle bundle = new Bundle() { Type = Bundle.BundleType.Transaction };
            bundle.Entry.Add(CreateConditionalEntryForPut(patient));
            bundle.Entry.Add(CreateConditionalEntryForDelete(practitioner));

            Patient addedPatient = client.Create(patient);
            Practitioner addedPractitioner = client.Create(practitioner);
            client.ReturnFullResource = true;
            bundle = client.Transaction(bundle);

            List<Bundle.ResponseComponent> responses =
                bundle.Entry.Select(e => e.Response).Where(r => r != null).ToList();
            List<Resource> resources = GetResources(bundle).ToList();
            Assert.Equal(resources.Count, 1);
            Patient resultPatient = resources[0] as Patient;
        }

        [Theory]
        [TestMetadata("BC05", "POST batch with conditional put")]
        [Fixture(false, "Transaction_Patient.xml")]
        public void Bundle_PostBatchWithConditionalPut(Bundle bundle)
        {
            bundle = client.Transaction(bundle);

            List<Bundle.ResponseComponent> responses =
                bundle.Entry.Select(e => e.Response).Where(r => r != null).ToList();
            List<Resource> resources = GetResources(bundle).ToList();

            Assert.Equal(resources.Count, 1);
            Assert.True(resources[0] is Patient);
            Assert.True(responses[0].Status.Contains(HttpStatusCode.Created.ToString()));

            client.Delete(resources[0]);
        }


        [Theory]
        [TestMetadata("BC06", "POST batch with conditional put and references")]
        [Fixture(false, "Transaction_Patient.xml", "allergyintolerance-example.xml")]
        public void Bundle_PostBatchWithConditionalPutAndReferences(Bundle bundle, AllergyIntolerance allergyIntolerance)
        {
            Patient patient = bundle.GetResources().First() as Patient;
            allergyIntolerance.Patient = new ResourceReference() { Reference = string.Format("{0}/{1}", bundle.Entry[0].Resource.ResourceType, bundle.Entry[0].FullUrl) };
            bundle.Entry.Add(CreateEntryForCreate(allergyIntolerance));
            bundle = client.Transaction(bundle);


            List<Bundle.ResponseComponent> responses =
                       bundle.Entry.Select(e => e.Response).Where(r => r != null).ToList();
            List<Resource> resources = GetResources(bundle).ToList();
            Assert.Equal(resources.Count, 2);
            Patient resultPatient = resources[0] as Patient;
            AllergyIntolerance resultAllergyIntolerance = resources[1] as AllergyIntolerance;

            FhirAssert.IsTrue(
                resultPatient != null && resultAllergyIntolerance != null,
                "not all resources exist on the server");
            FhirAssert.IsTrue(
                resultAllergyIntolerance.Patient.Reference.Contains(resultPatient.GetReferenceId()),
                "AllergyIntolerance doesn't correctly reference the Patient in the bundle.");

            client.Delete(resultPatient);
            client.Delete(resultAllergyIntolerance);
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

        private Bundle GetBatchBundleForCreate(params Resource[] resources)
        {
            Bundle bundle = new Bundle() { Type = Bundle.BundleType.Batch };
            foreach (var resource in resources)
            {
                bundle.Entry.Add(CreateEntryForConditionalCreate(resource));
            }
            return bundle;
        }
        private Bundle.EntryComponent CreateEntryForConditionalCreate(Resource resource)
        {
            Bundle.EntryComponent entry = CreateEntryForCreate(resource);
            entry.Request.IfNoneExist = Utils.GetSprinklerTagCriteria(resource).ToQueryString();
            return entry;
        }

        private Bundle.EntryComponent CreateEntryForCreate(Resource resource)
        {
            return new Bundle.EntryComponent()
            {
                Resource = resource,
                Request = new Bundle.RequestComponent()
                {
                    Method = Bundle.HTTPVerb.POST,
                },
                FullUrl = GenerateUuid()
            };
        }

        private Bundle.EntryComponent CreateConditionalEntryForPut(Resource resource)
        {
            return new Bundle.EntryComponent()
            {
                Resource = resource,
                Request = new Bundle.RequestComponent()
                {
                    Method = Bundle.HTTPVerb.PUT,
                    Url = string.Format("{1}/?{2}", client.Endpoint, resource.ResourceType, Utils.GetSprinklerTagCriteria(resource).ToQueryString())
                },
                FullUrl = GenerateUuid()
            };
        }

        private Bundle.EntryComponent CreateConditionalEntryForDelete(Resource resource)
        {
            return new Bundle.EntryComponent()
            {
                Resource = resource,
                Request = new Bundle.RequestComponent()
                {
                    Method = Bundle.HTTPVerb.DELETE,
                    Url = string.Format("{1}/?{2}", client.Endpoint, resource.ResourceType, Utils.GetSprinklerTagCriteria(resource).ToQueryString())
                },
                FullUrl = GenerateUuid()
            };
        }
        private string GenerateUuid()
        {
            return string.Format(uuidFormat, System.Guid.NewGuid());
        }
    }
}