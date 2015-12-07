using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;
using Hl7.Fhir.Client;
using Hl7.Fhir.Model;
using Hl7.Fhir.Support;
using Hl7.Fhir.Support.Search;

namespace Sprinkler
{
    [SprinklerTestModule("Search")]
    public class SearchTest
    {
        FhirClient client;

        public SearchTest(Uri fhirUri)
        {
            client = new FhirClient(fhirUri);
        }

       // [SprinklerTest("search full system without criteria")]
        public void SearchWithoutCriteria()
        {
            var result = client.Search(count:10);
        }

        [SprinklerTest("search resource type without criteria")]
        public void SearchResourcesWithoutCriteria()
        {
            var result = client.Search(ResourceType.Patient,count: 10);

            if (result.Entries.Count == 0)
                TestResult.Fail("search did not return any results");

            if(result.Entries.Count > 10)
                TestResult.Fail("search returned more patients than specified in _count");

            if(result.Entries.ByResourceType<Patient>().Count() ==0)
                TestResult.Fail("search returned entries other than patient");

            allPatients = result;
        }

        private Bundle allPatients;

        [SprinklerTest("search patient resource on partial familyname")]
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

            var result = client.Search(ResourceType.Patient, "family", name);

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

        [SprinklerTest("search condition by subject (patient) reference")]
        public void SearchConditionByPatientReference()
        {
            var conditions = client.Search(ResourceType.Condition);

            if (conditions.Entries.Count == 0)
                TestResult.Fail("no conditions found - cannot run test");
            
            var condition = conditions.Entries.ByResourceType<Condition>()
                .Where(c => c.Resource.Subject != null && c.Resource.Subject.Type == "Patient")
                .First();

            var patientRef = new ResourceLocation(condition.Resource.Subject.Url);

            var patient = client.Read<Patient>(patientRef.Id);

            if(patient == null)
                TestResult.Fail("failed to find patient condition is referring to");

            var result = client.Search(ResourceType.Condition, "subject", "patient/" + patientRef.Id);
            if (result.Entries.Count() == 0)
                TestResult.Fail("failed to find any conditions (using subject=)");

            result = client.Search(ResourceType.Condition, "subject._id", patientRef.Id);
            if (result.Entries.Count() == 0)
                TestResult.Fail("failed to find any conditions (using subject._id=)");

            string patFirstName = patient.Resource.Name[0].Family.First().Substring(2, 3);
            var param = new SearchParam("subject.name", new StringParamValue(patFirstName));
            result = client.Search(ResourceType.Condition, new SearchParam[] { param });
            if (result.Entries.Count() == 0)
                TestResult.Fail("failed to find any conditions (using subject.name)");

            result = client.Search(ResourceType.Condition, "subject.identifier", patient.Resource.Identifier[0].Key);
            if (result.Entries.Count() == 0)
                TestResult.Fail("failed to find any conditions (using subject.identifier)");

        }
    }
}
