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

        public void AddTestResult(TestResult testResult)
        {
            if (ShowStatusInConsole)
            {
                Console.Write(Resources.statusForCategoryCode, new string(' ', Console.WindowWidth - 1),
                    testResult.Category, testResult.Code);
            }
            ResultList.Add(testResult);
        }

        public void SerializeTo(TextWriter writer)
        {
            var serializer = new XmlSerializer(typeof (TestResults));
            serializer.Serialize(writer, this);
        }

        public void SerializeTo(TextWriter writer, TextReader stylesheet)
        {
            var xslt = new XslCompiledTransform();
            using (var xmlReader = XmlReader.Create(stylesheet))
            {
                xslt.Load(xmlReader);
            }
            var outputXml = new StringWriter();
            SerializeTo(outputXml);
            using (var inputXml = new StringReader(outputXml.ToString()))
            {
                var writerSettings = xslt.OutputSettings;
                xslt.Transform(XmlReader.Create(inputXml), XmlWriter.Create(writer, writerSettings));
            }
        }
    }
}