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
using Hl7.Fhir.Rest;
using Hl7.Fhir.Model;
using Hl7.Fhir.Support;
using Sprinkler.Framework;

namespace Sprinkler.Tests
{
    [SprinklerTestModule("Bundle")]
    public class BundleTest : SprinklerTestClass
    {

        BundleEntry connDoc, patDoc, prac1Doc, prac2Doc, binDoc;
        Bundle postResult;

        [SprinklerTest("BU01", "post a bundle with xds docreference and binary")]
        public void PostBundle()
        {
            var bundle = DemoData.GetDemoXdsBundle();

            postResult = client.Transaction(bundle);
            HttpTests.AssertEntryIdsArePresentAndAbsoluteUrls(postResult);
            if (postResult.Entries.Count != 5)
                TestResult.Fail(String.Format("Bundle response contained {0} entries in stead of 5", postResult.Entries.Count));

            postResult = client.RefreshBundle(postResult);
            HttpTests.AssertEntryIdsArePresentAndAbsoluteUrls(postResult);
            var entries = postResult.Entries.ToList();
            
            connDoc = entries[0];
            
            if (new ResourceIdentity(connDoc.Id).Id==null) TestResult.Fail("failed to assign id to new xds document");
            if (new ResourceIdentity(connDoc.SelfLink).VersionId == null) TestResult.Fail("failed to assign a version id to new xds document");

            patDoc = entries[1];
            if (new ResourceIdentity(patDoc.Id) == null) TestResult.Fail("failed to assign id to new patient");

            prac1Doc = entries[2];
            if (new ResourceIdentity(prac1Doc.Id).Id == null) TestResult.Fail("failed to assign id to new practitioner (#1)");
            if (new ResourceIdentity(prac1Doc.SelfLink).VersionId == null) TestResult.Fail("failed to assign a version id to new practitioner (#1)");

            prac2Doc = entries[3];
            if (new ResourceIdentity(prac2Doc.Id).Id == null) TestResult.Fail("failed to assign id to new practitioner (#2)");
            if (new ResourceIdentity(prac2Doc.SelfLink).VersionId == null) TestResult.Fail("failed to assign a version id to new practitioner (#2)");

            binDoc = entries[4];
            if (new ResourceIdentity(binDoc.Id).Id == null) TestResult.Fail("failed to assign id to new binary");
            if (new ResourceIdentity(binDoc.SelfLink).VersionId == null)
                TestResult.Fail("failed to assign a version id to new binary");

            var docResource = ((ResourceEntry<DocumentReference>)connDoc).Resource;

            if (!prac1Doc.Id.ToString().Contains(docResource.Author[0].Reference))
                TestResult.Fail("doc reference's author[0] does not reference newly created practitioner #1");
            if (!prac2Doc.Id.ToString().Contains(docResource.Author[1].Reference))
                TestResult.Fail("doc reference's author[1] does not reference newly created practitioner #2");

            var binRl = new ResourceIdentity(binDoc.Id);

            if (!docResource.Text.Div.Contains(binRl.OperationPath.ToString()))
                TestResult.Fail("href in narrative was not fixed to point to newly created binary");
        }

        [SprinklerTest("BU02", "fetch created resources")]
        public void FetchCreatedResources()
        {
            client.Read<DocumentReference>(connDoc.Id);
            client.Read<Patient>(patDoc.Id);
            client.Read<Practitioner>(prac1Doc.Id);
            client.Read<Practitioner>(prac2Doc.Id);
            client.Read<Binary>(binDoc.Id);
        }


        [SprinklerTest("BU03", "post a bundle with identical cids")]
        public void PostBundleAgain()
        {
            var bundle = DemoData.GetDemoXdsBundle();
            var trans = client.Transaction(bundle);
            var entries = trans.Entries.ToList();
            HttpTests.AssertEntryIdsArePresentAndAbsoluteUrls(trans);

            // If server honors the 'search' link, it might *not* re-create the patient
            if (entries[0].Id == connDoc.Id || entries[4].Id == binDoc.Id)  // etcetera
                TestResult.Fail("submitting a bundle with identical cids should still create new resources");

            //TODO: verify server honors the 'search' link
        }

        [SprinklerTest("BU04", "post a bundle with updates")]
        public void PostBundleWithUpdates()
        {
            var newMasterId = "urn:oid:123.456.7.8.9";
            var doc = postResult.Entries.OfType<ResourceEntry<DocumentReference>>().First();
            doc.Resource.MasterIdentifier.Value = newMasterId;

            var pat = postResult.Entries.OfType<ResourceEntry<Patient>>().First();
            pat.Resource.Identifier[0].Value = "3141592";

            var entries = postResult.Entries.ToList();            
            var returnedBundle = client.Transaction(postResult);
            HttpTests.AssertEntryIdsArePresentAndAbsoluteUrls(returnedBundle);

            var entries2 = returnedBundle.Entries.ToList();
            if (entries2[0].Id != entries[0].Id || entries2[4].Id != entries[4].Id)  // etcetera
                TestResult.Fail("submitting a batch with updates created new resources");

            var refreshedBundle = client.RefreshBundle(returnedBundle);
            HttpTests.AssertEntryIdsArePresentAndAbsoluteUrls(refreshedBundle);

            if (refreshedBundle.Entries.OfType<ResourceEntry<DocumentReference>>().First().Resource.MasterIdentifier.Value != newMasterId)
                TestResult.Fail("update on document resource was not reflected in batch result");
            
            if(refreshedBundle.Entries.OfType<ResourceEntry<Patient>>().First().Resource.Identifier[0].Value != "3141592")
                TestResult.Fail("update on patient was not reflected in batch result");

            doc = client.Read<DocumentReference>(doc.Id);
            if (doc.Resource.MasterIdentifier.Value != newMasterId)
                TestResult.Fail("update on document resource was not reflected in new version");

            pat = client.Read<Patient>(pat.Id);
            if (pat.Resource.Identifier[0].Value != "3141592")
                TestResult.Fail("update on patient was not reflected in new version");
        }

    }
}
