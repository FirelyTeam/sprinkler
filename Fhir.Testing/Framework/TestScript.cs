using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Hl7.Fhir.Model;
using Hl7.Fhir.Serialization;
using Hl7.Fhir.Rest;


namespace Fhir.Testing.Framework
{
    public class TestScript
    {
        private string filename;
        private FhirClient client;

        public TestScript(string filename)
        {
            this.filename = filename;
        }


        public bool WantSetup { get; set; }
        public string Base { get; set; }

        public void execute()
        {
            client = new FhirClient(Base);

            XmlDocument doc = new XmlDocument();
            doc.Load(filename);
            var root = doc.DocumentElement;
            if (root.NamespaceURI != "http://hl7.org/fhir" || root.Name != "TestScript")
                throw new Exception("Unrecognised start to script: expected http://hl7.org/fhir :: TestScript");
            String name = readChildValue(root, "name");
            Console.WriteLine("Execute Test Script: "+name);
            var child = root.FirstChild;
            while (child != null)
            {
                if (child.Name == "setup" && WantSetup)
                    executeSetup((XmlElement) child);
                if (child.Name == "suite")
                    executeSuite((XmlElement) child);
                child = child.NextSibling;
            }
            Console.WriteLine("Finish Test Script. Elapsed time = todo");
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
                    Console.WriteLine("  " + child.Attributes["id"].Value);
                    executeSetupAction((XmlElement) child);
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
                else if (child.NodeType == XmlNodeType.Element)
                  throw new Exception("Unexpected content");
                child = child.NextSibling;
            }

        }

        private void executeUpdate(XmlElement element)
        {
            String source = element.InnerXml;
            Resource res = FhirParser.ParseResourceFromXml(source);
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
                else if (child.NodeType == XmlNodeType.Element)
                    throw new Exception("Unexpected content");

                child = child.NextSibling;
            }

        }

        private void executeTest(XmlElement element)
        {
            String name = readChildValue(element, "name");
            Console.Write("  Test " + name+": ");

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

        private void executeOperation(XmlElement element)
        {
            String url = readChildValue(element, "url");
            XmlElement input = findChild(element, "input");
            Resource result;
            if (input != null) 
            {
                Parameters params = FhirParser.ParseResourceFromXml(input.InnerXml);
                result = client.Operation(url, params);
            }
            else
                result = client.Operation(url);
            XmlElement output = findChild(element, "output");
            XmlElement rules = findChild(element, "rules");
            output = nextElement(output);
            Resource expected = FhirParser.ParseResourceFromXml(output.OuterXml);
            var comp = new ResourceComparer();
            comp.Rules = rules.Attributes["value"].Value;
            comp.Expected = expected;
            comp.Observed = result;
            if (comp.execute()) 
                Console.WriteLine("passed");
            else
            {
                Console.WriteLine("failed");
                foreach (var s in comp.Errors)
                Console.WriteLine("  "+s);
            }
        }
    }

    public class ResourceComparer
    {
        public string Rules { get; set; }

        public Resource Expected { get; set; }

        public Resource Observed { get; set; }

        public List<string> Errors { get; private set; }

        public bool execute()
        {
            if (Rules != "min")
                throw new Exception("Unknown rule " + Rules);
            Errors.Clear();
            // given that it's "min" (only option allowed at this time), then our task is 
            //  * iterate the expected
            //  * anything in that, find it in the observed
            //  * check the value for fixed value or specified pattern
            //  * check for list management extensions

            return false;

        }

    }

}
