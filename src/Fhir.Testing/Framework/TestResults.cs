/* 
 * Copyright (c) 2014, Furore (info@furore.com) and contributors
 * See the file CONTRIBUTORS for details.
 * 
 * This file is licensed under the BSD 3-Clause license
 * available at https://raw.github.com/furore-fhir/sprinkler/master/LICENSE
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using System.Xml.Xsl;
using Hl7.Fhir.Rest;
//using Sprinkler.Properties;

namespace Sprinkler.Framework
{
    /*
    [Serializable]
    public class TestResults
    {
        public TestResults()
        {
            ResultList = new List<TestResult>();
        }

        public TestResults(string title, bool showStatusInConsole = false) : this()
        {
            Title = title;
            ShowStatusInConsole = showStatusInConsole;
        }

        [XmlIgnore]
        public bool ShowStatusInConsole { get; set; }

        public string Title { get; set; }
        public List<TestResult> ResultList { get; set; }

        // logging to Console
        public static void LogTestExceptionToConsole(Exception exception)
        {
            const string indent = "    - ";

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
                                Console.WriteLine(String.Format("{0}OperationOutcome.Issue({1}): {2}", indent, isuenr,
                                    issue.Details));
                            isuenr++;
                        }
                    }
                }

                exception = exception.InnerException;
            }
        }

        private static void LogTest(TestResult test, bool showAllInConsole)
        {
            if (showAllInConsole)
            {
                string s = "{0}\nTested module:[{1}] method:[{2}]";
                Console.Write(s, new string(' ', Console.WindowWidth - 1),
                    test.Category, test.Code);
            }
            else
            {
                var title = string.Format("{0}: {1}", test.Code, test.Title);
                var outcome = test.Outcome.ToString().ToUpper();
                Console.WriteLine(string.Format("{0,-60} : {1}", title, outcome));

                if (test.Exception != null)
                    LogTestExceptionToConsole(test.Exception);
            }
        }

        public void AddTestResult(TestResult testResult)
        {
            LogTest(testResult, ShowStatusInConsole);
            ResultList.Add(testResult);
        }

        

        
    }
    */
}