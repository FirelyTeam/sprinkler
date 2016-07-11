using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Furore.Fhir.Sprinkler.FhirUtilities;
using Furore.Fhir.Sprinkler.FhirUtilities.ResourceManagement;
using Furore.Fhir.Sprinkler.XunitRunner.FhirExtensions;
using Hl7.Fhir.Model;
using Hl7.Fhir.Rest;
using Xunit;
using Assert = Furore.Fhir.Sprinkler.FhirUtilities.Assert;

namespace Furore.Fhir.Sprinkler.Xunit.TestSet
{
    [FixtureConfiguration(FixtureType.File)]
    public class SearchTest : IClassFixture<FhirClientFixture>
    {
        private readonly FhirClient client;

        public SearchTest(FhirClientFixture client)
        {
            this.client = client.Client;
        }

        [Theory]
        [TestMetadata("SE01", "Search resource type without criteria")]
        [Fixture(true, "patient-example-no_references.xml")]
        public void SearchResourcesWithoutCriteria(IAutoSetupFixture<Patient> patient)
        {
            int pageSize = 10;
            Bundle result = client.Search<Patient>(pageSize: pageSize);
            BundleAssert.CheckConditionForResources(result, r => r.Id != null || r.VersionId != null,
                "Resources must have id/versionId information");
            BundleAssert.CheckMinimumNumberOfElementsInBundle(result, 1);
            BundleAssert.CheckMaximumNumberOfElementsInBundle(result, pageSize);

            if (!result.Entry.ByResourceType<Patient>().Any())
                Assert.Fail("search returned entries other than patient");
        }

        [TestMetadata("SE02", "Search on non-existing resource")]
        [Fact]
        public void TrySearchNonExistingResource()
        {
            Assert.Fails(client, () => client.Search("Nonexistingnonpatientresource"), HttpStatusCode.NotFound);
        }

        [TestMetadata("SE03", "Search patient resource on partial familyname")]
        [Theory]
        [Fixture(true, "patient-example-no_references.xml")]
        public void SearchResourcesWithNameCriterium(IAutoSetupFixture<Patient> patient)
        {
            string name = patient.Fixture.Name.SelectMany(n => n.Family).First().Substring(0, 3).ToLower();
            Bundle result = client.Search<Patient>(new[] {"family=" + name});

            Assert.EntryIdsArePresentAndAbsoluteUrls(result);
            BundleAssert.CheckMinimumNumberOfElementsInBundle(result, 1);

            Assert.IsTrue(result.Entry.ByResourceType<Patient>().All(p => p.Name != null &&
                                                                          p.Name.Any(
                                                                              n =>
                                                                                  n.Family != null &&
                                                                                  n.Family.Any(
                                                                                      f => f.ToLower().Contains(name)))),
                "search result contains patients that do not match the criterium");
        }

        [TestMetadata("SE04", "Search patient resource on given name")]
        [Theory]
        [Fixture(true, "patient-example-no_references.xml")]
        public void SearchPatientOnGiven(IAutoSetupFixture<Patient> patient)
        {
            var given = patient.Fixture.Name.SelectMany(n => n.Given).First().ToLower();
            Bundle result = client.Search<Patient>(new[] {"given=" + given});


            Assert.EntryIdsArePresentAndAbsoluteUrls(result);
            BundleAssert.CheckMinimumNumberOfElementsInBundle(result, 1);
            Assert.IsTrue(result.Entry.ByResourceType<Patient>().All(p => p.Name != null &&
                                                                          p.Name.Any(
                                                                              n =>
                                                                                  n.Given != null &&
                                                                                  n.Given.Any(
                                                                                      f => f.ToLower().Contains(given)))),
                "search result contains patients that do not match the criterium");
        }

