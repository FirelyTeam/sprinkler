using System;
using System.Collections.Generic;
using System.Linq;
using Hl7.Fhir.Rest;
using Sprinkler.Framework;

namespace Sprinkler
{
    public class TestSet
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

        public static bool IsValidUrl(string url) {
            return Uri.IsWellFormedUriString(url, UriKind.Absolute);
        }

        // A list of groups, each of them holds a list of tests
        public static List<Tuple<string, List<Tuple<string, string>>>> GetTestModules()
        {
            List<Tuple<string, List<Tuple<string, string>>>> dictModulesCodes =
                new List<Tuple<string, List<Tuple<string, string>>>>();
            var testclasses = TestRunner.GetTestClasses();

            foreach (var testclass in testclasses)
            {
                var testmethods =
                    TestRunner.GetTestMethods(testclass)
                        .Select(SprinklerTestAttribute.AttributeOf)
                        .Select(methodAttr => Tuple.Create(methodAttr.Code, methodAttr.Title))
                        .OrderBy(el=>el.Item1)
                        .ToList();
                var moduleAttr = SprinklerTestModuleAttribute.AttributeOf(testclass);
                dictModulesCodes.Add(Tuple.Create<string, List<Tuple<string, string>>>(moduleAttr.Name, testmethods));
            }
            return dictModulesCodes;
        }

    }

}