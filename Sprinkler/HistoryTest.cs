using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;
using System.Xml;
using Newtonsoft.Json;
using Hl7.Fhir.Client;
using Hl7.Fhir.Support;
using Hl7.Fhir.Model;


namespace Sprinkler
{
    [SprinklerTestModule("History")]
    public class HistoryTest
    {
        FhirClient client;
        private CreateUpdateDeleteTest crudTests;

        public HistoryTest(Uri fhirUri, CreateUpdateDeleteTest crudTests)
        {
            client = new FhirClient(fhirUri);
            this.crudTests = crudTests;
        }

        [SprinklerTest("get history for specific resource id")]
        public void HistoryForSpecificResource()
        {
            if (crudTests.CreateDate == null) TestResult.Skipped();

            var history = client.History<Patient>(crudTests.CrudId);

            // There's one version less here, because we don't have the deletion
            int expected = crudTests.Versions.Count + 1;

            if (history.Entries.Count != expected)
                TestResult.Fail(String.Format("{0} versions expected after crud test, found {1}",
                                    expected, history.Entries.Count));

            if (!history.Entries.FilterResourceEntries()
                    .All(ent => crudTests.Versions.Contains(ent.SelfLink)))
                TestResult.Fail("Selflinks on returned versions do not match links returned on creation" + history.Entries.Count);

            instanceHistory = history;

            checkSortOrder(history);
        }

        private Bundle instanceHistory;


        [SprinklerTest("get history for specific resource id with _since")]
        public void HistoryForSpecificResourceId()
        {
            if (crudTests.CreateDate == null) TestResult.Skipped();

            var before = crudTests.CreateDate.Value.AddMinutes(-1);
            var after = before.AddHours(1);

            var history = client.History<Patient>(crudTests.CrudId, before);
            checkSortOrder(history);
            var historySl = history.Entries.Select(entry=>entry.SelfLink).ToArray();

            if (!instanceHistory.Entries.All(entry => historySl.Contains(entry.SelfLink)))
                TestResult.Fail("history with _since does not contain all versions of instance");

            history = client.History<Patient>(crudTests.CrudId, after);
            if (history.Entries.Count != 0)
                TestResult.Fail("Setting since to after the last update still returns history");
        }


        [SprinklerTest("fetch individual versions using vread")]
        public void VReadVersions()
        {
            foreach (var ent in instanceHistory.Entries.FilterResourceEntries())
            {
                var rl = new ResourceLocation(ent.SelfLink);

                var version = client.VRead<Patient>(rl.Id, rl.VersionId);

                if (version == null) TestResult.Fail("Cannot find version that was present in history");

                HttpTests.AssertContentLocationPresentAndValid(client);
                var selfLink = new ResourceLocation(client.LastResponseDetails.ContentLocation);
                if (String.IsNullOrEmpty(selfLink.Id) || String.IsNullOrEmpty(selfLink.VersionId))
                    TestResult.Fail("Optional Content-Location contains an invalid version-specific url");
            }

            foreach (var ent in instanceHistory.Entries.FilterDeletedEntries())
            {
                var rl = new ResourceLocation(ent.SelfLink);

                HttpTests.AssertFail(client, () => client.VRead<Patient>(rl.Id, rl.VersionId), HttpStatusCode.Gone);
            }
        }

        [SprinklerTest("try to fetch history of non-existing resource")]
        public void HistoryForNonExistingResource()
        {
            if (crudTests.CreateDate == null) TestResult.Skipped();

            HttpTests.AssertFail(client, () => client.History<Patient>("3141592unlikely"), HttpStatusCode.NotFound);
        }

        [SprinklerTest("get history for specific resource type with _since")]
        public void HistoryForResourceType()
        {
            if (crudTests.CreateDate == null) TestResult.Skipped();

            var before = crudTests.CreateDate.Value.AddMinutes(-1);
            var after = before.AddHours(1);

            var history = client.History<Patient>(before);
            typeHistory = history;
            checkSortOrder(history);

            var historyLinks = history.Entries.Select(be => be.SelfLink).ToArray();

            if(!instanceHistory.Entries.All(ent => historyLinks.Contains(ent.SelfLink)))
                TestResult.Fail("history with _since does not contain all versions of instance");

            history = client.History<Patient>(crudTests.CrudId, DateTimeOffset.Now.AddMinutes(1));

            if (history.Entries.Count != 0)
                TestResult.Fail("Setting since to a future moment still returns history");
        }

        private Bundle typeHistory;


        [SprinklerTest("get history for whole system with _since")]
        public void HistoryForWholeSystem()
        {
            if (crudTests.CreateDate == null) TestResult.Skipped();

            var before = crudTests.CreateDate.Value.AddMinutes(-1);

            var history = client.History(before);
            systemHistory = history;
            checkSortOrder(history);

            var historyLinks = history.Entries.Select(be => be.SelfLink).ToArray();

            if(!crudTests.Versions.All(sl => historyLinks.Contains(sl)))
                TestResult.Fail("history with _since does not contain all versions of instance");

            history = client.History<Patient>(crudTests.CrudId, DateTimeOffset.Now.AddMinutes(1));

            if (history.Entries.Count != 0)
                TestResult.Fail("Setting since to a future moment still returns history");
        }

        private Bundle systemHistory;

        [SprinklerTest("page forward through resource type history")]
        public void PageFwdThroughResourceHistory()
        {
            var pageSize = 30;
            var page = client.History<Patient>(count: pageSize);
            var total = page.TotalResults;

            forwardCount = 0;

            // Browse forwards
            while (page != null)
            {
                if (page.Entries.Count > pageSize)
                    TestResult.Fail("Server returned a page with more entries than set by _count");

                forwardCount += page.Entries.Count;
                lastPage = page;
                page = client.Continue(page);
            }

            if (total.HasValue && forwardCount < total)
                TestResult.Fail(String.Format("Paging did not return all entries(expected at least {0}, {1} returned)", 
                                total, forwardCount));
        }

        Bundle lastPage = null;
        int forwardCount = -1;

        [SprinklerTest("page backwards through resource type history")]
        public void PageBackThroughResourceHistory()
        {
            if (forwardCount == -1) TestResult.Skipped();

            var pageSize = 30;
            var page = client.History<Patient>(count: pageSize);

            page = client.Continue(page, PageDirection.Last);
            var backwardsCount = 0;

            // Browse backwards
            while (page != null)
            {
                if (page.Entries.Count > pageSize)
                    TestResult.Fail("Server returned a page with more entries than set by count");

                backwardsCount += page.Entries.Count;
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
