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

namespace Sprinkler.Tests
{
    //[SprinklerTestModule("History")]
    public class HistoryTest : SprinklerTestClass
    {
        //private CreateUpdateDeleteTest crudTests;
        private Bundle history;

        string id;
        public string Location
        {
            get
            {
                return "Patient/" + id;
            }
        }
        private DateTimeOffset? CreateDate;
        List<Uri> Versions = new List<Uri>();

        private void initialize()
        {
            id = "sprink" + new Random().Next().ToString();
            CreateDate = DateTimeOffset.Now;

            Patient patient = DemoData.GetDemoPatient();
            ResourceEntry<Patient> entry = client.Create<Patient>(patient, id, null, true);

            entry.Resource.Telecom.Add(new Contact() { System = Contact.ContactSystem.Email, Value = "info@furore.com" });
            client.Update<Patient>(entry, refresh: true);
            client.Update<Patient>(entry, refresh: true);
        }

      
        [SprinklerTest("HI01", "Request the full history for a specific resource")]
        public void HistoryForSpecificResource()
        {
            initialize();
            if (CreateDate == null) TestResult.Skip();

            history = client.History(Location);
            HttpTests.AssertEntryIdsArePresentAndAbsoluteUrls(history);

            // There's one version less here, because we don't have the deletion
            int expected = Versions.Count + 1;

            if (history.Entries.Count != expected)
                TestResult.Fail(String.Format("{0} versions expected after crud test, found {1}", expected, history.Entries.Count));

            if (!history.Entries.OfType<ResourceEntry>()
                    .All(ent => Versions.Contains(ent.SelfLink)))
                TestResult.Fail("Selflinks on returned versions do not match links returned on creation" + history.Entries.Count);

            
            checkSortOrder(history);
        }

        


        [SprinklerTest("HI02", "Request the full history for a resource with _since")]
        public void HistoryForSpecificResourceId()
        {
            if (CreateDate == null) TestResult.Skip();

            var before = CreateDate.Value.AddMinutes(-1);
            var after = before.AddHours(1);

            var history = client.History(Location, before);
            HttpTests.AssertEntryIdsArePresentAndAbsoluteUrls(history);
            checkSortOrder(history);
            var historySl = history.Entries.Select(entry=>entry.SelfLink).ToArray();

            if (!history.Entries.All(entry => historySl.Contains(entry.SelfLink)))
                TestResult.Fail("history with _since does not contain all versions of instance");

            history = client.History(Location, after);
            if (history.Entries.Count != 0)
                TestResult.Fail("Setting since to after the last update still returns history");
        }


        [SprinklerTest("HI03", "Request individual history versions from a resource")]
        public void VReadVersions()
        {
            foreach (var ent in history.Entries.OfType<ResourceEntry>())
            {
                var identity = new ResourceIdentity(ent.SelfLink);

                var version = client.Read<Patient>(identity);

                if (version == null) TestResult.Fail("Cannot find version that was present in history");

                HttpTests.AssertContentLocationPresentAndValid(client);
                var selfLink = new ResourceIdentity(client.LastResponseDetails.ContentLocation);
                if (String.IsNullOrEmpty(selfLink.Id) || String.IsNullOrEmpty(selfLink.VersionId))
                    TestResult.Fail("Optional Content-Location contains an invalid version-specific url");
            }

            foreach (var ent in history.Entries.OfType<DeletedEntry>())
            {
                var identity = new ResourceIdentity(ent.SelfLink);

                HttpTests.AssertFail(client, () => client.Read<Patient>(identity), HttpStatusCode.Gone);
            }
        }

        [SprinklerTest("HI04", "Fetching history of non-existing resource returns exception")]
        public void HistoryForNonExistingResource()
        {
            if (CreateDate == null) TestResult.Skip();

            HttpTests.AssertFail(client, () => client.History("Patient/3141592unlikely"), HttpStatusCode.NotFound);
        }

