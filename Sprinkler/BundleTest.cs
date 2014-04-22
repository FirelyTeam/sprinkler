using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;
using Hl7.Fhir.Client;
using Hl7.Fhir.Model;
using Hl7.Fhir.Support;

namespace Sprinkler
{
    [SprinklerTestModule("Bundle")]
    public class BundleTest
    {
        FhirClient client;

        public BundleTest(Uri fhirUri)
        {
            client = new FhirClient(fhirUri);
        }

        BundleEntry xdsDoc, patDoc, prac1Doc, prac2Doc, binDoc;
        Bundle postResult;

        [SprinklerTest("post a bundle with xds docreference and binary")]
        public void PostBundle()
        {
            var bundle = DemoData.GetDemoXdsBundle();

            postResult = client.Batch(bundle);

            if (postResult.Entries.Count != 5)
                TestResult.Fail(String.Format("Bundle response contained {0} entries in stead of 5", postResult.Entries.Count));

            xdsDoc = postResult.Entries[0];
            if (ResourceLocation.GetIdFromResourceId(xdsDoc.Id)==null) TestResult.Fail("failed to assign id to new xds document");
            if (ResourceLocation.GetVersionFromResourceId(xdsDoc.SelfLink) == null) TestResult.Fail("failed to assign a version id to new xds document");

            patDoc = postResult.Entries[1];
            if (ResourceLocation.GetIdFromResourceId(patDoc.Id) == null) TestResult.Fail("failed to assign id to new patient");

            prac1Doc = postResult.Entries[2];
            if (ResourceLocation.GetIdFromResourceId(prac1Doc.Id) == null) TestResult.Fail("failed to assign id to new practitioner (#1)");
            if (ResourceLocation.GetVersionFromResourceId(prac1Doc.SelfLink) == null) TestResult.Fail("failed to assign a version id to new practitioner (#1)");

            prac2Doc = postResult.Entries[3];
            if (ResourceLocation.GetIdFromResourceId(prac2Doc.Id) == null) TestResult.Fail("failed to assign id to new practitioner (#2)");
            if (ResourceLocation.GetVersionFromResourceId(prac2Doc.SelfLink) == null) TestResult.Fail("failed to assign a version id to new practitioner (#2)");

            binDoc = postResult.Entries[4];
            if (ResourceLocation.GetIdFromResourceId(binDoc.Id) == null) TestResult.Fail("failed to assign id to new binary");
            if (ResourceLocation.GetVersionFromResourceId(binDoc.SelfLink) == null) TestResult.Fail("failed to assign a version id to new binary");

            var docResource = ((ResourceEntry<DocumentReference>)xdsDoc).Resource;

            if (!prac1Doc.Id.ToString().Contains(docResource.Author[0].Reference))
                TestResult.Fail("doc reference's author[0] does not reference newly created practitioner #1");
            if (!prac2Doc.Id.ToString().Contains(docResource.Author[1].Reference))
                TestResult.Fail("doc reference's author[1] does not reference newly created practitioner #2");

            var binRl = new ResourceLocation(binDoc.SelfLink);
            if (!docResource.Text.Div.Contains(binRl.OperationPath.ToString()))
                TestResult.Fail("href in narrative was not fixed to point to newly created binary");
        }

        [SprinklerTest("fetch created resources")]
        public void FetchCreatedResources()
        {
            client.Fetch<DocumentReference>(xdsDoc.Id);
            client.Fetch<Patient>(patDoc.Id);
            client.Fetch<Practitioner>(prac1Doc.Id);
            client.Fetch<Practitioner>(prac2Doc.Id);
            client.Fetch<Binary>(binDoc.Id);
        }


        [SprinklerTest("post a bundle with identical cids")]
        public void PostBundleAgain()
        {
            var bundle = DemoData.GetDemoXdsBundle();

            postResult = client.Batch(bundle);

            // If server honors the 'search' link, it might *not* re-create the patient
            if (postResult.Entries[0].Id == xdsDoc.Id || postResult.Entries[4].Id == binDoc.Id)  // etcetera
                TestResult.Fail("submitting a bundle with identical cids should still create new resources");

            //TODO: verify server honor the 'search' link
        }

        [SprinklerTest("post a bundle with updates")]
        public void PostBundleWithUpdates()
        {
            var newMasterId = "urn:oid:123.456.7.8.9";
            var doc = postResult.Entries.ByResourceType<DocumentReference>().First();
            doc.Resource.MasterIdentifier.Key = newMasterId;

            var pat = postResult.Entries.ByResourceType<Patient>().First();
            pat.Resource.Identifier[0].Key = "3141592";

            var postResult2 = client.Batch(postResult);

            if (postResult2.Entries[0].Id != postResult.Entries[0].Id || postResult2.Entries[4].Id != postResult.Entries[4].Id)  // etcetera
                TestResult.Fail("submitting a batch with updates created new resources");

            if (postResult2.Entries.ByResourceType<DocumentReference>().First().Resource.MasterIdentifier.Key != newMasterId)
                TestResult.Fail("update on document resource was not reflected in batch result");
            
            if(postResult2.Entries.ByResourceType<Patient>().First().Resource.Identifier[0].Key != "3141592")
                TestResult.Fail("update on patient was not reflected in batch result");

            doc = client.Fetch<DocumentReference>(doc.Id);
            if (doc.Resource.MasterIdentifier.Key != newMasterId)
                TestResult.Fail("update on document resource was not reflected in new version");

            pat = client.Fetch<Patient>(pat.Id);
            if (pat.Resource.Identifier[0].Key != "3141592")
                TestResult.Fail("update on patient was not reflected in new version");
        }


        
    }
}
