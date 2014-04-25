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


        private Bundle allPatients;

        [SprinklerTest("SE03", "Search patient resource on partial familyname")]
        public void SearchResourcesWithNameCriterium()
        {
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

        [SprinklerTest("SE04", "Search condition by subject (patient) reference")]
        public void SearchConditionByPatientReference()
        {
            var conditions = client.Search<Condition>();

            if (conditions.Entries.Count == 0)
                TestResult.Fail("no conditions found - cannot run test");
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

        private string CreateObservation(decimal value)
        {
            Observation observation = new Observation();
            observation.Name = new CodeableConcept("http://loinc.org", "2164-2");
            observation.Value = new Quantity() { System = new Uri("http://unitofmeasure.org"), Value = value, Units = "mmol" };
            observation.BodySite = new CodeableConcept("http://snomed.info/sct", "182756003");

            ResourceEntry<Observation> entry = client.Create<Observation>(observation, null, true);
            return entry.GetBasicId();
        }

        [SprinklerTest("SE21", "Search for quantity (in observation)")]
        public void SearchQuantity()
        {
            string id0 = CreateObservation(4.12345M);
            string id1 = CreateObservation(4.12346M);
            string id2 = CreateObservation(4.12349M);

            Bundle bundle = client.Search("Observation", new string[] { "value-quantity=4.1234||mmol" });

            TestResult.Assert(bundle.Has(id0), "Search on quantity value 4.1234 should return 4.12345");
            TestResult.Assert(!bundle.Has(id1), "Search on quantity value 4.1234 should not return 4.12346");
            TestResult.Assert(!bundle.Has(id2), "Search on quantity value 4.1234 should not return 4.12349");
        }
    }

}
