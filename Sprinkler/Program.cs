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
using Hl7.Fhir.Rest;
using Hl7.Fhir.Support;
using Sprinkler.Framework;
using Sprinkler.Tests;

namespace Sprinkler
{
    class Program
    {       
        static void Main(string[] args)
        {
            //string FHIR_URL = "http://localhost.fiddler:1396/fhir";
            
            string FHIR_URL = "http://localhost:1396/fhir";

            //string FHIR_URL = "http://fhirlab.apphb.com/fhir";
            //string FHIR_URL = "http://localhost.:62124/fhir";
            
            //string FHIR_URL = "http://spark.furore.com/fhir";
            //string FHIR_URL = "http://hl7connect.healthintersections.com.au/svc/fhir";

            //string FHIR_URL = "http://fhir.healthintersections.com.au/open";

            //string FHIR_URL = "http://healthfire.duteaudesign.com/svc";
            //string FHIR_URL = "http://172.28.174.240:9556/svc/fhir";
            //string FHIR_URL = "http://nprogram.azurewebsites.net";
            //string FHIR_URL = "https://api.fhir.me";
            //string FHIR_URL = "http://12.157.84.95/OnlineFHIRService/diagnosticorder";
            //string FHIR_URL = "http://12.157.84.54:8080/fhir";
            //string FHIR_URL = "http://12.157.84.197:18080/fhir";

            if (args.Count() == 1) FHIR_URL = args[0];

            Console.WriteLine("Sprinkler v0.5 - Conformance test suite for FHIR 0.8 (DSTU1)");
            Console.WriteLine("Testing at server endpoint " + FHIR_URL);

            if (!FHIR_URL.StartsWith("http:") && !FHIR_URL.StartsWith("https:")) 
                FHIR_URL = "http://" + FHIR_URL;

            var fhirUri = new Uri(FHIR_URL, UriKind.Absolute);
            FhirClient client = new FhirClient(fhirUri);

            TestRunner tester = new TestRunner(client, LogTest);
            
            //tester.Run<TestTransactions>();
            tester.RunAll();

            Console.WriteLine();
            Console.WriteLine("Ready (press any key to continue)");
            Console.ReadKey();
        }

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
    
    }
}
