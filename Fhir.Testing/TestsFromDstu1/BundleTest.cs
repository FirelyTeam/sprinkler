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
        private BundleEntry _binDoc;
        private BundleEntry _connDoc, _patDoc;
        private Bundle _postResult;
        private BundleEntry _prac1Doc, _prac2Doc;

        [SprinklerTest("BU01", "post a bundle with xds docreference and binary")]
        public void PostBundle()
        {
            Bundle bundle = DemoData.GetDemoXdsBundle();

            _postResult = Client.Transaction(bundle);
            Assert.EntryIdsArePresentAndAbsoluteUrls(_postResult);
            if (_postResult.Entries.Count != 5)
                Assert.Fail(String.Format("Bundle response contained {0} entries in stead of 5",
                    _postResult.Entries.Count));

            _postResult = Client.RefreshBundle(_postResult);
            Assert.EntryIdsArePresentAndAbsoluteUrls(_postResult);
            List<BundleEntry> entries = _postResult.Entries.ToList();

            _connDoc = entries[0];

            if (new ResourceIdentity(_connDoc.Id).Id == null)
                Assert.Fail("failed to assign id to new xds document");
            if (new ResourceIdentity(_connDoc.SelfLink).VersionId == null)
                Assert.Fail("failed to assign a version id to new xds document");

            _patDoc = entries[1];
            if (new ResourceIdentity(_patDoc.Id) == null) Assert.Fail("failed to assign id to new patient");

            _prac1Doc = entries[2];
            if (new ResourceIdentity(_prac1Doc.Id).Id == null)
                Assert.Fail("failed to assign id to new practitioner (#1)");
            if (new ResourceIdentity(_prac1Doc.SelfLink).VersionId == null)
                Assert.Fail("failed to assign a version id to new practitioner (#1)");

            _prac2Doc = entries[3];
            if (new ResourceIdentity(_prac2Doc.Id).Id == null)
                Assert.Fail("failed to assign id to new practitioner (#2)");
            if (new ResourceIdentity(_prac2Doc.SelfLink).VersionId == null)
                Assert.Fail("failed to assign a version id to new practitioner (#2)");

            _binDoc = entries[4];
            if (new ResourceIdentity(_binDoc.Id).Id == null) Assert.Fail("failed to assign id to new binary");
            if (new ResourceIdentity(_binDoc.SelfLink).VersionId == null)
                Assert.Fail("failed to assign a version id to new binary");

            DocumentReference docResource = ((ResourceEntry<DocumentReference>) _connDoc).Resource;

            if (!_prac1Doc.Id.ToString().Contains(docResource.Author[0].Reference))
                Assert.Fail("doc reference's author[0] does not reference newly created practitioner #1");
            if (!_prac2Doc.Id.ToString().Contains(docResource.Author[1].Reference))
                Assert.Fail("doc reference's author[1] does not reference newly created practitioner #2");

            var binRl = new ResourceIdentity(_binDoc.Id);

            if (!docResource.Text.Div.Contains(binRl.OperationPath.ToString()))
                Assert.Fail("href in narrative was not fixed to point to newly created binary");
        }

        [SprinklerTest("BU02", "fetch created resources")]
        public void FetchCreatedResources()
        {
            Assert.SkipWhen(_connDoc == null);

            Client.Read<DocumentReference>(_connDoc.Id);
            Client.Read<Patient>(_patDoc.Id);
            Client.Read<Practitioner>(_prac1Doc.Id);
            Client.Read<Practitioner>(_prac2Doc.Id);
            Client.Read<Binary>(_binDoc.Id);
        }


        [SprinklerTest("BU03", "post a bundle with identical cids")]
        public void PostBundleAgain()
        {
            Bundle bundle = DemoData.GetDemoXdsBundle();
            Bundle trans = Client.Transaction(bundle);
            List<BundleEntry> entries = trans.Entries.ToList();
            Assert.EntryIdsArePresentAndAbsoluteUrls(trans);

            // If server honors the 'search' link, it might *not* re-create the patient
            if (entries[0].Id == _connDoc.Id || entries[4].Id == _binDoc.Id) // etcetera
                Assert.Fail("submitting a bundle with identical cids should still create new resources");

            //TODO: verify server honors the 'search' link
        }

        [SprinklerTest("BU04", "post a bundle with updates")]
        public void PostBundleWithUpdates()
        {
            if (_postResult == null) Assert.Skip();

            string newMasterId = "urn:oid:123.456.7.8.9";
            ResourceEntry<DocumentReference> doc =
                _postResult.Entries.OfType<ResourceEntry<DocumentReference>>().First();
            doc.Resource.MasterIdentifier.Value = newMasterId;

            ResourceEntry<Patient> pat = _postResult.Entries.OfType<ResourceEntry<Patient>>().First();
            pat.Resource.Identifier[0].Value = "3141592";

            List<BundleEntry> entries = _postResult.Entries.ToList();
            Bundle returnedBundle = Client.Transaction(_postResult);
            Assert.EntryIdsArePresentAndAbsoluteUrls(returnedBundle);

            List<BundleEntry> entries2 = returnedBundle.Entries.ToList();
            if (entries2[0].Id != entries[0].Id || entries2[4].Id != entries[4].Id) // etcetera
                Assert.Fail("submitting a batch with updates created new resources");

            Bundle refreshedBundle = Client.RefreshBundle(returnedBundle);
            Assert.EntryIdsArePresentAndAbsoluteUrls(refreshedBundle);

            if (
                refreshedBundle.Entries.OfType<ResourceEntry<DocumentReference>>()
                    .First()
                    .Resource.MasterIdentifier.Value != newMasterId)
                Assert.Fail("update on document resource was not reflected in batch result");

            if (refreshedBundle.Entries.OfType<ResourceEntry<Patient>>().First().Resource.Identifier[0].Value !=
                "3141592")
                Assert.Fail("update on patient was not reflected in batch result");

            doc = Client.Read<DocumentReference>(doc.Id);
            if (doc.Resource.MasterIdentifier.Value != newMasterId)
                Assert.Fail("update on document resource was not reflected in new version");

            pat = Client.Read<Patient>(pat.Id);
            if (pat.Resource.Identifier[0].Value != "3141592")
                Assert.Fail("update on patient was not reflected in new version");
        }
    }
}