        // todo: bug in API. Werkt weer bij volgende versie.
        [SprinklerTest("HI06", "Get all history for a resource type with _since")]
        public void HistoryForResourceType()
        {
            if (CreateDate == null) TestResult.Skip();

            var before = CreateDate.Value.AddMinutes(-1);
            var after = before.AddHours(1);

            var history = client.TypeHistory<Patient>(since: before);
            HttpTests.AssertEntryIdsArePresentAndAbsoluteUrls(history);
            typeHistory = history;
            checkSortOrder(history);

            var historyLinks = history.Entries.Select(be => be.SelfLink).ToArray();

            if(!history.Entries.All(ent => historyLinks.Contains(ent.SelfLink)))
                TestResult.Fail("history with _since does not contain all versions of instance");

            history = client.History(Location, DateTimeOffset.Now.AddMinutes(1));

            if (history.Entries.Count != 0)
                TestResult.Fail("Setting since to a future moment still returns history");
        }

        private Bundle typeHistory;


        [SprinklerTest("HI08", "Get the history for the whole system with _since")]
        public void HistoryForWholeSystem()
        {
            if (CreateDate == null) TestResult.Skip();

            var before = CreateDate.Value.AddMinutes(-1);
            
            var history = client.WholeSystemHistory(before);
            HttpTests.AssertEntryIdsArePresentAndAbsoluteUrls(history);

            HttpTests.AssertHasAllForwardNavigationLinks(history); // Assumption: system has more history than pagesize
            systemHistory = history;
            checkSortOrder(history);

            var historyLinks = history.Entries.Select(be => be.SelfLink).ToArray();

            if(!Versions.All(sl => historyLinks.Contains(sl)))
                TestResult.Fail("history with _since does not contain all versions of instance");

            history = client.History(Location, DateTimeOffset.Now.AddMinutes(1));

            if (history.Entries.Count != 0)
                TestResult.Fail("Setting since to a future moment still returns history");
        }

        private Bundle systemHistory;

        [SprinklerTest("HI09", "Paging forward through a resource type history")]
        public void PageFwdThroughResourceHistory()
        {
            var pageSize = 30;
            var page = client.TypeHistory<Patient>(pageSize: pageSize);
            HttpTests.AssertEntryIdsArePresentAndAbsoluteUrls(page);

            forwardCount = 0;

            // Browse forwards
            while (page != null)
            {
                if (page.Entries.Count > pageSize)
                    TestResult.Fail("Server returned a page with more entries than set by _count");

                forwardCount += page.Entries.Count;
                lastPage = page;
                HttpTests.AssertEntryIdsArePresentAndAbsoluteUrls(page);
                page = client.Continue(page);
                
            }

            //if (total.HasValue && forwardCount < total)
            //    TestResult.Fail(String.Format("Paging did not return all entries(expected at least {0}, {1} returned)", 
            //                    total, forwardCount));
        }

        Bundle lastPage = null;
        int forwardCount = -1;

        [SprinklerTest("HI10", "Page backwards through a resource type history")]
        public void PageBackThroughResourceHistory()
        {
            if (forwardCount == -1) TestResult.Skip();

            var pageSize = 30;
            var page = client.TypeHistory<Patient>(pageSize: pageSize);
            HttpTests.AssertEntryIdsArePresentAndAbsoluteUrls(page);

            page = client.Continue(page, PageDirection.Last);
            var backwardsCount = 0;

            // Browse backwards
            while (page != null)
            {
                if (page.Entries.Count > pageSize)
                    TestResult.Fail("Server returned a page with more entries than set by count");

                backwardsCount += page.Entries.Count;
                HttpTests.AssertEntryIdsArePresentAndAbsoluteUrls(page);
                page = client.Continue(page, PageDirection.Previous);
                
            }

            if (backwardsCount != forwardCount)
                TestResult.Fail(String.Format("Paging forward returns {0} entries, backwards returned {1}", 
                            forwardCount, backwardsCount));
        }


        private static void checkSortOrder(Bundle bundle)
        {
            DateTimeOffset maxDate = DateTimeOffset.MaxValue;

            foreach (var be in bundle.Entries)
            {
                DateTimeOffset? lastUpdate = be is ResourceEntry ? ((be as ResourceEntry).LastUpdated) :
                            ((be as DeletedEntry).When);

                if (lastUpdate == null)
                    TestResult.Fail(String.Format("Result contains entry with no LastUpdate (id: {0})", be.SelfLink));

                if (lastUpdate > maxDate)
                    TestResult.Fail("Result is not ordered on LastUpdate, first out of order has id " + be.SelfLink);

                maxDate = lastUpdate.Value;
            }
        }
    }
}
