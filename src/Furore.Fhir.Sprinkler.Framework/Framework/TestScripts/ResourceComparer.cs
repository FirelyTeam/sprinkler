using System;
using System.Collections.Generic;
using System.Reflection;
using Hl7.Fhir.Model;
using Hl7.Fhir.Serialization;

namespace Furore.Fhir.Sprinkler.Framework.Framework.TestScripts
{
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
                string fhirName = getFhirPropertyName(property);
                if (source != null && (source is Element || source is System.Collections.IList) && property.Name != "Extension")
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
                    else if (source is System.Collections.IList)
                        ok = compareLists(path, reportErrors, ok, property, source, target);
                    else if (source is Primitive)
                        ok = comparePrimitives(path, reportErrors, ok, property, (Primitive)source, (Primitive)target);
                    else if (!CompareElementsMin(path + "." + property.Name, source, target, reportErrors))
                        ok = false;

                }
            }
            return ok;
        }

        private bool comparePrimitives(String path, bool reportErrors, bool ok, PropertyInfo property, Primitive source, Primitive target)
        {
            String sourceValue = source == null ? null : source.GetValueAsString();
            String targetValue = target == null ? null : target.GetValueAsString();

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
            return ok;
        }

        private bool compareLists(String path, bool reportErrors, bool ok, PropertyInfo property, object source, object target)
        {
            System.Collections.IList sl = source as System.Collections.IList;
            System.Collections.IList tl = target as System.Collections.IList;

            //System.Collections.Generic.List<Element> sl = (List<Element>)source;
            //System.Collections.Generic.List<Element> tl = (List<Element>)target;
            if (sl.Count != 0)
            {
                String rules = readCodeExtension(sl[0] as Element, "http://www.healthintersections.com.au/fhir/ExtensionDefinition/list-rules");
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
                        int c = 0;
                        foreach (Element es in sl)
                        {
                            var found = false;
                            foreach (Element et in tl)
                            {
                                if (CompareElementsMin(path + "." + property.Name+"["+c+"]", es, et, false))
                                    found = true;
                            }
                            if (!found)
                            {
                                if (reportErrors)
                                    Errors.Add("Element '" + path + "." + property.Name + "[" + c + "] not found");
                                ok = false;
                            }
                            c++;
                        }
                    }
                }
                else if (rules == "exact" || rules == null)
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
                        int c = 0;
                        IEnumerator<Element> te = tl.GetEnumerator() as IEnumerator<Element>;
                        foreach (Element es in sl)
                        {
                            te.MoveNext();
                            if (!CompareElementsMin(path + "." + property.Name + "[" + c + "]", es, te.Current, true))
                                ok = false;
                            c++;
                        }
                    }
                }
                else if (rules == "open")
                {
                    // nothing to check - the list is allowed to contain anything
                    int count = readIntegerExtension(sl[0] as Element, "http://www.healthintersections.com.au/fhir/ExtensionDefinition/list-max");
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
                    throw new Exception("Unknown list rule type '" + rules + "' on Element '" + path + "." + property.Name);
            }
            return ok;
        }


        private string getFhirPropertyName(PropertyInfo property)
        {
            // why does this line (copied from elsewhere) not compile? Stupid....

            //var ea = (FhirElementAttribute) Attribute.GetCustomAttribute(property, typeof(FhirElementAttribute));
            //if (ea != null)
            //    return ea.Name;
            //else
                return null;
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
                return Guid.TryParse(value, out guidOutput);
            }
            else if (pattern == "%urn:uuid")
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
                return (value != null) || (value != "");
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
