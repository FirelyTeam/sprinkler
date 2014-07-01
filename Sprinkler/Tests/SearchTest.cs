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
using System.Text;
using System.Net;
using System.IO;
using Hl7.Fhir.Rest;
using Hl7.Fhir.Model;
using Hl7.Fhir.Support;
using Sprinkler.Framework;

namespace Sprinkler.Tests
{
    [SprinklerTestModule("Search")]
    public class SearchTest : SprinklerTestClass
    {
        private Bundle allPatients;

       // [SprinklerTest("search full system without criteria")]
        public void SearchWithoutCriteria()
        {
            var result = client.WholeSystemSearch(pageSize:10);
        }

        [SprinklerTest("SE01", "Search resource type without criteria")]
        public void SearchResourcesWithoutCriteria()
        {
            var result = client.Search<Patient>(pageSize: 10);
            HttpTests.AssertEntryIdsArePresentAndAbsoluteUrls(result);

            if (result.Entries.Count == 0)
                TestResult.Fail("search did not return any results");

            if(result.Entries.Count > 10)
                TestResult.Fail("search returned more patients than specified in _count");

            if(result.Entries.ByResourceType<Patient>().Count() ==0)
                TestResult.Fail("search returned entries other than patient");

            allPatients = result;
        }

        [SprinklerTest("SE02", "Search on non-existing resource")]
        public void TrySearchNonExistingResource()
        {
            HttpTests.AssertFail(client, () => client.Search("Nonexistingnonpatientresource"), HttpStatusCode.NotFound);
        }


        

        [SprinklerTest("SE03", "Search patient resource on partial familyname")]
        public void SearchResourcesWithNameCriterium()
        {
            if (allPatients == null) TestResult.Skip();
            // First create a search argument: any family name present in the
            // previous unlimited search result that has at least 5 characters
            var name = allPatients.Entries.ByResourceType<Patient>()
                .Where(p => p.Resource.Name != null)
                .SelectMany(p => p.Resource.Name)
                .Where(hn => hn.Family != null)
                .SelectMany(hn => hn.Family)
                .Where(s => s.Length > 5).First();

            // Take the first three characters
            name = name.Substring(0, 3);

            
            var result = client.Search<Patient>(new string[] { "family="+name });
            HttpTests.AssertEntryIdsArePresentAndAbsoluteUrls(result);

            if (result.Entries.Count == 0)
                TestResult.Fail("search did not return any results");

            // Each patient returned should have a family name with the
            // criterium
            var names = result.Entries.ByResourceType<Patient>()
                .Where(p=>p.Resource.Name != null)
                   .SelectMany(p => p.Resource.Name)
                    .Where(hn => hn.Family != null)
                        .SelectMany(hn => hn.Family);

            var correct = result.Entries.ByResourceType<Patient>()
                .All(p => p.Resource.Name != null &&
                    p.Resource.Name.Where(hn => hn.Family != null)
                        .SelectMany(hn => hn.Family)
                        .Any(s => s.ToLower().Contains(name.ToLower())));

            if (!correct)
                TestResult.Fail("search result contains patients that do not match the criterium");
        }



        [SprinklerTest("SE04", "Search patient resource on given name")]
        public void SearchPatientOnGiven()
        {
            Patient patient = Utils.NewPatient("Adams", "Fester");
            client.Create<Patient>(patient);
            Bundle bundle = client.Search<Patient>(new string[] { "given=Fester" });

            bool found = bundle.ResourcesOf<Patient>().Where(p => p.HasGiven("Fester")).Count() > 0;

            TestResult.Assert(found, "Patient was not found with given name");
            
        }

