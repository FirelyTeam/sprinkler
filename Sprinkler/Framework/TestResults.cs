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
using Sprinkler.Properties;

namespace Sprinkler.Framework
{
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
        public static void RegisterTestException(Exception exception)
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

        private static void LogTest(TestResult test)
        {
            var title = string.Format("{0}: {1}", test.Code, test.Title);
            var outcome = test.Outcome.ToString().ToUpper();
            Console.WriteLine(string.Format("{0,-60} : {1}", title, outcome));

            if (test.Exception != null)
                RegisterTestException(test.Exception);
        }


        public void AddTestResult(TestResult testResult)
        {
            if (ShowStatusInConsole)
            {
                Console.Write(Resources.statusForCategoryCode, new string(' ', Console.WindowWidth - 1),
                    testResult.Category, testResult.Code);
            }
            else
            {
                LogTest(testResult);
            }
            ResultList.Add(testResult);
        }

        public void SerializeTo(TextWriter writer, TextReader stylesheet = null)
        {
            var serializer = new XmlSerializer(typeof (TestResults));
            if (stylesheet == null)
            {
                serializer.Serialize(writer, this);
            }
            else
            {
                SerializeAndTransform(writer, stylesheet, serializer);
            }
        }

        private void SerializeAndTransform(TextWriter writer, TextReader stylesheet, XmlSerializer serializer)
        {
            var outputXml = new StringWriter();
            serializer.Serialize(outputXml, this);
            var xslt = new XslCompiledTransform();
            using (var xmlReader = XmlReader.Create(stylesheet))
            {
                xslt.Load(xmlReader);
            }
            using (var inputXml = new StringReader(outputXml.ToString()))
            {
                var writerSettings = xslt.OutputSettings;
                xslt.Transform(XmlReader.Create(inputXml), XmlWriter.Create(writer, writerSettings));
            }
        }
    }
}