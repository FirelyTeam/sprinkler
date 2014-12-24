using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Hl7.Fhir.Model;
using Hl7.Fhir.Serialization;
using Hl7.Fhir.Rest;
using System.Reflection;


namespace Fhir.Testing.Framework
{
    public class TestScript
    {
        private FhirClient client;

        public bool WantSetup { get; set; }
        public string Base { get; set; }
        public string Filename { get; set; }

        public void execute()
        {
            client = new FhirClient(Base);

            XmlDocument doc = new XmlDocument();
            doc.Load(Filename);
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
                else if (child.NodeType == XmlNodeType.Element && !(child.Name == "name"))
                    throw new Exception("Unexpected content: "+child.Name);

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
                        result = client.Operation(url, paramlist);
                    }
                    else
                        result = client.Operation(url);
                }
                catch (FhirOperationException e)
                {
                    if (e.Outcome != null)
                        result = e.Outcome;
                    else
                        throw;
                }

                XmlElement output = findChild(element, "output");
                XmlElement rules = findChild(output, "rules");
                output = nextElement(rules);
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
                        Console.WriteLine("  " + s);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("failed");
                Console.WriteLine("  " + e.Message);
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
            Errors = new List<string>();

            if (Expected == null)
            {
                if (Observed == null)
                    return true;
                else
                {
                    Errors.Add("Found a resource of type '" + Observed.GetType().ToString() + " expecting no resource");
                    return false;
                }
            }
            if (Observed == null)
            {
                Errors.Add("Found no resource expecting a '" + Expected.GetType().ToString());
                return false;

            }

            if (Observed.GetType() != Expected.GetType())
            {
                Errors.Add("Found a resource of type '" + Observed.GetType().ToString() + " expecting a resource of type " + Expected.GetType().ToString());
                return false;
            }

