using Hl7.Fhir.Rest;
using Sprinkler.Framework;
using Sprinkler.Tests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sprinkler
{
    class ConsoleApplication
    {
        public static void registerTestException(Exception exception)
        {
            string indent = "    - ";

            while (exception != null)
            {
                if (!String.IsNullOrEmpty(exception.Message))
                    Console.WriteLine("{0}{1}", indent, exception.Message);

                if (exception is FhirOperationException)
                {
                    var foe = exception as FhirOperationException;
                    if (foe.Outcome != null && foe.Outcome.Issue != null)
                    {
                        int isuenr = 1;
                        foreach (var issue in foe.Outcome.Issue)
                        {
                            if (!issue.Details.StartsWith("Stack"))
                                Console.WriteLine(String.Format("{0}OperationOutcome.Issue({1}): {2}", indent, isuenr, issue.Details));
                            isuenr++;
                        }
                    }
                }

                exception = exception.InnerException;
            }
        }

        static void LogTest(TestResult test)
        {
            string title = string.Format("{0}: {1}", test.Code, test.Title);
            string outcome = test.Outcome.ToString().ToUpper();
            Console.WriteLine(string.Format("{0,-60} : {1}", title, outcome));

            if (test.Exception != null)
                registerTestException(test.Exception);
        }

        private static FhirClient getClient(string url)
        {
            if (!url.StartsWith("http:") && !url.StartsWith("https:"))
                url = "http://" + url;
            
            var endpoint = new Uri(url, UriKind.Absolute);
            return new FhirClient(endpoint);
        }

        public static void Run(string url)
        {
            FhirClient client = getClient(url);
            TestRunner tester = new TestRunner(client, LogTest);

            Console.WriteLine("Running all tests");
            tester.RunAll();
        }

        public static void Connectathon6(string url)
        {
            FhirClient client = getClient(url);
            TestRunner tester = new TestRunner(client, LogTest);

            Console.WriteLine("Running tests for CONNECTATHON 6 (may 2014)");
            //tester.Run<CreateUpdateDeleteTest>();
            tester.RunAll();
        }
    }
}
