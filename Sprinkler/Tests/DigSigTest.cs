/* 
 * Copyright (c) 2014, Furore (info@furore.com) and contributors
 * See the file CONTRIBUTORS for details.
 * 
 * This file is licensed under the BSD 3-Clause license
 * available at https://raw.github.com/furore-fhir/sprinkler/master/LICENSE
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;
using System.Xml;
using Newtonsoft.Json;
using Hl7.Fhir.Rest;
using Hl7.Fhir.Support;
using Hl7.Fhir.Model;
using Sprinkler.Framework;
using Hl7.Fhir.Serialization;
using System.Security.Cryptography.X509Certificates;

namespace Sprinkler.Tests
{
    [SprinklerTestModule("DIGSIG")]
    public class DigSigTest : SprinklerTestClass
    {
        private string _signedXml;

        [SprinklerTest("DS01", "Posting a feed with a valid signature")]
        public void TestSigning()
        {
            Bundle b = new Bundle();

            b.Title = "Updates to resource 233";
            b.Id = new Uri("urn:uuid:0d0dcca9-23b9-4149-8619-65002224c3");
            b.LastUpdated = new DateTimeOffset(2012, 11, 2, 14, 17, 21, TimeSpan.Zero);
            b.AuthorName = "Ewout Kramer";

            ResourceEntry<Patient> p = new ResourceEntry<Patient>();
            p.Id = new ResourceIdentity("http://test.com/fhir/Patient/233");
            p.Resource = new Patient();
            p.Resource.Name = new List<HumanName> { HumanName.ForFamily("Kramer").WithGiven("Ewout") };
            b.Entries.Add(p);

            var certificate = getCertificate();

            var bundleData = FhirSerializer.SerializeBundleToXmlBytes(b);
            var bundleXml = Encoding.UTF8.GetString(bundleData);
            var bundleSigned = XmlSignatureHelper.Sign(bundleXml, certificate);
            _signedXml = bundleSigned;

            using (var response = postBundle(bundleSigned))
            {
                if (response.StatusCode != HttpStatusCode.OK) TestResult.Fail("Server refused POSTing signed document at /");
            }
        }

        private static X509Certificate2 getCertificate()
        {
            var myAssembly = typeof(CreateUpdateDeleteTest).Assembly;
            var stream = myAssembly.GetManifestResourceStream("Sprinkler.Tests.spark.pfx");
            var data = new byte[stream.Length];
            stream.Read(data, 0, (int)stream.Length);
            var certificate = new X509Certificate2(data);
            return certificate;
        }

        [SprinklerTest("DS02", "Posting a feed with an invalid signature")]
        public void TestSigningTampered()
        {
            var bundleSigned = _signedXml;

            int dv = bundleSigned.IndexOf("<DigestValue>");
            var changedBundle = bundleSigned.Replace("<name>Ewout", "<name>Ewald");

            using (var response = postBundle(changedBundle))
            {
                if (response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.Created) TestResult.Fail("Server accepted POSTing an invalid and signed document at /");
            }
        }

        private HttpWebResponse postBundle(string bundle)
        {
            var req = HttpWebRequest.Create(client.Endpoint);
            req.ContentType = "application/xml+fhir";
            req.Method = "POST";
            var outStream = req.GetRequestStream();
            var outStreamWriter = new StreamWriter(outStream, Encoding.UTF8);

            outStreamWriter.Write(bundle);
            outStreamWriter.Flush();
            outStreamWriter.Close();

            var response = (HttpWebResponse)req.GetResponseNoEx();
            return response;
        }
    }
}
