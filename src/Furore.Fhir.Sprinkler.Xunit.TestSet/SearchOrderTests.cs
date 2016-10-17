using System;
using System.Collections.Generic;
using System.Linq;
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
    public class SearchOrderTests
    {

        [Theory]
        [TestMetadata("SE-Order01", "Search patients using sort order on family, email and birthdate search parameters")]
        [Fixture(false, "patient-example-no_references.xml")]
        public void SearchPatientsUsingSortOrder(Patient patient)
        {
            FhirClient client = FhirClientBuilder.CreateFhirClient();
            Guid searchGuid = Guid.NewGuid();
            UpdatePatient(patient, "Away", "1984-11-20", "bbb@test.com");
            Patient patient1 = client.CreateTagged(patient, searchGuid);


            UpdatePatient(patient, "Brian", "1984-11-20", "aaaa@test.com");
            Patient patient2 = client.CreateTagged(patient, searchGuid);


            UpdatePatient(patient, "Adams", "1984-11-25", "ccc@test.com");
            Patient patient3 = client.CreateTagged(patient, searchGuid);
            try
            {
                Bundle result = client.SearchTagged<Patient>(searchGuid, new[] { "_sort=family" });
                Assert.True(result.Entry.Count() == 3, "MatchFixtureTypeNameAll 3 resources should be returne by the search");
                Assert.True(result.Entry[0].Resource.Id == patient3.Id, "Unexpected result returned by search operation");

                result = client.SearchTagged<Patient>(searchGuid, new[] { "_sort=email" });
                Assert.True(result.Entry.Count() == 3, "MatchFixtureTypeNameAll 3 resources should be returne by the search");
                Assert.True(result.Entry[0].Resource.Id == patient2.Id, "Unexpected result returned by search operation");


                result = client.SearchTagged<Patient>(searchGuid, new[] { "_sort=birthdate" });
                Assert.True(result.Entry.Count() == 3, "MatchFixtureTypeNameAll 3 resources should be returne by the search");
                Assert.True(result.Entry[2].Resource.Id == patient3.Id, "Unexpected result returned by search operation");

                result = client.SearchTagged<Patient>(searchGuid, new[] { "_sort=birthdate", "_sort=email" });
                Assert.True(result.Entry.Count() == 3, "MatchFixtureTypeNameAll 3 resources should be returne by the search");
                Assert.True(result.Entry[0].Resource.Id == patient2.Id, "Unexpected result returned by search operation");

                result = client.SearchTagged<Patient>(searchGuid, new[] { "_sort=birthdate", "_sort=family" });
                Assert.True(result.Entry.Count() == 3, "MatchFixtureTypeNameAll 3 resources should be returne by the search");
                Assert.True(result.Entry[0].Resource.Id == patient1.Id, "Unexpected result returned by search operation");
            }
            finally
            {
                client.Delete(patient1);
                client.Delete(patient2);
                client.Delete(patient3);
            }
        }

        private void UpdatePatient(Patient patient, string family, string birthDate, string email)
        {
            patient.Name[0].Family = new List<string> { family };
            patient.BirthDateElement = new Date(birthDate); //"1984-11-20"
            patient.Telecom.Clear();
            patient.Telecom.Add(new ContactPoint(ContactPoint.ContactPointSystem.Email, ContactPoint.ContactPointUse.Work, email));
        }

        [Fact]
        [TestMetadata("SE-Order02", "Search observations using sort order on value-quantity")]
        public void SearchSortOrderForQuantityParameter()
        {
            FhirClient client = FhirClientBuilder.CreateFhirClient();
            Resource[] observationsInput = new[]
            {
                9M,
                6M,
                5M
            }.Select(CreateObservation).Cast<Resource>().ToArray();

            var observations = client.CreateTagged(observationsInput).Cast<Observation>().ToArray();

            Bundle bundle = client.SearchTagged<Observation>(observations[0].Meta, new[] { "_sort=value-quantity" });
            Assert.True(bundle.Entry[0].Resource.Id == observations[2].Id, "Unexpected order returned by search operation");

            ((Quantity) observations[2].Value).Value = 11M;
            client.Update(observations[2]);

            bundle = client.SearchTagged<Observation>(observations[0].Meta, new[] { "_sort=value-quantity" });
            Assert.True(bundle.Entry[0].Resource.Id == observations[1].Id, "Unexpected order returned by search operation");
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


        [Fact]
        [TestMetadata("SE-Order03", "Search structure definitions using sort order on url")]
        public void SearchSortOrderForUriParameter()
        {

            FhirClient client = FhirClientBuilder.CreateFhirClient();
            Resource[] structureDefinitionsInput = new[]
            {
                "http://hl7.org/fhir/StructureDefinition/code",
                "http://hl7.org/fhir/StructureDefinition/aaa",
                "http://hl7.org/fhir/StructureDefinition/bbb"
            }.Select(CreateStructureDefinition).Cast<Resource>().ToArray();

            var structures = client.CreateTagged(structureDefinitionsInput).Cast<StructureDefinition>().ToArray();

            Bundle bundle = client.SearchTagged<StructureDefinition>(structures[0].Meta, new[] { "_sort=url" });
            Assert.True(bundle.Entry[0].Resource.Id == structures[1].Id, "Unexpected order returned by search operation");

            bundle = client.SearchTagged<StructureDefinition>(structures[0].Meta, new[] { "_sort:desc=url" });
            Assert.True(bundle.Entry[0].Resource.Id == structures[0].Id, "Unexpected order returned by search operation");
        }


        [TestMetadata("SE-Order04", "Search AllergyIntolerance using sort order on patient")]
        [Theory]
        [Fixture(false, "patient-example-no_references.xml")]
        public void SearchSortOrderForReferenceParameter(Patient patient)
        {
            FhirClient client = FhirClientBuilder.CreateFhirClient();
            Patient patient1 = client.Create(patient);
            Patient patient2 = client.Create(patient);

            AllergyIntolerance intolerance = new AllergyIntolerance()
            {
                Substance = new CodeableConcept("http://snomed.info/sct", "227493005", "Cashew nuts"),
                Patient = new ResourceReference() {Reference = patient1.GetReferenceId()}
            };
            intolerance = client.Create(intolerance);
        }

        private StructureDefinition CreateStructureDefinition(string url)
        {
            StructureDefinition structureDefinition = new StructureDefinition();
            structureDefinition.Name = "Structure";
            structureDefinition.Status = ConformanceResourceStatus.Draft;
            structureDefinition.Kind = StructureDefinition.StructureDefinitionKind.Logical;
            structureDefinition.Abstract = true;
            structureDefinition.Url = url;

            return structureDefinition;
        }

    }
}