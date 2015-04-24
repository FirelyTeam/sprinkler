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
using System.Net;
using Hl7.Fhir.Model;
using Hl7.Fhir.Rest;
using Sprinkler.Framework;

namespace Sprinkler.Tests
{
    [SprinklerModule("History")]
    public class HistoryTest : SprinklerTestClass
    {
        //private CreateUpdateDeleteTest crudTests;
        private IList<string> versions = new List<string>();
        private DateTimeOffset? _createDate;
        private int _forwardCount = -1;
        private Bundle history;

        private string _id;
        private Bundle _lastPage;
        private string _location;
        private Bundle _systemHistory;
        private Bundle referenceBundle;

        private void Initialize()
        {
            
            Patient patient = DemoData.GetDemoPatient();
            patient = Client.Create(patient);

            _id = patient.Id;
            _location = "Patient/" + _id;
            _createDate = DateTimeOffset.Now;
            
            versions.Add(patient.VersionId);

            patient.Telecom.Add(new ContactPoint {System = ContactPoint.ContactPointSystem.Email, Value = "info@furore.com"});

            patient = Client.Update(patient, true);
            versions.Add(patient.VersionId);

            patient = Client.Update(patient, true);
            versions.Add(patient.VersionId);

            Client.Delete(_location);
        }


        [SprinklerTest("HI01", "Request the full history for a specific resource")]
        public void HistoryForSpecificResource()
        {
            Initialize();
            Assert.SkipWhen(_createDate == null);

            history = Client.History(_location);
            Assert.EntryIdsArePresentAndAbsoluteUrls(history);

            // There's one version less here, because we don't have the deletion
            int expected = versions.Count + 1;

            if (history.Entry.Count != expected)
            {
                Assert.Fail("{0} versions expected after crud test, found {1}", expected, history.Entry.Count);
            }

            CheckTransactionElements(history);
            CheckVersionIds(history);
            CheckSortOrder(history);
        }

        [SprinklerTest("HI02", "Request the full history for a resource with _since")]
        public void HistoryForSpecificResourceId()
        {
            Assert.SkipWhen(_createDate == null) ;

            DateTimeOffset before = _createDate.Value.AddMinutes(-1);
            DateTimeOffset after = before.AddHours(1);

            Bundle history = Client.History(_location, before);
            Assert.EntryIdsArePresentAndAbsoluteUrls(history);
            
            CheckTransactionElements(history);
            CheckVersionIds(history);
            CheckSortOrder(history);

            var versionIds = history.VersionIds();

            if (!versions.AreAllFoundIn(versionIds))
            {
                Assert.Fail("history with _since does not contain all versions");
            }

            history = Client.History(_location, after);
            if (history.Entry.Count != 0)
                Assert.Fail("Setting since to after the last update still returns history");
        }


        [SprinklerTest("HI03", "Request individual history versions from a resource")]
        public void VReadVersions()
        {
            foreach (string version in versions)
            {
                var location = ResourceIdentity.Build("Patient", this._id, version);
                Patient patient = Client.Read<Patient>(location);

                if (patient == null)
                {
                    Assert.Fail("Cannot find version that was present in history");
                }

                Assert.ContentLocationPresentAndValid(this.Client);

                var identity = patient.ResourceIdentity();
                //var selfLink = new ResourceIdentity(Client.LastResponseDetails.ContentLocation);
                //if (String.IsNullOrEmpty(selfLink.Id) || String.IsNullOrEmpty(selfLink.VersionId))
                //    Assert.Fail("Optional Content-Location contains an invalid version-specific url");

                if (String.IsNullOrEmpty(identity.Id) || String.IsNullOrEmpty(identity.VersionId))
                    Assert.Fail("Optional Content-Location contains an invalid version-specific url");
            }

            foreach (var ent in (Utils.GetDeleted(history.Entry)))
            {
                var identity = new ResourceIdentity(ent.Transaction.Url);
                Assert.Fails(Client, () => Client.Read<Patient>(identity), HttpStatusCode.Gone);
            }
        }

        [SprinklerTest("HI04", "Fetching history of non-existing resource returns exception")]
        public void HistoryForNonExistingResource()
        {
            if (_createDate == null) Assert.Skip();

            Assert.Fails(Client, () => Client.History("Patient/3141592unlikely"), HttpStatusCode.NotFound);
        }

        [SprinklerTest("HI06", "Get all history for a resource type with _since")]
        public void HistoryForResourceType()
        {
            if (_createDate == null) Assert.Skip();

            DateTimeOffset before = _createDate.Value.AddMinutes(-1);
            DateTimeOffset after = before.AddHours(1);

            Bundle history = Client.TypeHistory("Patient", before);
            
            Assert.EntryIdsArePresentAndAbsoluteUrls(history);
            referenceBundle = history;

            CheckSortOrder(history);

            var _versions = history.VersionIds();
            if (!versions.AreAllFoundIn(_versions))
            {
                Assert.Fail("history with _since does not contain all versions of instance");
            }

            history = Client.History(_location, DateTimeOffset.Now.AddMinutes(1));

            if (history.Entry.Count != 0)
                Assert.Fail("Setting since to a future moment still returns history");
        }


