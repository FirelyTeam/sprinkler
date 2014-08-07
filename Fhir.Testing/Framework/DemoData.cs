/* 
 * Copyright (c) 2014, Furore (info@furore.com) and contributors
 * See the file CONTRIBUTORS for details.
 * 
 * This file is licensed under the BSD 3-Clause license
 * available at https://raw.github.com/furore-fhir/sprinkler/master/LICENSE
 */

using System.IO;
using System.Reflection;
using Hl7.Fhir.Model;
using Hl7.Fhir.Serialization;

namespace Sprinkler.Framework
{
    public static class DemoData
    {
        public static string GetDemoConn5ExampleBundleXml()
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            string resourceName = "Sprinkler.conn5-21-example.xml";

            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            using (var reader = new StreamReader(stream))
            {
                return reader.ReadToEnd();
            }
        }

        public static Bundle GetDemoConn5ExampleBundle()
        {
            string xml = GetDemoConn5ExampleBundleXml();
            Bundle bundle = FhirParser.ParseBundleFromXml(xml);
            return bundle;
        }

        public static string GetDemoConn5CidExampleBundleXml()
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            string resourceName = "Sprinkler.conn5-21-cid-example.xml";

            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            using (var reader = new StreamReader(stream))
            {
                return reader.ReadToEnd();
            }
        }

        public static Bundle GetDemoConn5CidExampleBundle()
        {
            string xml = GetDemoConn5CidExampleBundleXml();
            Bundle bundle = FhirParser.ParseBundleFromXml(xml);
            return bundle;
        }

        public static string GetDemoXdsBundleXml()
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            string resourceName = "Sprinkler.xds-example.xml";

            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            using (var reader = new StreamReader(stream))
            {
                return reader.ReadToEnd();
            }
        }

        public static Bundle GetDemoXdsBundle()
        {
            string xml = GetDemoXdsBundleXml();
            Bundle bundle = FhirParser.ParseBundleFromXml(xml);
            return bundle;
        }

        //public static string GetDemoCdaXml()
        //{
        //    var assembly = Assembly.GetExecutingAssembly();
        //    var resourceName = "Sprinkler.cda-demo.xml";

        //    using (Stream stream = assembly.GetManifestResourceStream(resourceName))
        //    using (StreamReader reader = new StreamReader(stream))
        //    {
        //        return reader.ReadToEnd();
        //    }
        //}

        //public static byte[] GetDemoCdaBytes()
        //{
        //    XDocument d = XDocument.Parse(GetDemoCdaXml());
        //    var mem = new MemoryStream();
        //    var writer = XmlWriter.Create(mem);

        //    d.WriteTo(writer);

        //    return mem.ToArray();
        //}

        public static Bundle GetDemoDocumentBundle()
        {
            string xml = GetDemoDocumentXml();
            Bundle bundle = FhirParser.ParseBundleFromXml(xml);
            return bundle;
        }

        public static string GetDemoDocumentXml()
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            string resourceName = "Sprinkler.conn5-21-example.xml";

            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            using (var reader = new StreamReader(stream))
            {
                return reader.ReadToEnd();
            }
        }

        public static string GetDemoPatientXml()
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            string resourceName = "Sprinkler.patient-example.xml";

            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            using (var reader = new StreamReader(stream))
            {
                return reader.ReadToEnd();
            }
        }

        public static string GetDemoPatientJson()
        {
            Patient pat = GetDemoPatient();

            string json = FhirSerializer.SerializeResourceToJson(pat);
            return json;
        }

        public static Binary GetDemoBinary()
        {
            Patient pat = GetDemoPatient();
            var bin = new Binary();

            // NB: in the default patient-example there is no photo element.
            // Copy the photo element from the current example when replacing this file!
            bin.Content = pat.Photo[0].Data;

            bin.ContentType = pat.Photo[0].ContentType;

            return bin;
        }

        public static Patient GetDemoPatient()
        {
            string xml = GetDemoPatientXml();
            var pat = (Patient) FhirParser.ParseResourceFromXml(xml);
            return pat;
        }
    }
}