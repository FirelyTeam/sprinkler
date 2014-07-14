/* 
 * Copyright (c) 2014, Furore (info@furore.com) and contributors
 * See the file CONTRIBUTORS for details.
 * 
 * This file is licensed under the BSD 3-Clause license
 * available at https://raw.github.com/furore-fhir/sprinkler/master/LICENSE
 */
using System;
using System.Globalization;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using Hl7.Fhir.Rest;

namespace Sprinkler.Framework
{
    public enum TestOutcome
    {
        Success,
        Fail,
        Skipped
    }

    [Serializable]
    public class TestFailedException : Exception, IXmlSerializable
    {
        public TestOutcome Outcome = TestOutcome.Fail;

        // parameterless constructor for serialization
        public TestFailedException()
        {
        }

        // overloaded ctor: message
        public TestFailedException(string message) : base(message)
        {
        }

        // overloaded ctor: outcome
        public TestFailedException(TestOutcome outcome)
            : base(outcome.ToString())
        {
            Outcome = outcome;
        }

        // overloaded ctor: message, inner exception
        public TestFailedException(string message, Exception inner) : base(message, inner)
        {
        }

        public XmlSchema GetSchema()
        {
            return null;
        }

        public void ReadXml(XmlReader reader)
        {
            throw new NotImplementedException();
        }

        // used by serialization
        public void WriteXml(XmlWriter writer)
        {
            WriteXml(writer, this);
        }

        private static void WriteXml(XmlWriter writer, Exception exception)
        {
            writer.WriteStartElement("Message");
            if (exception is FhirOperationException)
            {
                var foe = exception as FhirOperationException;
                if (foe.Outcome != null && foe.Outcome.Issue != null)
                {
                    var isuenr = 1;
                    foreach (var issue in foe.Outcome.Issue)
                    {
                        if (!issue.Details.StartsWith("Stack"))
                        {
                            writer.WriteStartElement("Stack");
                            writer.WriteAttributeString("issue", isuenr.ToString(CultureInfo.CurrentCulture));
                            writer.WriteString(issue.Details);
                            writer.WriteEndElement();
                        }
                        isuenr++;
                    }
                }
            }
            else
            {
                writer.WriteString(exception.Message);
            }
            exception = exception.InnerException;
            if (exception != null)
            {
                writer.WriteStartElement("Inner");
                WriteXml(writer, exception);
                writer.WriteEndElement();
            }
            writer.WriteEndElement();
        }
    }

    public class TestResult
    {
        public string Category { get; set; }
        public string Code { get; set; }
        public string Title { get; set; }
        public TestOutcome Outcome { get; set; }
        public TestFailedException Exception { get; set; }

        public static void Skip()
        {
            throw new TestFailedException(TestOutcome.Skipped);
        }

        public static void Fail(string message)
        {
            throw new TestFailedException(message);
        }

        public static void Fail(Exception inner, string message = "Exception caught")
        {
            throw new TestFailedException(message, inner);
        }

        public static void Assert(bool assertion, string message)
        {
            if (!assertion)
                Fail(message);
        }
    }
}