using Hl7.Fhir.Rest;
using Sprinkler.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sprinkler.Framework
{
    public static class Test
    {
        public static TestRunner CreateRunner(string uri, Action<TestResult> log)
        {
            FhirClient client = new FhirClient(uri);
            return new TestRunner(client, log);
        }

    }
}
