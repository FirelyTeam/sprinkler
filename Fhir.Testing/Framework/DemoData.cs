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
using Fhir.Testing.Properties;
using System.Collections.Generic;
using System.IO.Compression;
using System;

namespace Sprinkler.Framework
{
    public static class DemoData
    {
        public static string GetDemoConn5ExampleBundleXml()
        {
            return Resources.conn5_21_example;
        }

        public static Bundle GetDemoConn5ExampleBundle()
        {
            string xml = GetDemoConn5ExampleBundleXml();
            Bundle bundle = FhirParser.ParseBundleFromXml(xml);
            return bundle;
        }

        public static string GetDemoConn5CidExampleBundleXml()
        {
            return Resources.conn5_21_cid_example;
        }

        public static Bundle GetDemoConn5CidExampleBundle()
        {
            string xml = GetDemoConn5CidExampleBundleXml();
            Bundle bundle = FhirParser.ParseBundleFromXml(xml);
            return bundle;
        }

        public static string GetDemoXdsBundleXml()
        {
            return Resources.xds_example;
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
        //    var resourceName = "Fhir.Testing.cda-demo.xml";

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
            return Resources.conn5_21_example;
        }

        public static string GetDemoPatientXml()
        {
            return Resources.patient_example;
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
            var pat = (Patient)FhirParser.ParseResourceFromXml(xml);
            return pat;
        }



        public static List<Resource> GetListofResources()
        {
            const string ZIPFILEPATH = "example-resources.zip";
            List<Resource> resources = new List<Resource>();
           
            createEmptyDir("ResourceExamples");

            using (FileStream zipFileToOpen = new FileStream(ZIPFILEPATH, FileMode.Open))
            using (ZipArchive archive = new ZipArchive(zipFileToOpen, ZipArchiveMode.Read))
            {
                archive.ExtractToDirectory("ResourceExamples");
            }

            IEnumerable<string> files = Directory.EnumerateFiles("ResourceExamples");
            
            foreach (string s in files)
            {
                string xml = File.ReadAllText(s);
                Resource resource = FhirParser.ParseResourceFromXml(xml);
                resources.Add(resource);
            }
            Directory.Delete("ResourceExamples", true);
            return resources;            
        }

        private static void createEmptyDir(string baseTestPath)
        {
            if (Directory.Exists(baseTestPath)) Directory.Delete(baseTestPath, true);
            Directory.CreateDirectory(baseTestPath);
        }
    }
}