        [SprinklerTest("SE05", "Search condition by subject (patient) reference")]
        public void SearchConditionByPatientReference()
        {
            var conditions = client.Search<Condition>();

            if (conditions.Entries.Count == 0)
            {
                var patients = client.Search<Patient>();
                if (patients.Entries.Count == 0)
                    TestResult.Fail("no patients found - cannot run test");
                var newCondition = new Condition
                {
                    Subject = new ResourceReference
                    {
                        Reference = patients.Entries[0].Id.ToString()
                    }
                };
                client.Create(newCondition);
            }
            
            var condition = conditions.Entries.ByResourceType<Condition>()
                .Where(c => c.Resource.Subject != null && new ResourceIdentity(c.Resource.Subject.Url).Collection == "Patient") 
                .First();

            var patientRef = new ResourceIdentity(condition.Resource.Subject.Url);

            var patient = client.Read<Patient>(patientRef);

            if(patient == null)
                TestResult.Fail("failed to find patient condition is referring to");

            var result = client.Search<Condition>(new string[] { "subject=" + patientRef });
            HttpTests.AssertEntryIdsArePresentAndAbsoluteUrls(result);

            if (result.Entries.Count() == 0)
                TestResult.Fail("failed to find any conditions (using subject=)");

            result = client.Search<Condition>(new string[] { "subject._id=" + patientRef.Id });
            HttpTests.AssertEntryIdsArePresentAndAbsoluteUrls(result);

            if (result.Entries.Count() == 0)
                TestResult.Fail("failed to find any conditions (using subject._id=)");

            string patFirstName = patient.Resource.Name[0].Family.First();
            //.Substring(2, 3);
            var param = "subject.name=" + patFirstName;
            
            result = client.Search<Condition>(new string[] { param });
            HttpTests.AssertEntryIdsArePresentAndAbsoluteUrls(result);

            if (result.Entries.Count() == 0)
                TestResult.Fail("failed to find any conditions (using subject.name)");

            string identifier = patient.Resource.Identifier[0].Value;
            result = client.Search<Condition>(new string[] {  "subject.identifier=" + identifier });
            
            if (result.Entries.Count() == 0)
                TestResult.Fail("failed to find any conditions (using subject.identifier)");
        }

        [SprinklerTest("SE06", "Search with includes")]
        public void SearchWithIncludes()
        {
            Bundle bundle  = client.Search<Condition>(new string[] { "_include=Condition.subject" });

            var patients = bundle.Entries.ByResourceType<Patient>();
            TestResult.Assert(patients.Count() > 0, "Search Conditions with _include=Condition.subject should have patients");
        }

        private string createObservation(decimal value, string units = "mmol")
        {
            Observation observation = new Observation();
            observation.Status = Observation.ObservationStatus.Preliminary;
            observation.Reliability = Observation.ObservationReliability.Questionable;
            observation.Name = new CodeableConcept("http://loinc.org", "2164-2");
            observation.Value = new Quantity() { System = new Uri("http://unitsofmeasure.org"), Value = value, Units = units, Code = units };
            observation.BodySite = new CodeableConcept("http://snomed.info/sct", "182756003");

            ResourceEntry<Observation> entry = client.Create<Observation>(observation, null, true);
            return entry.GetBasicId();
        }

        [SprinklerTest("SE21", "Search for quantity (in observation) - precision tests")]
        public void SearchQuantity()
        {
            string id0 = createObservation(4.12345M);
            string id1 = createObservation(4.12346M);
            string id2 = createObservation(4.12349M);

            Bundle bundle = client.Search("Observation", new string[] { "value-quantity=4.1234||mmol" });

            TestResult.Assert(bundle.Has(id0), "Search on quantity value 4.1234 should return 4.12345");
            TestResult.Assert(!bundle.Has(id1), "Search on quantity value 4.1234 should not return 4.12346");
            TestResult.Assert(!bundle.Has(id2), "Search on quantity value 4.1234 should not return 4.12349");
        }

        [SprinklerTest("SE22", "Search for quantity (in observation) - operators")]
        public void SearchQuantityGreater()
        {
            string id0 = createObservation(4.12M);
            string id1 = createObservation(5.12M);
            string id2 = createObservation(6.12M);

            Bundle bundle = client.Search("Observation", new string[] { "value-quantity=>5||mmol" });

            TestResult.Assert(!bundle.Has(id0), "Search greater than quantity should not return lesser value.");
            TestResult.Assert(bundle.Has(id1), "Search greater than quantity should return greater value");
            TestResult.Assert(bundle.Has(id2), "Search greater than quantity should return greater value");
        }

        [SprinklerTest("SE23", "Search for quantity (in observation) - unit conversion")]
        public void SearchQuantityWithUcum()
        {
            string id;
            Bundle bundle;

            id = createObservation(4, "kg");
            bundle = client.Search("Observation", new string[] { "value-quantity=4000||g" });
            TestResult.Assert(!bundle.Has(id), "Search on quantity result should not return an observation with a less precise value.");
            
            id = createObservation(4000, "g");
            bundle = client.Search("Observation", new string[] { "value-quantity=4||kg" });
            TestResult.Assert(bundle.Has(id), "Search on quantity should return an observation with a more precise value.");

            id = createObservation(7, "N");
            bundle = client.Search("Observation", new string[] { "value-quantity=7||kg.m/s2" });
            TestResult.Assert(bundle.Has(id), "Search on quantity should return an observation with a more precise value.");
        }
    }

}