        [TestMetadata("SE05", "Search condition by subject (patient) reference - given as url")]
        [Theory]
        [Fixture(true, "patient-example-no_references.xml")]
        public void SearchConditionByPatientReference(IAutoSetupFixture<Patient> patient)
        {
            Condition condition = new Condition()
            {
                Patient = new ResourceReference() {Reference = patient.Fixture.GetReferenceId()}
            };
            try
            {
                condition = client.CreateTagged(condition);

                Bundle result = client.SearchTagged<Condition>(condition.Meta, new[] {"patient=" + condition.Patient.Url});

                Assert.EntryIdsArePresentAndAbsoluteUrls(result);
                Assert.CorrectNumberOfResults(1, result.Entry.Count(),
                    "conditions for this patient (using patient=)");

                client.Delete(condition);
            }
            finally
            {
                if (condition.Id != null)
                {
                    client.Delete(condition);
                }
            }
        }

        [TestMetadata("SE06", "Search condition by subject (patient) reference - given just as id")]
        [Theory]
        [Fixture(true, "patient-example-no_references.xml")]
        public void SearchEncounterByPatientReference(IAutoSetupFixture<Patient> patient)
        {
            Condition condition = new Condition()
            {
                Patient = new ResourceReference() {Reference = patient.Fixture.GetReferenceId()}
            };
            try
            {
                condition = client.CreateTagged(condition);
                Bundle result = client.SearchTagged<Condition>(condition.Meta, new[] {"patient=" + patient.Fixture.Id});

                Assert.EntryIdsArePresentAndAbsoluteUrls(result);
                Assert.CorrectNumberOfResults(1, result.Entry.Count(),
                    "conditions for this patient (using patient=)");
            }
            finally
            {
                if (condition.Id != null)
                {
                    client.Delete(condition);
                }
            }
        }

        [TestMetadata("SE07", "Search with includes")]
        [Theory]
        [Fixture(true, "patient-example-no_references.xml")]
        public void SearchWithIncludes(IAutoSetupFixture<Patient> patient)
        {
            Condition condition = new Condition()
            {
                Patient = new ResourceReference() {Reference = patient.Fixture.GetReferenceId()}
            };
            try
            {
                condition = client.Create(condition);
                Bundle bundle = client.Search<Condition>(new[] {"_id=" + condition.Id, "_include=Condition:patient"});

                IEnumerable<Patient> patients = bundle.Entry.ByResourceType<Patient>();
                Assert.IsTrue(patients.Any(),
                    "Search Conditions with _include=Condition.subject should have patients");
            }
            finally
            {
                if (condition.Id != null)
                {
                    client.Delete(condition);
                }
            }
        }

        [TestMetadata("SE08", "Search for quantity (in observation) - precision tests")]
        [Fact]
        public void SearchQuantity()
        {
            Resource[] observations = new[]
            {
                4.12345M,
                4.12346M,
                4.12349M
            }.Select(CreateObservation).Cast<Resource>().ToArray();

            var idValues = client.CreateTagged(observations).Select(o => o.Id).ToArray();
            try
            {
                Bundle bundle = client.SearchTagged<Observation>(observations[0].Meta, new[] {"value-quantity=4.1234||mg"});

                Assert.IsTrue(bundle.ContainsResource(idValues[0]),
                    "Search on quantity value 4.1234 should return 4.12345");
                Assert.IsTrue(!bundle.ContainsResource(idValues[1]),
                    "Search on quantity value 4.1234 should not return 4.12346");
                Assert.IsTrue(!bundle.ContainsResource(idValues[2]),
                    "Search on quantity value 4.1234 should not return 4.12349");
            }
            finally
            {
                DeleteIds(ResourceType.Observation, idValues);
            }
        }

        private Observation CreateObservation(decimal value)
        {
            var observation = new Observation();
            observation.Status = Observation.ObservationStatus.Preliminary;
            observation.Code = new CodeableConcept() { Coding = new List<Coding>() { new Coding("http://loinc.org", "2164-2"), new Coding("http://snomed.org", "abc123") }, Text = "Code text" };
            observation.Value = new Quantity
            {
                System = "http://unitsofmeasure.org",
                Value = value,
                Code = "mg",
                Unit = "miligram"
            };
            observation.BodySite = new CodeableConcept("http://snomed.info/sct", "182756003");
            return observation;
        }

