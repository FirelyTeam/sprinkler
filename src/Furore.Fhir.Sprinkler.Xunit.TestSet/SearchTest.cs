using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Furore.Fhir.Sprinkler.FhirUtilities.ResourceManagement;
using Furore.Fhir.Sprinkler.Xunit.ClientUtilities;
using Furore.Fhir.Sprinkler.Xunit.ClientUtilities.FhirClientTestExtensions;
using Furore.Fhir.Sprinkler.Xunit.ClientUtilities.XunitFhirExtensions.Attributes;
using Hl7.Fhir.Model;
using Hl7.Fhir.Rest;
using Xunit;

namespace Furore.Fhir.Sprinkler.Xunit.TestSet
{
    [FixtureConfiguration(FixtureType.File)]
    [Collection("Pagination concurrency issue")]
    public class SearchTest
    {
        private readonly FhirClient client;

        public SearchTest()
        {
            this.client = FhirClientBuilder.CreateFhirClient();
        }

        [Fact]
        [TestMetadata("SE01", "Search resource type without criteria")]
        public void SearchResourcesWithoutCriteria()
        {
            int pageSize = 10;
            Bundle result = client.Search<Patient>(pageSize: pageSize);
            BundleAssert.CheckConditionForResources(result, r => r.Id != null || r.VersionId != null,
                "Resources must have id/versionId information");
            BundleAssert.CheckMinimumNumberOfElementsInBundle(result, 1);
            BundleAssert.CheckMaximumNumberOfElementsInBundle(result, pageSize);

            if (!result.Entry.ByResourceType<Patient>().Any())
                FhirAssert.Fail("search returned entries other than patient");
        }

        [TestMetadata("SE02", "Search on non-existing resource")]
        [Fact]
        public void TrySearchNonExistingResource()
        {
            Assert.Throws<FhirOperationException>(() => client.Search("Nonexistingnonpatientresource"));
            Assert.True(client.LastBodyAsResource is OperationOutcome,
                "When the search fails, a server SHOULD return an OperationOutcome detailing the cause of the failure");
        }

        [TestMetadata("SE03", "Search patient resource on partial familyname")]
        [Theory]
        [Fixture("patient-example-no_references.xml", AutomaticCreateDelete = true)]
        public void SearchResourcesWithNameCriterium(IAutoSetupFixture<Patient> patient)
        {
            string name = patient.Fixture.Name.SelectMany(n => n.Family).First().Substring(0, 3).ToLower();
            Bundle result = client.Search<Patient>(new[] {"family=" + name});

            FhirAssert.EntryIdsArePresentAndAbsoluteUrls(result);
            BundleAssert.CheckMinimumNumberOfElementsInBundle(result, 1);

            FhirAssert.IsTrue(result.Entry.ByResourceType<Patient>().All(p => p.Name != null &&
                                                                          p.Name.Any(
                                                                              n =>
                                                                                  n.Family != null &&
                                                                                  n.Family.Any(
                                                                                      f => f.ToLower().Contains(name)))),
                "search result contains patients that do not match the criterium");
        }

        [TestMetadata("SE04", "Search patient resource on given name")]
        [Theory]
        [Fixture("patient-example-no_references.xml", AutomaticCreateDelete = true)]
        public void SearchPatientOnGiven(IAutoSetupFixture<Patient> patient)
        {
            var given = patient.Fixture.Name.SelectMany(n => n.Given).First().ToLower();
            Bundle result = client.Search<Patient>(new[] {"given=" + given});


            FhirAssert.EntryIdsArePresentAndAbsoluteUrls(result);
            BundleAssert.CheckMinimumNumberOfElementsInBundle(result, 1);
            FhirAssert.IsTrue(result.Entry.ByResourceType<Patient>().All(p => p.Name != null &&
                                                                          p.Name.Any(
                                                                              n =>
                                                                                  n.Given != null &&
                                                                                  n.Given.Any(
                                                                                      f => f.ToLower().Contains(given)))),
                "search result contains patients that do not match the criterium");
        }

