using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Mime;
using Hl7.Fhir.Model;
using Hl7.Fhir.Client;

namespace Sprinkler
{
    [SprinklerTestModule("Conformance")]
    public class ConformanceTest
    {
        FhirClient client;

        public ConformanceTest(Uri fhirUri)
        {
            client = new FhirClient(fhirUri);
        }


        [SprinklerTest("get on /metadata")]
        public void GetConformanceUsingMetadata()
        {
           var entry = client.Conformance(useOptionsVerb: false);
           checkResultHeaders();
        }


        [SprinklerTest("get using OPTIONS")]
        public void GetConformanceUsingOptions()
        {
           client.Conformance(useOptionsVerb: true);
           checkResultHeaders();
        }

        private void checkResultHeaders()
        {
            HttpTests.AssertValidResourceContentTypePresent(client);
            HttpTests.AssertContentLocationValidIfPresent(client);
        }
 
    }
}
