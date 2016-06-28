/* 
 * Copyright (c) 2014, Furore (info@furore.com) and contributors
 * See the file CONTRIBUTORS for details.
 * 
 * This file is licensed under the BSD 3-Clause license
 * available at https://raw.github.com/furore-fhir/sprinkler/master/LICENSE
 */

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.IO.Compression;
using Hl7.Fhir.Model;
using Hl7.Fhir.Serialization;
using System.Reflection;

namespace Furore.Fhir.Sprinkler.TestSet
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
            return (Bundle)FhirParser.ParseFromXml(xml);
        }

        public static string GetDemoConn5CidExampleBundleXml()
        {
            return Resources.conn5_21_cid_example;
        }

        public static Bundle GetDemoConn5CidExampleBundle()
        {
            string xml = GetDemoConn5CidExampleBundleXml();
            return (Bundle)FhirParser.ParseFromXml(xml);
        }

        public static string GetDemoXdsBundleXml()
        {
            return Resources.xds_example;
        }

        public static Bundle GetDemoBundle()
        {
            string xml = GetDemoXdsBundleXml();
            return (Bundle)FhirParser.ParseFromXml(xml);
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
            return (Bundle)FhirParser.ParseFromXml(xml);
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

        public static List<DomainResource> GetListofResources()
        {
            const string ZIPFILENAME = "examples.zip";
            const string resourcesDir = "Resources";




            string file = AssemblyResources.GetResourcePath(ZIPFILENAME);

            List<DomainResource> resources = new List<DomainResource>();

            using (FileStream zipFileToOpen = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.Read))
                
            using (ZipArchive archive = new ZipArchive(zipFileToOpen, ZipArchiveMode.Read))
            {
                ReadOnlyCollection<ZipArchiveEntry> entries = archive.Entries;
                foreach (ZipArchiveEntry e in entries)
                {
                    StreamReader reader = new StreamReader(e.Open());   
                    Resource resource = FhirParser.ParseResourceFromXml(reader.ReadToEnd());
                    if (resource is DomainResource)
                    //if (resource.ResourceType == ResourceType.DomainResource)
                    {
                         DomainResource domresource = (DomainResource)resource;
                         resources.Add(domresource); 
                    }                                       
                }               
            }
            return resources;        
        }
  
    }

    public static class AssemblyResources
    {
        public static string GetResourcePath(string filename)
        {
            // DOES NOT WORK in dnx:
            // string file = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, resourcesDir, ZIPFILEPATH);

            // DOES NOT WORK EITHER (in dnx):
            //Assembly.GetExecutingAssembly().Location;

            string location = typeof(AssemblyResources).Assembly.Location;
                
            string path = Path.GetDirectoryName(location);
            string file = Path.Combine(path, "Resources", filename);
            return file;
        }
    }

}