        [TestMetadata("SE05", "Search condition by subject (patient) reference - given as url")]
        [Theory]
        [Fixture("patient-example-no_references.xml", AutomaticCreateDelete = true)]
        public void SearchConditionByPatientReference(IAutoSetupFixture<Patient> patient)
        {
            Condition condition = new Condition()
            {
                Patient = new ResourceReference() {Reference = patient.Fixture.GetReferenceId()},
                Code = new CodeableConcept(@"http://example.org/sprinkler", Guid.NewGuid().ToString()),
                VerificationStatus = Condition.ConditionVerificationStatus.Provisional
            };
            try
            {
                condition = client.CreateTagged(condition);

                Bundle result = client.SearchTagged<Condition>(condition.Meta, new[] {"patient=" + condition.Patient.Url});

                FhirAssert.EntryIdsArePresentAndAbsoluteUrls(result);
                FhirAssert.CorrectNumberOfResults(1, result.Entry.Count(),
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
        [Fixture("patient-example-no_references.xml", AutomaticCreateDelete = true)]
        public void SearchEncounterByPatientReference(IAutoSetupFixture<Patient> patient)
        {
            Condition condition = new Condition()
            {
                Patient = new ResourceReference() {Reference = patient.Fixture.GetReferenceId()},
                Code = new CodeableConcept(@"http://example.org/sprinkler", Guid.NewGuid().ToString()),
                VerificationStatus = Condition.ConditionVerificationStatus.Provisional
            };
            try
            {
                condition = client.CreateTagged(condition);
                Bundle result = client.SearchTagged<Condition>(condition.Meta, new[] {"patient=" + patient.Fixture.Id});

                FhirAssert.EntryIdsArePresentAndAbsoluteUrls(result);
                FhirAssert.CorrectNumberOfResults(1, result.Entry.Count(),
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
        [Fixture("patient-example-no_references.xml", AutomaticCreateDelete = true)]
        public void SearchWithIncludes(IAutoSetupFixture<Patient> patient)
        {
            Condition condition = new Condition()
            {
                Patient = new ResourceReference() {Reference = patient.Fixture.GetReferenceId()},
                Code = new CodeableConcept(@"http://example.org/sprinkler", Guid.NewGuid().ToString()),
                VerificationStatus = Condition.ConditionVerificationStatus.Provisional
            };
            try
            {
                condition = client.Create(condition);
                Bundle bundle = client.Search<Condition>(new[] {"_id=" + condition.Id, "_include=Condition:patient"});

                IEnumerable<Patient> patients = bundle.Entry.ByResourceType<Patient>();
                FhirAssert.IsTrue(patients.Any(),
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

                FhirAssert.IsTrue(bundle.ContainsResource(idValues[0]),
                    "Search on quantity value 4.1234 should return 4.12345");
                FhirAssert.IsTrue(!bundle.ContainsResource(idValues[1]),
                    "Search on quantity value 4.1234 should not return 4.12346");
                FhirAssert.IsTrue(!bundle.ContainsResource(idValues[2]),
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
                6.12M
            }.Select(CreateObservation).Cast<Resource>().ToArray();

            var idValues = client.CreateTagged(observations).Select(o => o.Id).ToArray();

            try
            {
                Bundle bundle = client.SearchTagged<Observation>(observations[0].Meta, new[] { "value-quantity=gt5||mg" });

                BundleAssert.CheckMinimumNumberOfElementsInBundle(bundle, 2);
                BundleAssert.CheckTypeForResources<Observation>(bundle);
                BundleAssert.CheckConditionForResources<Observation>(bundle, o => o.Value is Quantity &&
                        ((Quantity)o.Value).Value > 5, "MatchFixtureTypeNameAll observation should have a quantity larger than 5 mg");
                FhirAssert.IsTrue(!bundle.ContainsResource(idValues[0]), "Search greater than quantity should not return lesser value.");
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
                FhirAssert.IsTrue(!bundle.ContainsResource(idValues[0]),
                    "Search on quantity value 6.0||mg should not return  6M");

            }
            finally
            {
                DeleteIds(ResourceType.Observation, idValues);
            }
        }

        [TestMetadata("SE11", "Search with quantifier :missing, on Patient.gender")]
        [Theory]
        [Fixture("patient-example-no_references.xml")]
        public void SearchPatientByGenderMissing(Patient patient)
        {
            patient.Gender = null;
            patient = client.CreateTagged(patient);
            try
            {
                Bundle b = client.SearchTagged<Patient>(patient.Meta, new[] {"gender:missing=true"}, pageSize: 500);
                IEnumerable<Patient> actual =
                 
                       b .Entry.ByResourceType<Patient>();

                FhirAssert.CorrectNumberOfResults(1, actual.Count(),
                    "Expected {0} patients without gender, but got {1}.");

            }
            finally
            {
                client.Delete(patient);
            }
        }

        [TestMetadata("SE12", "Search with non-existing parameter.")]
        [Theory]
        [Fixture("patient-example-no_references.xml")]
        public void SearchPatientByNonExistingParameter(Patient patient)
        {
            patient = client.CreateTagged(patient);
            try
            {
                Bundle actual = client.SearchTagged<Patient>(patient.Meta, new[] { "noparam=nonsense" });
                //Obviously a non-existing search parameter
                int nrOfActualPatients = actual.Entry.ByResourceType<Patient>().Count();
                FhirAssert.CorrectNumberOfResults(1, nrOfActualPatients,
                    "Expected all patients ({0}) since the only search parameter is non-existing, but got {1}.");
                IEnumerable<OperationOutcome> outcomes = actual.Entry.ByResourceType<OperationOutcome>();
            }
            finally
            {
                DeleteIds(ResourceType.Patient, patient.Id);
            }
        }

        [TestMetadata("SE13", "Search with malformed parameter.")]
        [Fact]
        public void SearchPatientByMalformedParameter()
        {
            Bundle actual = null;
            FhirAssert.Fails(client, () => client.Search("Patient", new[] { "...=test" }), out actual,
                HttpStatusCode.BadRequest);
        }

        [TestMetadata("SE14", "Search for deleted resources.")]
        [Theory]
        [Fixture("patient-example-no_references.xml")]
        public void SearchShouldNotReturnDeletedResource(Patient patient)
        {
            patient = client.Create(patient);
            client.Delete(patient);

            Bundle bundle = client.Search<Patient>(new[] { "_id=" + patient.Id });
            BundleAssert.CheckBundleEmpty(bundle);
        }

        [TestMetadata("SE15", "Paging forward and backward through a search result")]
        [Theory]
        [Fixture("patient-example-no_references.xml")]
        public void PageThroughResourceSearch(Patient patient)
        {
            FhirClient _client = FhirClientBuilder.CreateFhirClient();
            int pageSize = 1;
            Resource[] results = _client.CreateTagged(patient, patient).ToArray();
            try
            {
                Bundle page = _client.SearchTagged<Patient>(results[0].Meta, null, null, pageSize);

                int forwardCount = TestBundlePages(_client, page, PageDirection.Next, pageSize);
                int backwardsCount = TestBundlePages(_client, _client.Continue(page, PageDirection.Last), PageDirection.Previous,
                    pageSize);

                FhirAssert.IsTrue(forwardCount == backwardsCount,
                    String.Format("Paging forward returns {0} entries, backwards returned {1}",
                        forwardCount, backwardsCount));
                FhirAssert.IsTrue(forwardCount >= 2, "Bundle should have at least two pages");
            }
            finally 
            {
                DeleteIds(ResourceType.Patient, results.Select(r => r.Id).ToArray());
            }
        }

        private int TestBundlePages(FhirClient _client, Bundle page, PageDirection direction, int pageSize)
        {
            int pageCount = 0;
            while (page != null)
            {
                pageCount++;
                BundleAssert.CheckConditionForResources(page, r => r.Id != null || r.VersionId != null,
                    "Resources must have id/versionId information");

                BundleAssert.CheckMaximumNumberOfElementsInBundle(page, pageSize);

                page = _client.Continue(page, direction);
            }
            return pageCount;
        }

        [TestMetadata("SE16", "Search for code (in observation) - token parameter")]
        [Fact]
        public void SearchWithToken()
        {
            Observation observation = client.CreateTagged(CreateObservation(4.12345M));
            try
            {

                Bundle bundle = client.SearchTagged<Observation>(observation.Meta,
                    new[] {"code=http://loinc.org/|2164-2"});

                FhirAssert.IsTrue(bundle.ContainsResource(observation.Id),
                    "Search on code with system 'http://loinc.org/' and code '2164-2' should return observation");

                bundle = client.SearchTagged<Observation>(observation.Meta, new[] {"code=2164-2"});

                FhirAssert.IsTrue(bundle.ContainsResource(observation.Id),
                    "Search on code with *no* system and code '2164-2' should return observation");

                bundle = client.SearchTagged<Observation>(observation.Meta, new[] {"code=|2164-2"});

                FhirAssert.IsTrue(!bundle.ContainsResource(observation.Id),
                    "Search on code with system '<empty>' and code '2164-2' should not return observation");

                bundle = client.SearchTagged<Observation>(observation.Meta,
                    new[] {"code=completelyBonkersNamespace|2164-2"});

                FhirAssert.IsTrue(!bundle.ContainsResource(observation.Id),
                    "Search on code with system 'completelyBonkersNamespace' and code '2164-2' should return nothing");
            }
            finally
            {
                DeleteIds(ResourceType.Observation, observation.Id);
            }
        }

        [TestMetadata("SE17", "Using type query string parameter")]
        [Fact]
        public void SearchUsingTypeSearchParameter()
        {
            Location loc = new Location();
            loc.Type = new CodeableConcept("http://hl7.org/fhir/v3/RoleCode", "RNEU", "test type");
            loc = client.Create(loc);
            try
            {

                Bundle bundle = client.Search("Location", new string[]
                {
                    "type=RNEU"
                });

                BundleAssert.CheckTypeForResources<Location>(bundle);
                BundleAssert.CheckMinimumNumberOfElementsInBundle(bundle, 1);
            }
            finally
            {
                client.Delete(loc);
            }
        }

        [Theory]
        [TestMetadata("SE18", "Search using the :not modifier")]
        [Fixture("patient-example-no_references.xml")]
        public void SearchResourcesUsingNotModifier(Patient patient)
        {
            patient.Gender = AdministrativeGender.Female;
            Patient importedPatient = client.CreateTagged(patient);
            try
            {

                Bundle result = client.Search<Patient>(new[] { "gender:not=male" });
                BundleAssert.CheckMinimumNumberOfElementsInBundle(result, 1);
                BundleAssert.CheckTypeForResources<Patient>(result);
                Assert.True(result.ResourcesOf<Patient>().All(p => p.Gender != AdministrativeGender.Male),
                    "Returned resources do not satisfy the :not condition");

                result = client.SearchTagged<Patient>(importedPatient.Meta, new[] { "gender:not=male" });
                Assert.True(result.Entry.Count() == 1, "Only one result should be returned by the search");
                Assert.True(result.Entry[0].Resource.Id == importedPatient.Id, "Unexpected result returned by search operation");

            }
            finally
            {
                client.Delete(importedPatient);
            }
        }

        [Theory(Skip = "Known issue: :not modifier doesn't match undefined values.")]
        [TestMetadata("SE19", "Search using the :not modifier to match undefined values")]
        [Fixture("patient-example-no_references.xml")]
        public void SearchResourcesUsingNotModifierForGettingUndefinedValues(Patient patient)
        {
            patient.Gender = AdministrativeGender.Unknown; //patient with unknown gender
            Patient patientWithNoDefinedGender = CreateSimplePatient(); //patient without gender information
            var importedPatients = client.CreateTagged(patient, patientWithNoDefinedGender).Cast<Patient>().ToArray();

            try
            {
                Bundle result = client.Search<Patient>(new[] { "gender:not=male", "gender:not=female" });
                BundleAssert.CheckMinimumNumberOfElementsInBundle(result, 2);
                BundleAssert.CheckTypeForResources<Patient>(result);
                Assert.True(result.ResourcesOf<Patient>().All(p => p.Gender != AdministrativeGender.Male & p.Gender != AdministrativeGender.Female),
                    "Returned resources do not satisfy the :not condition");

                importedPatients[0].Gender = AdministrativeGender.Other;
                client.Update(importedPatients[0]);

                result = client.Search<Patient>(new[] { "gender:not=male", "gender:not=female", "gender:not=unknown" });
                BundleAssert.CheckMinimumNumberOfElementsInBundle(result, 2);
                BundleAssert.CheckTypeForResources<Patient>(result);
                Assert.True(result.ResourcesOf<Patient>().All(p => p.Gender != AdministrativeGender.Male & p.Gender != AdministrativeGender.Female
                & p.Gender != AdministrativeGender.Unknown),
                    "Returned resources do not satisfy the :not condition");

                result = client.SearchTagged<Patient>(importedPatients[0].Meta, new[] { "gender:not=male", "gender:not=female", "gender:not=unknown" });
                BundleAssert.CheckMinimumNumberOfElementsInBundle(result, 2);
                BundleAssert.CheckTypeForResources<Patient>(result);
                Assert.True(result.ResourcesOf<Patient>().All(p => p.Gender != AdministrativeGender.Male & p.Gender != AdministrativeGender.Female
                & p.Gender != AdministrativeGender.Unknown),
                    "Returned resources do not satisfy the :not condition");
                Assert.True(result.Entry.Any(r=> r.Resource.Id == importedPatients[0].Id), "Expected result not found in result set");
                Assert.True(result.Entry.Any(r=> r.Resource.Id == importedPatients[1].Id), "Expected result not found in result set");


            }
            finally
            {
                client.Delete(importedPatients[0]);
                client.Delete(importedPatients[1]);
            }
        }


        public Patient CreateSimplePatient(string family = "Adams", params string[] given)
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

    }
}