        [SprinklerTest("HI08", "Get the history for the whole system with _since")]
        public void HistoryForWholeSystem()
        {
            if (_createDate == null) Assert.Skip();
            DateTimeOffset before = _createDate.Value.AddMinutes(-1);
            Bundle history;


            history = Client.WholeSystemHistory(before);
            Assert.EntryIdsArePresentAndAbsoluteUrls(history);

            // Assert.HasAllForwardNavigationLinks(history); // Assumption: system has more history than pagesize
            if (history.FirstLink == null || history.LastLink == null)
            {
                Assert.Fail("Expecting first, last link to be present");
            }

            _systemHistory = history;
            CheckSortOrder(history);

            var historyLinks = history.VersionIds();

            if (!versions.AreAllFoundIn(historyLinks))
            { 
                Assert.Fail("history with _since does not contain all versions of instance");
            }

            history = Client.History(_location, DateTimeOffset.Now.AddMinutes(1));

            if (history.Entry.Count != 0)
                Assert.Fail("Setting since to a future moment still returns history");
        }

        [SprinklerTest("HI09", "Paging forward through a resource type history")]
        public void PageFwdThroughResourceHistory()
        {
            int pageSize = 30;
            Bundle page = Client.TypeHistory("Patient", since: DateTimeOffset.Now.AddHours(-1), pageSize: pageSize);
            Assert.EntryIdsArePresentAndAbsoluteUrls(page);

            _forwardCount = 0;

            // Browse forwards
            while (page != null)
            {
                if (page.Entry.Count > pageSize)
                    Assert.Fail("Server returned a page with more entries than set by _count");

                _forwardCount += page.Entry.Count;
                _lastPage = page;
                Assert.EntryIdsArePresentAndAbsoluteUrls(page);
                page = Client.Continue(page);
            }

            //if (total.HasValue && forwardCount < total)
            //    TestResult.Fail(String.Format("Paging did not return all entries(expected at least {0}, {1} returned)", 
            //                    total, forwardCount));
        }

        [SprinklerTest("HI10", "Page backwards through a resource type history")]
        public void PageBackThroughResourceHistory()
        {
            if (_forwardCount == -1) Assert.Skip();

            int pageSize = 30;
            Bundle page = Client.TypeHistory("Patient", since: DateTimeOffset.Now.AddHours(-1), pageSize: pageSize);
            Assert.EntryIdsArePresentAndAbsoluteUrls(page);

            page = Client.Continue(page, PageDirection.Last);
            int backwardsCount = 0;

            // Browse backwards
            while (page != null)
            {
                if (page.Entry.Count > pageSize)
                    Assert.Fail("Server returned a page with more entries than set by count");

                backwardsCount += page.Entry.Count;
                Assert.EntryIdsArePresentAndAbsoluteUrls(page);
                page = Client.Continue(page, PageDirection.Previous);
            }

            if (backwardsCount != _forwardCount)
                Assert.Fail(String.Format("Paging forward returns {0} entries, backwards returned {1}",
                    _forwardCount, backwardsCount));
        }

        [SprinklerTest("HI11", "Fetch first page of full histroy")]
        public void FullHistory()
        {
            Bundle history = Client.WholeSystemHistory();
        }


        // =================================================================================
        // Static tests

        private static void CheckTransactionElements(Bundle history)
        {
            if (!history.Entry.All(e => e.Transaction != null))
            {
                Assert.Fail("Not all history results have a transaction component");

            }
        }

        private static void CheckVersionIds(Bundle history)
        {
            if (!history.Entry.Select(e => e.Transaction).All(t => new ResourceIdentity(t.Url).HasVersion))
            {
                Assert.Fail("Not all history entries have a versioned url");
                //Assert.Fail("Selflinks on returned versions do not match links returned on creation" +
                // Versions.Contains(ent.VersionId)))
            }
        }

        private static void CheckSortOrder(Bundle bundle)
        {
            DateTimeOffset maxDate = DateTimeOffset.MaxValue;

            foreach (var resource in bundle.GetResources())
            {
                if (resource.Meta == null)
                {
                    Assert.Fail("Resource with id {0} does not contain a meta element", resource.Id);
                }

                var lastUpdate = resource.Meta.LastUpdated;

                if (lastUpdate == null)
                {
                    Assert.Fail(String.Format("Result contains entry with no LastUpdate (id: {0})", resource.VersionId));
                }

                if (lastUpdate > maxDate)
                {
                    Assert.Fail("Result is not ordered on LastUpdate, first out of order has id " + resource.VersionId);
                }

                maxDate = lastUpdate.Value;
            }
        }       

    }

    

    public static class BundleExtensions
    {
        // API
        public static IEnumerable<Resource> GetResources(this Bundle bundle)
        {
            foreach(var entry in bundle.Entry)
            {
                if (entry.Resource != null) yield return entry.Resource;
            }
        }

        public static IEnumerable<string> VersionIds(this Bundle bundle)
        {
            return bundle.Entry.Select(entry => new ResourceIdentity(entry.Transaction.Url).VersionId);
        }

        public static bool HasSameElements<T>(this IEnumerable<T> list1, IEnumerable<T> list2)
        {
            return list1.AreAllFoundIn(list2) && list2.AreAllFoundIn(list1);
        }

        public static bool AreAllFoundIn<T>(this IEnumerable<T> list1, IEnumerable<T> list2)
        {
            foreach (T item in list1)
            {
                if (!list2.Contains(item)) return false;
            }
            return true;
        }

    }

    
}