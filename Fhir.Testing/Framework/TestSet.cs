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
using Hl7.Fhir.Rest;
using Sprinkler.Framework;

namespace Sprinkler.Framework
{
    /*public class TestSet
    {
        private TestRunner testRunner;

        // Factory
        public static TestSet NewInstance(string url)
        {
            var client = GetClient(url);
            var testRunner = new TestRunner(client);
            return new TestSet(testRunner);
        }

        private TestSet(TestRunner testRunner)
        {
            this.testRunner = testRunner;
        }

        /*
        private static Action<TestResult> LogTest(TestResults results)
        {
            return results.AddTestResult;
        }
        

        // Run method
        public void Run(TestResults results, params string[] tests)
        {
            testRunner.Run(tests, LogTest(results));
        }

        private static FhirClient GetClient(string url)
        {
            if (!IsValidUrl(url))
                throw new ArgumentException("Not a valid url");
            var endpoint = new Uri(url, UriKind.Absolute);
            return new FhirClient(endpoint);
        }

        public static bool IsValidUrl(string url)
        {
            return Uri.IsWellFormedUriString(url, UriKind.Absolute);
        }

        

    }
    */

}