using Hl7.Fhir.Model;
using Hl7.Fhir.Parsers;
using Hl7.Fhir.Serializers;
using Hl7.Fhir.Support;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace Sprinkler
{
    public static class DemoData
    {
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
            var errors = new ErrorList();

            var bundle = FhirParser.ParseBundleFromXml(xml, errors);
            return bundle;
        }

        public static string GetDemoCdaXml()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = "Sprinkler.cda-demo.xml";

            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            using (StreamReader reader = new StreamReader(stream))
            {
                return reader.ReadToEnd();
            }
        }

        public static byte[] GetDemoCdaBytes()
        {
            XDocument d = XDocument.Parse(GetDemoCdaXml());
            var mem = new MemoryStream();
            var writer = XmlWriter.Create(mem);

            d.WriteTo(writer);

            return mem.ToArray();
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
            bin.Content = pat.Photo[0].Data;
            bin.ContentType = pat.Photo[0].ContentType;

            return bin;
        }

        public static Patient GetDemoPatient()
        {
            string xml = GetDemoPatientXml();
            var errors = new ErrorList();

            var pat = (Patient)FhirParser.ParseResourceFromXml(xml, errors);
            return pat;
        }
    }

}
