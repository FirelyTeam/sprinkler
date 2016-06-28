using System;
using System.Diagnostics;
using System.Xml;
using Hl7.Fhir.Model;
using Hl7.Fhir.Rest;
using Hl7.Fhir.Serialization;

namespace Furore.Fhir.Sprinkler.Framework.Framework.TestScripts
{
    public class TestScript
    {
        private FhirClient client;

        public bool WantSetup { get; set; }
        public string Base { get; set; }
        public string Filename { get; set; }
        public string TestId { get; set; }

        private int total = 0;
        private int fail = 0;

        public void execute()
        {
            client = new FhirClient(Base);

            XmlDocument doc = new XmlDocument();
            doc.Load(Filename);
            var root = doc.DocumentElement;
            if (root.NamespaceURI != "http://hl7.org/fhir" || root.Name != "TestScript")
                throw new Exception("Unrecognised start to script: expected http://hl7.org/fhir :: TestScript");
            String name = readChildValue(root, "name");
            Console.WriteLine("Execute Test Script: " + name);
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();

            var child = root.FirstChild;
            while (child != null)
            {
                if (child.Name == "setup" && WantSetup)
                    executeSetup((XmlElement)child);
                if (child.Name == "suite")
                    executeSuite((XmlElement)child);
                child = child.NextSibling;
            }
            stopWatch.Stop();
            Console.WriteLine("Finish Test Script. Elapsed time = " + display(stopWatch));
            Console.WriteLine("Summary: " + total + " tests, " + (total - fail) + " passed, " + fail + " failed");
            Console.WriteLine("");
            Console.WriteLine("Press Any Key to Close");
            Console.ReadKey();
        }

        private string readChildValue(XmlElement element, string name)
        {
            var child = element.FirstChild;
            while (child != null)
            {
                if (child.Name == name)
                    return child.Attributes["value"].Value;
                child = child.NextSibling;
            }
            return null;
        }

        private XmlElement findChild(XmlElement element, string name)
        {
            var child = element.FirstChild;
            while (child != null)
            {
                if (child.Name == name)
                    return (XmlElement)child;
                child = child.NextSibling;
            }
            return null;
        }

        private XmlElement nextElement(XmlElement element)
        {
            var child = element.NextSibling;
            while (child != null)
            {
                if (child.NodeType == XmlNodeType.Element)
                    return (XmlElement)child;
                child = child.NextSibling;
            }
            return null;
        }

        private void executeSetup(XmlElement element)
        {
            Console.WriteLine("Setup");
            var child = element.FirstChild;
            while (child != null)
            {
                if (child.Name == "action")
                {
                    Stopwatch stopWatch = new Stopwatch();
                    stopWatch.Start();
                    Console.Write("  " + readChildValue(child as XmlElement, "name"));
                    executeSetupAction((XmlElement)child);
                    Console.WriteLine(" (" + display(stopWatch) + ")");
                }
                else if (child.NodeType == XmlNodeType.Element)
                    throw new Exception("Unexpected content");

                child = child.NextSibling;
            }

        }

        private void executeSetupAction(XmlElement element)
        {
            var child = element.FirstChild;
            while (child != null)
            {
                if (child.Name == "update")
                {
                    executeUpdate((XmlElement) child);
                }
                else if (child.NodeType == XmlNodeType.Element && child.Name != "name")
                    throw new Exception("Unexpected content: "+child.Name);
                child = child.NextSibling;
            }

        }

        private void executeUpdate(XmlElement element)
        {
            String source = element.InnerXml;
            Resource res = FhirParser.ParseResourceFromXml(source);
            res.ResourceBase = new Uri(Base);
            client.Update(res);
            
        }

        private void executeSuite(XmlElement element)
        {
            String name = readChildValue(element, "name");
            Console.WriteLine("Execute Suite: " + name);

            var child = element.FirstChild;
            while (child != null)
            {
                if (child.Name == "test")
                {
                    executeTest((XmlElement)child);
                }
                else if (child.NodeType == XmlNodeType.Element && !(child.Name == "name" || child.Name == "description"))
                    throw new Exception("Unexpected content: "+child.Name);

                child = child.NextSibling;
            }

        }

        private void executeTest(XmlElement element)
        {
            String name = readChildValue(element, "name");
            String id = element.Attributes["id"].Value;
            if ((TestId == null) || (TestId == id))
            {
                Console.Write("  Test " + name + (id != null ? "["+id+"]" : "") +": ");
                total++;

                var child = element.FirstChild;
                while (child != null)
                {
                    if (child.Name == "operation")
                    {
                        executeOperation((XmlElement)child);
                    }
                    else if (child.NodeType == XmlNodeType.Element && !(child.Name == "name" || child.Name == "description"))
                        throw new Exception("Unexpected content");

                    child = child.NextSibling;
                }
            }
        }

        private void executeOperation(XmlElement element)
        {
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();
            try
            {
                String url = readChildValue(element, "url");
                XmlElement input = findChild(element, "input");
                Resource result;
                try
                {
                    if (input != null)
                    {
                        Parameters paramlist = (Parameters)FhirParser.ParseResourceFromXml(input.InnerXml);
                        var name = readChildValue(element,"name");
                        result = client.Operation(new ResourceIdentity(url), name, paramlist);
                    }
                    else
                        result = client.Get(url);
                }
                catch (FhirOperationException e)
                {
                    if (e.Outcome != null)
                        result = e.Outcome;
                    else
                        throw;
                }
                stopWatch.Stop();

                XmlElement output = findChild(element, "output");
                XmlElement rules = findChild(output, "rules");
                output = nextElement(rules);
                Resource expected = FhirParser.ParseResourceFromXml(output.OuterXml);
                var comp = new ResourceComparer();
                comp.Rules = rules.Attributes["value"].Value;
                comp.Expected = expected;
                comp.Observed = result;
                if (comp.execute())
                    Console.WriteLine("passed" + " (" + display(stopWatch) + ")");
                else
                {
                    fail++;
                    Console.WriteLine("failed" + " (" + display(stopWatch) + ")");
                    foreach (var s in comp.Errors)
                        Console.WriteLine("    " + s);
                }
            }
            catch (Exception e)
            {
                if (stopWatch.IsRunning)
                    stopWatch.Stop();
                Console.WriteLine("failed" + " (" + display(stopWatch) + ")");
                Console.WriteLine("    " + e.Message);
                fail++;
            }

        }

        private string display(Stopwatch stopWatch)
        {
            TimeSpan ts = stopWatch.Elapsed;


            return ""+(ts.Hours * 3600 + ts.Minutes * 60 + ts.Seconds)+"."+ ts.Milliseconds+" sec";
        }
    }
}