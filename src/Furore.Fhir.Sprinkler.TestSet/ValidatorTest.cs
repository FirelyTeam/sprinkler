/* 
 * Copyright (c) 2014, Furore (info@furore.com) and contributors
 * See the file CONTRIBUTORS for details.
 * 
 * This file is licensed under the BSD 3-Clause license
 * available at https://raw.github.com/furore-fhir/sprinkler/master/LICENSE
 */

using System;
using System.IO;
using System.Net;
using System.Text;
using Furore.Fhir.Sprinkler.Framework.Framework;
using Hl7.Fhir.Rest;

namespace Furore.Fhir.Sprinkler.TestSet
{
    [SprinklerModule("Validation")]
    public class ValidatorTest : SprinklerTestClass
    {
        [SprinklerTest("VA01", "Validate creation of a valid resource")]
        public void CreateValidResource()
        {
            Uri location = new Uri(Client.Endpoint + "Patient/$validate");
            string resource = Resources.Patient_Valid;
            
            using (HttpWebResponse response = PostResource(location, resource))
            {
                if (response.StatusCode != HttpStatusCode.Created)
                    Assert.Fail("Server did not accept valid resource");
            }
        }

        [SprinklerTest("VA02", "Validate creation of an invalid resource (wrong name use)")]
        public void CreateResourceValueError()
        {
            Uri location = new Uri(Client.Endpoint + "Patient/$validate");
            string xml = Resources.Patient_ErrorUse;

            using (HttpWebResponse response = PostResource(location, xml))
            {
                if (response.StatusCode == HttpStatusCode.Created)
                    Assert.Fail("Server accepted invalid resource with 'unofficial' as a value for Patient.name.use");
            }
        }

        [SprinklerTest("VA03", "Validate creation of an invalid resource (cardinality minus)")]
        public void CreateResourceCardinalityMinus()
        {
            Uri location = new Uri(Client.Endpoint + "Patient/$validate");
            string xml = Resources.Patient_CardinalityMinus;

            using (HttpWebResponse response = PostResource(location, xml))
            {
                if (response.StatusCode == HttpStatusCode.Created)
                    Assert.Fail("Server accepted invalid resource with text.status cardinality of 0, should be 1.");
            }
        }

        [SprinklerTest("VA04", "Validate creation of an invalid resource (cardinality plus)")]
        public void CreateResourceCardinalityPlus()
        {
            Uri location = new Uri(Client.Endpoint + "Patient/$validate");
            string xml = Resources.Patient_CardinalityPlus;

            using (HttpWebResponse response = PostResource(location, xml))
            {
                if (response.StatusCode == HttpStatusCode.Created)
                    Assert.Fail("Server accepted invalid resource with name.use cardinality of 2, should be 1.");
            }
        }

        [SprinklerTest("VA05", "Validate creation of an invalid resource (constraint error)")]
        public void CreateResourceConstraintError()
        {
            Uri location = new Uri(Client.Endpoint + "Patient/$validate");
            string xml = Resources.Patient_ConstraintError;
            using (HttpWebResponse response = PostResource(location, xml))
            {
                if (response.StatusCode == HttpStatusCode.Created)
                    Assert.Fail("Server accepted invalid resource with a constraint error");
            }
        }

        [SprinklerTest("VA06", "Validate creation of an invalid resource (invalid element)")]
        public void CreateResourceInvalidElement()
        {
            Uri location = new Uri(Client.Endpoint + "Patient/$validate");
            string xml = Resources.Patient_InvalidElement;
            
            using (HttpWebResponse response = PostResource(location, xml))
            {
                if (response.StatusCode == HttpStatusCode.Created)
                    Assert.Fail("Server accepted invalid resource with an invalid element.");
            }
        }

        [SprinklerTest("VA07", "Validate creation of an invalid resource (wrong narrative)")]
        public void CreateResourceWrongNarrative()
        {
            Uri location = new Uri(Client.Endpoint + "Patient/$validate");
            string xml = Resources.Patient_InvalidElement;
            using (HttpWebResponse response = PostResource(location, xml))
            {
                if (response.StatusCode == HttpStatusCode.Created)
                    Assert.Fail("Server accepted invalid resource with wrong namespace on narrative.");
            }
        }


        //[SprinklerTest("VA03", "Validate a resource against a custom profile")]
        //public void ValidateResourceAgainstACustomProfile()
        //{
           
        //}

        private HttpWebResponse PostResource(Uri location, string content)
        {
            WebRequest req = WebRequest.Create(location);
            req.ContentType = "application/xml+fhir";
            req.Method = "POST";
            Stream outStream = req.GetRequestStream();
            var outStreamWriter = new StreamWriter(outStream, Encoding.UTF8);

            outStreamWriter.Write(content);
            outStreamWriter.Flush();
            outStreamWriter.Close();

            var response = (HttpWebResponse)req.GetResponseNoEx();
            return response;
        }



        //[SprinklerTest("V003", "Validate a valid resource update")]
        //public void ValidateUpdateResource()
        //{
        //    Patient patient = DemoData.GetDemoPatient();
        //    ResourceEntry<Patient> result = Client.Create(patient);

        //    OperationOutcome oo;
        //    if (!Client.TryValidateUpdate(result, out oo))
        //        Assert.Fail("Validation incorrectly reported failure.");
        //}

        //[SprinklerTest("V004", "Validate an invalid resource update")]
        //public void ValidateInvalidUpdateResource()
        //{
        //    Patient patient = DemoData.GetDemoPatient();
        //    ResourceEntry<Patient> result = Client.Create(patient);
        //    patient.Identifier = new List<Identifier> {new Identifier {System = "urn:oid:hallo" }};

        //    OperationOutcome oo;
        //    if (!Client.TryValidateUpdate(result, out oo))
        //        Assert.Fail("Validation incorrectly reported failure.");
        //}


    }
}