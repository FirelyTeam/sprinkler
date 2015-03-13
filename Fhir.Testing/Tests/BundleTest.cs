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
using Hl7.Fhir.Model;
using Hl7.Fhir.Rest;
using Sprinkler.Framework;

namespace Sprinkler.Tests
{
    [SprinklerModule("Bundle")]
    public class BundleTest : SprinklerTestClass
    {
        private Bundle.BundleEntryComponent _binDoc;
        private Bundle.BundleEntryComponent _connDoc, _patDoc;
        private Bundle _postResult;
        private Bundle.BundleEntryComponent _prac1Doc, _prac2Doc;

        [SprinklerTest("BU01", "post a bundle with xds docreference and binary")]
        public void PostBundle()
        {
            Bundle bundle = DemoData.GetDemoXdsBundle();
            _postResult = Client.Transaction(bundle);
            Assert.EntryIdsArePresentAndAbsoluteUrls(_postResult);

            if (_postResult.Entry.Count != 5)
                Assert.Fail(String.Format("Bundle response contained {0} entries in stead of 5",
                    _postResult.Entry.Count));

            _postResult = Client.RefreshBundle(_postResult);
            Assert.EntryIdsArePresentAndAbsoluteUrls(_postResult);
            List<Bundle.BundleEntryComponent> entries = _postResult.Entry;

            _connDoc = entries[0];

            if (new ResourceIdentity(_connDoc.Resource.Id).Id == null)
                Assert.Fail("failed to assign id to new xds document");
            if (new ResourceIdentity(_connDoc.Resource.Id).VersionId == null)
                Assert.Fail("failed to assign a version id to new xds document");

            _patDoc = entries[1];
            if (new ResourceIdentity(_patDoc.Resource.Id) == null) Assert.Fail("failed to assign id to new patient");

            _prac1Doc = entries[2];
            if (new ResourceIdentity(_prac1Doc.Resource.Id).Id == null)
                Assert.Fail("failed to assign id to new practitioner (#1)");
            if (new ResourceIdentity(_prac1Doc.Resource.Id).VersionId == null)
                Assert.Fail("failed to assign a version id to new practitioner (#1)");

            _prac2Doc = entries[3];
            if (new ResourceIdentity(_prac2Doc.Resource.Id).Id == null)
                Assert.Fail("failed to assign id to new practitioner (#2)");
            if (new ResourceIdentity(_prac2Doc.Resource.Id).VersionId == null)
                Assert.Fail("failed to assign a version id to new practitioner (#2)");

            _binDoc = entries[4];
            if (new ResourceIdentity(_binDoc.Resource.Id).Id == null) Assert.Fail("failed to assign id to new binary");
            if (new ResourceIdentity(_binDoc.Resource.Id).VersionId == null)
                Assert.Fail("failed to assign a version id to new binary");

            DocumentReference docResource = (DocumentReference) _connDoc.Resource;

            if (!_prac1Doc.Resource.Id.Contains(docResource.Author[0].Reference))
                Assert.Fail("doc reference's author[0] does not reference newly created practitioner #1");
            if (!_prac2Doc.Resource.Id.Contains(docResource.Author[1].Reference))
                Assert.Fail("doc reference's author[1] does not reference newly created practitioner #2");

            var binRl = new ResourceIdentity(_binDoc.Resource.Id);

            if (!docResource.Text.Div.Contains(binRl.MakeRelative().ToString()))
                
                Assert.Fail("href in narrative was not fixed to point to newly created binary");
        }

        [SprinklerTest("BU02", "fetch created resources")]
        public void FetchCreatedResources()
        {
            Assert.SkipWhen(_connDoc == null);

            Client.Read<DocumentReference>(_connDoc.Resource.Id);
            Client.Read<Patient>(_patDoc.Resource.Id);
            Client.Read<Practitioner>(_prac1Doc.Resource.Id);
            Client.Read<Practitioner>(_prac2Doc.Resource.Id);
            Client.Read<Binary>(_binDoc.Resource.Id);
        }


        [SprinklerTest("BU03", "post a bundle with identical cids")]
        public void PostBundleAgain()
        {
            Bundle bundle = DemoData.GetDemoXdsBundle();
            Bundle trans = Client.Transaction(bundle);
            List<Bundle.BundleEntryComponent> entries = trans.Entry;
            Assert.EntryIdsArePresentAndAbsoluteUrls(trans);

            // If server honors the 'search' link, it might *not* re-create the patient
            if (entries[0].Resource.Id == _connDoc.Resource.Id || entries[4].Resource.Id == _binDoc.Resource.Id) // etcetera
                Assert.Fail("submitting a bundle with identical cids should still create new resources");

            //TODO: verify server honors the 'search' link
        }

        [SprinklerTest("BU04", "post a bundle with updates")]
        public void PostBundleWithUpdates()
        {
            if (_postResult == null) Assert.Skip();

            string newMasterId = "urn:oid:123.456.7.8.9";
            DocumentReference doc = _postResult.Entry.ByResourceType<DocumentReference>().First();
            //    _postResult.Entries.OfType<ResourceEntry<DocumentReference>>().First();
            doc.MasterIdentifier.Value = newMasterId;

            Patient pat = _postResult.Entry.ByResourceType<Patient>().First();
            pat.Identifier[0].Value = "3141592";

            List<Bundle.BundleEntryComponent> entries = _postResult.Entry;
            Bundle returnedBundle = Client.Transaction(_postResult);
            Assert.EntryIdsArePresentAndAbsoluteUrls(returnedBundle);

            List<Bundle.BundleEntryComponent> entries2 = returnedBundle.Entry;
            if (entries2[0].Resource.Id != entries[0].Resource.Id || entries2[4].Resource.Id != entries[4].Resource.Id) // etcetera
                Assert.Fail("submitting a batch with updates created new resources");

            Bundle refreshedBundle = Client.RefreshBundle(returnedBundle);
            Assert.EntryIdsArePresentAndAbsoluteUrls(refreshedBundle);

            if (
                refreshedBundle.Entry.ByResourceType<DocumentReference>()
                    .First()
                    .MasterIdentifier.Value != newMasterId)
                Assert.Fail("update on document resource was not reflected in batch result");

            if (refreshedBundle.Entry.ByResourceType<Patient>().First().Identifier[0].Value !=
                "3141592")
                Assert.Fail("update on patient was not reflected in batch result");

            doc = Client.Read<DocumentReference>(doc.Id);
            if (doc.MasterIdentifier.Value != newMasterId)
                Assert.Fail("update on document resource was not reflected in new version");

            pat = Client.Read<Patient>(pat.Id);
            if (pat.Identifier[0].Value != "3141592")
                Assert.Fail("update on patient was not reflected in new version");
        }
    }

    public static class BundleExtensions
    {
        
    }
}