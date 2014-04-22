/* 
 * Copyright (c) 2014, Furore (info@furore.com) and contributors
 * See the file CONTRIBUTORS for details.
 * 
 * This file is licensed under the BSD 3-Clause license
 * available at https://raw.github.com/furore-fhir/sprinkler/master/LICENSE
 */
using Hl7.Fhir.Rest;
using Hl7.Fhir.Support;
using Hl7.Fhir.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using Hl7.Fhir.Serialization;

namespace Sprinkler.Framework
{
    public static class DemoData
    {
        public static string GetDemoConn5ExampleBundleXml()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = "Sprinkler.conn5-21-example.xml";

            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            using (StreamReader reader = new StreamReader(stream))
            {
                return reader.ReadToEnd();
            }
        }

        public static Bundle GetDemoConn5ExampleBundle()
        {
            string xml = GetDemoConn5ExampleBundleXml();
            var bundle = FhirParser.ParseBundleFromXml(xml);
            return bundle;
        }

        public static string GetDemoConn5CidExampleBundleXml()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = "Sprinkler.conn5-21-cid-example.xml";

            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            using (StreamReader reader = new StreamReader(stream))
            {
                return reader.ReadToEnd();
            }
        }

        public static Bundle GetDemoConn5CidExampleBundle()
        {
            string xml = GetDemoConn5CidExampleBundleXml();
            var bundle = FhirParser.ParseBundleFromXml(xml);
            return bundle;
        }

        public static string GetDemoXdsBundleXml()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = "Sprinkler.xds-example.xml";

            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            using (StreamReader reader = new StreamReader(stream))
            {
                return reader.ReadToEnd();
            }
        }

        public static Bundle GetDemoXdsBundle()
        {
            string xml = GetDemoXdsBundleXml();
            var bundle = FhirParser.ParseBundleFromXml(xml);
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
            var bundle = FhirParser.ParseBundleFromXml(xml);
            return bundle;
        }

        public static string GetDemoDocumentXml()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = "Sprinkler.conn5-21-example.xml";

            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            using (StreamReader reader = new StreamReader(stream))
            {
                return reader.ReadToEnd();
            }
        }

        public static string GetDemoPatientXml()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = "Sprinkler.patient-example.xml";

            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            using (StreamReader reader = new StreamReader(stream))
            {
                return reader.ReadToEnd();
            }
        }


        public static string GetDemoPatientJson()
        {
            var pat = GetDemoPatient();

            var json = FhirSerializer.SerializeResourceToJson(pat);
            return json;
        }

        public static Binary GetDemoBinary()
        {
            var pat = DemoData.GetDemoPatient();
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
            var pat = (Patient)FhirParser.ParseResourceFromXml(xml);
            return pat;
        }
    }

}