        [TestMetadata("SE09", "Search for quantity (in observation) - operators")]
        [Fact]
        public void SearchQuantityGreater()
        {
            Resource[] observations = new[]
           {
                4.12M,
                5.12M,
                46.12M
            }.Select(CreateObservation).Cast<Resource>().ToArray();

            var idValues = client.CreateTagged(observations).Select(o => o.Id).ToArray();

            try
            {
                Bundle bundle = client.SearchTagged<Observation>(observations[0].Meta, new[] { "value-quantity=gt5||mg" });

                BundleAssert.CheckMinimumNumberOfElementsInBundle(bundle, 2);
                BundleAssert.CheckTypeForResources<Observation>(bundle);
                BundleAssert.CheckConditionForResources<Observation>(bundle, o => o.Value is Quantity &&
                        ((Quantity)o.Value).Value > 5, "All observation should have a quantity larger than 5 mg");
                Assert.IsTrue(!bundle.ContainsResource(idValues[0]), "Search greater than quantity should not return lesser value.");
            }
            finally
            {
                DeleteIds(ResourceType.Observation, idValues);
            }
         
        }

        private void DeleteIds(ResourceType type, params string[] ids)
        {
            foreach (string value in ids)
            {
                client.Delete(string.Format("{0}/{1}", type, value));
            }
        }


        [TestMetadata("SE10", "Search for decimal parameters with trailing spaces")]
        [Fact]
        public void SearchObservationQuantityWith0Decimal()
        {
            Resource[] observations = new[]
            {
                6M,
                6.0M
            }.Select(CreateObservation).Cast<Resource>().ToArray();

            var idValues = client.CreateTagged(observations).Select(o => o.Id).ToArray();


            try
            {
                Bundle bundle = client.SearchTagged<Observation>(observations[0].Meta, new[] {"value-quantity=6||mg"});
                BundleAssert.CheckContainedResources<Observation>(bundle, idValues);

                bundle = client.SearchTagged<Observation>(observations[0].Meta, new[] {"value-quantity=6.0||mg"});
                BundleAssert.CheckContainedResources<Observation>(bundle, new string[] {idValues[1]});
                Assert.IsTrue(!bundle.ContainsResource(idValues[0]),
                    "Search on quantity value 6.0||mg should not return  6M");

            }
            finally
            {
                DeleteIds(ResourceType.Observation, idValues);
            }
        }

        [TestMetadata("SE11", "Search with quantifier :missing, on Patient.gender")]
        [Theory]
        [Fixture(false, "patient-example-no_references.xml")]
        public void SearchPatientByGenderMissing(Patient patient)
        {
            patient.Gender = null;
            patient = client.CreateTagged(patient);
            try
            {
                Bundle b = client.SearchTagged<Patient>(patient.Meta, new[] {"gender:missing=true"}, pageSize: 500);
                IEnumerable<Patient> actual =
                 
                       b .Entry.ByResourceType<Patient>();

                Assert.CorrectNumberOfResults(1, actual.Count(),
                    "Expected {0} patients without gender, but got {1}.");

            }
            finally
            {
                client.Delete(patient);
            }
        }

        [TestMetadata("SE12", "Search with non-existing parameter.")]
        [Theory]
        [Fixture(false, "patient-example-no_references.xml")]
        public void SearchPatientByNonExistingParameter(Patient patient)
        {
            patient = client.CreateTagged(patient);
            Bundle actual = client.SearchTagged<Patient>(patient.Meta, new[] { "noparam=nonsense" });
            //Obviously a non-existing search parameter
            int nrOfActualPatients = actual.Entry.ByResourceType<Patient>().Count();
            Assert.CorrectNumberOfResults(1, nrOfActualPatients,
                "Expected all patients ({0}) since the only search parameter is non-existing, but got {1}.");
            IEnumerable<OperationOutcome> outcomes = actual.Entry.ByResourceType<OperationOutcome>();
        }

    }
}

