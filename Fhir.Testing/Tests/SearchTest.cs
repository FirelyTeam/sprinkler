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
    [SprinklerModule("Search")]
    public class SearchTest : SprinklerTestClass
    {
        private Bundle allPatients;

        // [SprinklerTest("search full system without criteria")]
        public void SearchWithoutCriteria()
        {
            Bundle result = Client.WholeSystemSearch(pageSize: 10);
        }

        [SprinklerTest("SE01", "Search resource type without criteria")]
        public void SearchResourcesWithoutCriteria()
        {
            Bundle result = Client.Search<Patient>(pageSize: 10);
            Assert.EntryIdsArePresentAndAbsoluteUrls(result);

            if (result.Entry.Count == 0)
                Assert.Fail("search did not return any results");

            if (result.Entry.Count > 10)
                Assert.Fail("search returned more patients than specified in _count");

            if (result.Entry.ByResourceType<Patient>().Count() == 0)
                Assert.Fail("search returned entries other than patient");

            allPatients = result;
        }

        [SprinklerTest("SE02", "Search on non-existing resource")]
        public void TrySearchNonExistingResource()
        {
            Assert.Fails(Client, () => Client.Search("Nonexistingnonpatientresource"), HttpStatusCode.NotFound);
        }


        [SprinklerTest("SE03", "Search patient resource on partial familyname")]
        public void SearchResourcesWithNameCriterium()
        {
            Assert.SkipWhen(allPatients == null);
            // First create a search argument: any family name present in the
            // previous unlimited search result that has at least 5 characters
            string name = allPatients.Entry.ByResourceType<Patient>()
                .Where(p => p.Name != null)
                .SelectMany(p => p.Name)
                .Where(hn => hn.Family != null)
                .SelectMany(hn => hn.Family)
                .Where(s => s.Length > 5).First();

            // Take the first three characters
            name = name.Substring(0, 3);


            Bundle result = Client.Search<Patient>(new[] {"family=" + name});
            Assert.EntryIdsArePresentAndAbsoluteUrls(result);

            if (result.Entry.Count == 0)
                Assert.Fail("search did not return any results");

            // Each patient returned should have a family name with the
            // criterium
            IEnumerable<string> names = result.Entry.ByResourceType<Patient>()
                .Where(p => p.Name != null)
                .SelectMany(p => p.Name)
                .Where(hn => hn.Family != null)
                .SelectMany(hn => hn.Family);

            bool correct = result.Entry.ByResourceType<Patient>()
                .All(p => p.Name != null &&
                          p.Name.Where(hn => hn.Family != null)
                              .SelectMany(hn => hn.Family)
                              .Any(s => s.ToLower().Contains(name.ToLower())));

            if (!correct)
                Assert.Fail("search result contains patients that do not match the criterium");
        }


        [SprinklerTest("SE04", "Search patient resource on given name")]
        public void SearchPatientOnGiven()
        {
            Patient patient = new Patient();    
            patient.Name = new List<HumanName> { HumanName.ForFamily("Adams").WithGiven("Fester") };
            Client.Create(patient);
            Bundle bundle = Client.Search<Patient>(new[] {"given=Fester"});        

            var correctName = from pat in bundle.Entry.ByResourceType<Patient>()
                              where pat.Name.Contains(HumanName.ForFamily("Adams").WithGiven("Fester"))
                              select pat;

            bool found = correctName.Count() > 0; 

            Assert.IsTrue(found, "Patient was not found with given name");
        }

        [SprinklerTest("SE05", "Search condition by subject (patient) reference")]
        public void SearchConditionByPatientReference()
        {
            Bundle conditions = Client.Search<Condition>();

            if (conditions.Entry.Count == 0)
            {
                Bundle patients = Client.Search<Patient>();
                if (patients.Entry.Count == 0)
                    Assert.Fail("no patients found - cannot run test");
                var newCondition = new Condition
                {
                    Subject = new ResourceReference
                    {
                        Reference = patients.Entry[0].Resource.Id
                    }
                };
                Client.Create(newCondition);
            }

            IEnumerable<Condition> conditionsForPatients = conditions.Entry.ByResourceType<Condition>()
                .Where(
                    c =>
                        c.Subject != null && new ResourceIdentity(c.Subject.Url).ResourceType == "Patient");

            //We want a condition on a patient that has a name, for the last test in this method.
            ResourceIdentity patientRef = null;
            Patient entry = null;
            string patFirstName = "";


            foreach (var cond in conditionsForPatients)
            {
                try
                {
                    patientRef = new ResourceIdentity(cond.Subject.Url);
                    entry = Client.Read<Patient>(patientRef);
                    patFirstName = entry.Name[0].Family.First();
                    if (!string.IsNullOrEmpty(patFirstName)) break;
                    
                }
                catch (Exception)
                {
                    // Apparently this patient has no name, try again.
                }
            }

            if (entry == null)
                Assert.Fail("failed to find patient condition is referring to");

            IEnumerable<Condition> allConditionsForThisPatient =
                conditionsForPatients.Where(c => c.Subject != null && c.Subject.Url == patientRef);
            int nrOfConditionsForThisPatient = allConditionsForThisPatient.Count();

            Bundle result = Client.Search<Condition>(new[] {"subject=" + patientRef});
            Assert.EntryIdsArePresentAndAbsoluteUrls(result);

            Assert.CorrectNumberOfResults(nrOfConditionsForThisPatient, result.Entry.Count(),
                "conditions for this patient (using subject=)");

            //Test for issue #6, https://github.com/furore-fhir/spark/issues/6
            result = Client.Search<Condition>(new[] {"subject:Patient=" + patientRef});
            Assert.EntryIdsArePresentAndAbsoluteUrls(result);

            Assert.CorrectNumberOfResults(nrOfConditionsForThisPatient, result.Entry.Count(),
                "conditions for this patient (using subject:Patient=)");

            result = Client.Search<Condition>(new[] {"subject._id=" + patientRef.Id});
            Assert.EntryIdsArePresentAndAbsoluteUrls(result);

            Assert.CorrectNumberOfResults(nrOfConditionsForThisPatient, result.Entry.Count(),
                "conditions for this patient (using subject._id=)");

            string param = "subject.name=" + patFirstName;

            result = Client.Search<Condition>(new[] {param});
            Assert.EntryIdsArePresentAndAbsoluteUrls(result);

            if (result.Entry.Count() == 0)
                Assert.Fail("failed to find any conditions (using subject.name)");

            
            string identifier = entry.Identifier[0].Value;
            result = Client.Search<Condition>(new[] {"subject.identifier=" + identifier});

            if (result.Entry.Count() == 0)
                Assert.Fail("failed to find any conditions (using subject.identifier)");
        }

        [SprinklerTest("SE06", "Search with includes")]
        public void SearchWithIncludes()
        {
            Bundle bundle = Client.Search<Condition>(new[] {"_include=Condition.subject"});

            IEnumerable<Patient> patients = bundle.Entry.ByResourceType<Patient>();
            Assert.IsTrue(patients.Count() > 0,
                "Search Conditions with _include=Condition.subject should have patients");
        }

        private string createObservation(decimal value)
        {
            var observation = new Observation();
            observation.Status = Observation.ObservationStatus.Preliminary;
            observation.Reliability = Observation.ObservationReliability.Questionable;
            observation.Code = new CodeableConcept("http://loinc.org", "2164-2");
            observation.Value = new Quantity
            {
                System = "http://unitsofmeasure.org",
                Value = value,
                Code = "mg",
                Units = "miligram"
            };
            observation.BodySite = new CodeableConcept("http://snomed.info/sct", "182756003");

            Observation entry = Client.Create(observation);
            return entry.Id;
        }

        [SprinklerTest("SE21", "Search for quantity (in observation) - precision tests")]
        public void SearchQuantity()
        {
            string id0 = createObservation(4.12345M);
            string id1 = createObservation(4.12346M);
            string id2 = createObservation(4.12349M);

            Bundle bundle = Client.Search("Observation", new[] {"value-quantity=4.1234||mg"});

            Assert.IsTrue(bundle.ContainsResource(id0), "Search on quantity value 4.1234 should return 4.12345");
            Assert.IsTrue(!bundle.ContainsResource(id1), "Search on quantity value 4.1234 should not return 4.12346");
            Assert.IsTrue(!bundle.ContainsResource(id2), "Search on quantity value 4.1234 should not return 4.12349");

            
        }

        [SprinklerTest("SE22", "Search for quantity (in observation) - operators")]
        public void SearchQuantityGreater()
        {
            string id0 = createObservation(4.12M);
            string id1 = createObservation(5.12M);
            string id2 = createObservation(6.12M);

            Bundle bundle = Client.Search("Observation", new[] {"value-quantity=>5||mg"});

            Assert.IsTrue(!bundle.ContainsResource(id0), "Search greater than quantity should not return lesser value.");
            Assert.IsTrue(bundle.ContainsResource(id1), "Search greater than quantity should return greater value");
            Assert.IsTrue(bundle.ContainsResource(id2), "Search greater than quantity should return greater value");
        }

        [SprinklerTest("SE23", "Search with quantifier :missing, on Patient.gender.")]
        public void SearchPatientByGenderMissing()
        {
            var patients = new List<Patient>();
            var filter = "family=BROOKS";
            Bundle bundle = Client.Search<Patient>(new[] { filter });
            while (bundle != null && bundle.Entry.ByResourceType<Patient>().Count() > 0)
            {
                patients.AddRange(bundle.Entry.ByResourceType<Patient>());
                bundle = Client.Continue(bundle);
            }
            IEnumerable<Patient> patientsNoGender = patients.Where(p => p.Gender == null);
            int nrOfPatientsWithMissingGender = patientsNoGender.Count();
            IEnumerable<Patient> actual =
                Client.Search<Patient>(new[] { "gender:missing=true", filter }, pageSize: 500).Entry.ByResourceType<Patient>();

            Assert.CorrectNumberOfResults(nrOfPatientsWithMissingGender, actual.Count(),
                "Expected {0} patients without gender, but got {1}.");
        }

        [SprinklerTest("SE24", "Search with non-existing parameter.")]
        public void SearchPatientByNonExistingParameter()
        {
            int nrOfAllPatients = Client.Search<Patient>().Entry.ByResourceType<Patient>().Count();
            Bundle actual = Client.Search("Patient", new[] {"noparam=nonsense"});
                //Obviously a non-existing search parameter
            int nrOfActualPatients = actual.Entry.ByResourceType<Patient>().Count();
            Assert.CorrectNumberOfResults(nrOfAllPatients, nrOfActualPatients,
                "Expected all patients ({0}) since the only search parameter is non-existing, but got {1}.");
            IEnumerable<OperationOutcome> outcomes = actual.Entry.ByResourceType<OperationOutcome>();
            
            //Assert.IsTrue(outcomes.Any(), "There should be an OperationOutcome.");
            // mh: No there should not. Not in DSTU-1.

            /*
            Assert.IsTrue(
                outcomes.Any(o => o.Resource.Issue.Any(i => i.Severity == OperationOutcome.IssueSeverity.Warning)),
                "With a warning in it.");
            */
        }

        [SprinklerTest("SE25", "Search with malformed parameter.")]
        public void SearchPatientByMalformedParameter()
        {
            int nrOfAllPatients = Client.Search<Patient>().Entry.Count();
            Bundle actual = null;
            Assert.Fails(Client, () => Client.Search("Patient", new[] {"...=test"}), out actual,
                HttpStatusCode.BadRequest);
            /* The statements below inspect the OperationOutcome in the result. But the FhirClient currently does not return any of the HttpStatusCode != OK.
            var nrOfActualPatients = actual.Entries.ByResourceType<Patient>().Count();
            HttpTests.AssertCorrectNumberOfResults(0, nrOfActualPatients, "Expected no patients ({0}) since the only search parameter is invalid, but got {1}.");
            TestResult.Assert(actual.Entries.ByResourceType<OperationOutcome>().Any(), "There should be an OperationOutcome.");
            */
        }
    }

    internal static class TestExtensions
    {
        public static bool CodeableConceptNull(this CodeableConcept cc)
        {
            return cc == null ||
                   cc.Text == null &&
                   (cc.Coding == null ||
                    cc.Coding.Count() == 0 ||
                    cc.Coding.All(c => c.Code == null));
        }
    }
}