            if (Rules != "min") // (only option allowed at this time)
                throw new Exception("Unknown rule " + Rules);
            return CompareElementsMin(Expected.ResourceType.ToString(), Expected, Observed, true);
        }

        private bool CompareElementsMin(String path, Object Expected, Object Observed, bool reportErrors)
        {
            var ok = true;
            // given that it's "min", then our task is 
            //  * iterate the expected
            //  * anything in that, find it in the observed
            //  * check the value for fixed value or specified pattern
            //  * check for list management extensions
            foreach (PropertyInfo property in Expected.GetType().GetProperties())
            {
                object source = property.GetValue(Expected, null);
                if (source != null && source is Element)
                {
                    object target = property.GetValue(Observed, null);
                    if (target == null)
                    {
                        if (reportErrors)
                            Errors.Add("Unable to find element '" + path + "." + property.Name + " on target");
                        ok = false;
                    }
                    else if (source.GetType() != target.GetType())
                    {
                        if (reportErrors)
                            Errors.Add("Element '" + path + "." + property.Name + ": expected " + source.GetType().ToString() + " but found " + target.GetType().ToString());
                        ok = false;
                    }
                    else if (source is List<Element>)
                    {
                        List<Element> sl = (List<Element>)source;
                        List<Element> tl = (List<Element>)target;
                        if (sl.Count == 0)
                        {
                            if (tl.Count > 0)
                            {
                                if (reportErrors)
                                    Errors.Add("Element '" + path + "." + property.Name + ": expected nothing but found something");
                                ok = false;
                            }
                        }
                        else
                        {
                            String rules = readCodeExtension(sl[0], "http://www.healthintersections.com.au/fhir/ExtensionDefinition/list-rules");
                            if (rules == "exact-no-order")
                            {
                                if (tl.Count != sl.Count)
                                {
                                    if (reportErrors)
                                        Errors.Add("Element '" + path + "." + property.Name + ": expected " + sl.Count.ToString() + " items but found " + tl.Count.ToString() + " items");
                                    ok = false;
                                }
                                else
                                {
                                    // for each item in the source list, there must be an item in the target that has the same properties. 
                                }

                            }
                            else if (rules == "open")
                            {
                                // nothing to check - the list is allowed to contain anything
                                int count = readIntegerExtension(sl[0], "http://www.healthintersections.com.au/fhir/ExtensionDefinition/list-max");
                                if (tl.Count > count)
                                {
                                    if (reportErrors)
                                        Errors.Add("Element '" + path + "." + property.Name + ": expected at most " + count + " items but found " + tl.Count.ToString() + " items");
                                    ok = false;
                                }
                            }
                            else if (rules == "empty")
                            {
                                if (tl.Count > 0)
                                {
                                    if (reportErrors)
                                        Errors.Add("Element '" + path + "." + property.Name + ": expected nothing but found something");
                                    ok = false;
                                }
                            }
                            else if (rules == "minimum-no-order")
                            {
                                // for each item in the source list, there must be an item in the target that has the same properties. 
                            }
                            else
                                throw new Exception("Unknown list rule type '" + rules + "'");
                        }

                    }
                    else if (source is Primitive)
                    {
                        String sourceValue = ((Primitive) source).GetValueAsString();
                        String targetValue = ((Primitive) target).GetValueAsString();
                        if (sourceValue == null)
                        {
                            String pattern = readStringExtension((Primitive)source, "http://www.healthintersections.com.au/fhir/ExtensionDefinition/pattern");
                            if (pattern == null)
                            {
                                if (sourceValue != null)
                                {
                                    if (reportErrors)
                                        Errors.Add("Element '" + path + "." + property.Name + ": value should be missing, but is '" + sourceValue + "'");
                                    ok = false;
                                }
                            }
                            else
                            {
                                if (!matchesPattern(pattern, targetValue))
                                {
                                    if (reportErrors)
                                        Errors.Add("Element '" + path + "." + property.Name + ": value '" + targetValue + "' is not valid according to specified pattern '" + pattern + "'");
                                    ok = false;
                                }
                            }
                        }
                        else
                        {
                            if (sourceValue != targetValue)
                            {
                                if (reportErrors)
                                    Errors.Add("Element '" + path + "." + property.Name + ": expected value '" + sourceValue + "' but found '" + targetValue + "'");
                                ok = false;
                            }
                        }
                    }
                    else if (!CompareElementsMin(path + "." + property.Name, source, target, reportErrors))
                        ok = false;

                }
            }
            return ok;
        }

        private int readIntegerExtension(Element element, string url)
        {
            foreach (Extension ex in element.Extension) 
            {
                if (ex.Url == url)
                    return ((Integer)ex.Value).Value.Value;
            }
            return 0;
        }

        private string readCodeExtension(Element element, string url)
        {
            foreach (Extension ex in element.Extension)
            {
                if (ex.Url == url)
                    return ((Code)ex.Value).Value;
            }
            return null;
        }

        private string readStringExtension(Element element, string url)
        {
            foreach (Extension ex in element.Extension)
            {
                if (ex.Url == url)
                    return ((FhirString)ex.Value).Value;
            }
            return null;
        }

        private bool matchesPattern(string pattern, string value)
        {
            if (pattern == "%uuid")
            {
                Guid guidOutput;
                return value.StartsWith("urn:uuid:") && Guid.TryParse(value.Substring(9), out guidOutput);
            }
            else if (pattern == "%now-ish")
            {
                var min = DateTimeOffset.Now.AddMinutes(-5);
                var max = DateTimeOffset.Now.AddMinutes(5);
                var v = new FhirDateTime(value).ToDateTimeOffset();
                var bef = (v.CompareTo(min) > 0);
                var aft = (v.CompareTo(max) < 0);
                return bef && aft;
            }
            else if (pattern == "!")
            {
                return (value == null) || (value == "");
            }
            else if (pattern.StartsWith("!"))
            {
                return value != pattern.Substring(1);
            }
            else
                throw new Exception("Unknown pattern '" + pattern + "'");
        }
    }

}
