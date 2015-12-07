/* 
 * Copyright (c) 2014, Furore (info@furore.com) and contributors
 * See the file CONTRIBUTORS for details.
 * 
 * This file is licensed under the BSD 3-Clause license
 * available at https://raw.github.com/furore-fhir/sprinkler/master/LICENSE
 */

using System;
using System.Collections.Generic;
using System.Net;
using Furore.Fhir.Sprinkler.Framework.Framework;
using Furore.Fhir.Sprinkler.Framework.Utilities;
using Hl7.Fhir.Model;
using Hl7.Fhir.Rest;

namespace Furore.Fhir.Sprinkler.TestSet
{
    [SprinklerModule("History")]
    public class HistoryTest : SprinklerTestClass
    {
        private DateTimeOffset historyStartDate;
        private readonly IList<string> versions = new List<string>();
        private string id;
        private string location;

        [ModuleInitialize]
        public void Initialize()
        {
            historyStartDate = DateTimeOffset.Now;
            Patient patient = Utils.GetNewPatient();
            patient = Client.Create(patient);

            id = patient.Id;
            location = "Patient/" + id; 

            versions.Add(patient.VersionId);

            patient.Telecom.Add(new ContactPoint
            {
                System = ContactPoint.ContactPointSystem.Email,
                Value = "info@furore.com"
            });

            patient = Client.Update(patient, true);


            versions.Add(patient.VersionId);

            patient = Client.Update(patient, true);

            versions.Add(patient.VersionId);

            Client.Delete(location);
        }

        [SprinklerTest("HI01", "Request full history for specific resource")]
        public void HistoryForSpecificResource()
        {
            Bundle history = Client.History(location);

            // There's one version less here, because we don't have the deletion
            BundleAssert.CheckMinimumNumberOfElementsInBundle(history, versions.Count + 1);
            CheckHistoryBundleBasicRequirements(history);

        }

        [SprinklerTest("HI02", "Request full history for specific resource using the _since parameter (set to before the resource was created)")]
        public void HistoryForSpecificResource_SinceParameterSetToBeforeResourceWasCreated()
        {
            DateTimeOffset before = historyStartDate.AddMinutes(-1);

            Bundle history = Client.History(location, before);

            CheckHistoryBundleBasicRequirements(history);
            BundleAssert.ContainsAllVersionIds(history, versions);
        }

        [SprinklerTest("HI03", "Request full history for specific resource using the _since parameter (set to a future date)")]
        public void HistoryForSpecificResource_SinceParameterSetToFutureDate()
        {
            DateTimeOffset after = DateTimeOffset.Now.AddHours(1);

            Bundle history = Client.History(location, after);

            BundleAssert.CheckBundleEmpty(history);
        }

        [SprinklerTest("HI04", "Fetching history of non-existing resource returns exception")]
        public void HistoryForNonExistingResource()
        {
            Assert.Fails(Client, () => Client.History("Patient/3141592unlikely"), HttpStatusCode.NotFound);
        }

        [SprinklerTest("HI05", "Get all history for a resource type with _since (set to before test initialization data was created)")]
        public void HistoryForResourceType_SinceParameterSetToBeforeTestDataWasCreated()
        {
            DateTimeOffset before = historyStartDate.AddMinutes(-1);
            Bundle history = Client.TypeHistory("Patient", before);

            CheckHistoryBundleBasicRequirements(history);
            BundleAssert.CheckConditionForResources(history, r => r.Id != null || r.VersionId != null, "Resources must have id/versionId information");
        }

        [SprinklerTest("HI06", "Get all history for a resource type with _since (set to a future date)")]
        public void HistoryForResourceType_SinceParameterSetToFutureDate()
        {
            Bundle history = Client.History(location, DateTimeOffset.Now.AddHours(1));

            BundleAssert.CheckBundleEmpty(history);
        }

        [SprinklerTest("HI07", "Get the history for the whole system with _since (set to before test initialization data was created)")]
        public void HistoryForWholeSysteme_SinceParameterSetToBeforeTestDataWasCreated()
        {
            DateTimeOffset before = historyStartDate.AddMinutes(-1);

            Bundle history = Client.WholeSystemHistory(before);

            CheckHistoryBundleBasicRequirements(history);
            BundleAssert.CheckConditionForResources(history, r => r.Id != null || r.VersionId != null,
                "Resources must have id/versionId information");
        }

        [SprinklerTest("HI08", "Get the history for the whole system with _since (set to a future date)")]
        public void HistoryForWholeSystem()
        {
            Bundle history = Client.History(location, DateTimeOffset.Now.AddHours(1));

            BundleAssert.CheckBundleEmpty(history);
        }

        [SprinklerTest("HI09", "Paging forward and backward through a resource type history")]
        public void PageThroughResourceHistory()
        {
            int pageSize = 1;
            Bundle page = Client.TypeHistory("Patient", historyStartDate.AddMinutes(-1), pageSize: pageSize);
            

            int forwardCount = TestBundlePages(page, PageDirection.Next, pageSize);
            int backwardsCount = TestBundlePages(Client.Continue(page, PageDirection.Last), PageDirection.Previous, pageSize);

            if (forwardCount != backwardsCount)
            {
                Assert.Fail(String.Format("Paging forward returns {0} entries, backwards returned {1}",
                     forwardCount, backwardsCount));   
            }
        }

        [SprinklerTest("HI11", "Fetch first page of full histroy")]
        public void FullHistory()
        {
            Bundle history = Client.WholeSystemHistory();
            BundleAssert.CheckMinimumNumberOfElementsInBundle(history, versions.Count + 1);
            CheckHistoryBundleBasicRequirements(history);
        }

        //[SprinklerTest("HI12", "Request individual history versions from a resource")]
        //public void VReadVersions()
        //{
        //    foreach (string version in versions)
        //    {
        //        var locationForVersion = ResourceIdentity.Build("Patient", this.id, version);
        //        Patient patient = Client.Read<Patient>(location);

        //        if (patient == null)
        //        {
        //            Assert.Fail("Cannot find version that was present in history");
        //        }

        //        Assert.ContentLocationPresentAndValid(this.Client);

        //        var identity = patient.ResourceIdentity();

        //        if (String.IsNullOrEmpty(identity.Id) || String.IsNullOrEmpty(identity.VersionId))
        //            Assert.Fail("Optional Content-Location contains an invalid version-specific url");
        //    }

        //    //TODO: CorinaC: Commented code must be rewritten

        //    foreach (var ent in (Utils.GetDeleted(history.Entry)))
        //    {
        //        var identity = new ResourceIdentity(ent.Request.Url);
        //        Assert.Fails(Client, () => Client.Read<Patient>(identity), HttpStatusCode.Gone);
        //    }
        //}

        private int TestBundlePages(Bundle page, PageDirection direction, int pageSize)
        {
            int pageCount = 0;
            while (page != null)
            {
                pageCount++;
                BundleAssert.CheckConditionForResources(page, r => r.Id != null || r.VersionId != null,
                    "Resources must have id/versionId information");
                BundleAssert.CheckMaximumNumberOfElementsInBundle(page, pageSize);

                page = Client.Continue(page, direction);
            }
            return pageCount;
        }

        private void CheckHistoryBundleBasicRequirements(Bundle history)
        {
            Assert.EntryIdsArePresentAndAbsoluteUrls(history);
            BundleAssert.CheckConditionForAllElements(history, e => e.Request != null,
                "A history entry must contain a transaction element");
            //ToDo: check if this is necessary
            //BundleAssert.CheckConditionForAllElements(history, e => new ResourceIdentity(e.Request.Url).HasVersion,
            //    "A history entry must contain a versioned url");
            BundleAssert.CheckConditionForResourcesWithIdInformation(history, r => r.Meta != null,
                "A resource in a history entry must contain a meta element");
            BundleAssert.CheckConditionForResourcesWithIdInformation(history, r => r.Meta.LastUpdated != null,
                "A resource in a history entry must contain LastUpdate information");
            BundleAssert.CheckResourcesInReverseOrder(history, r => r.Meta.LastUpdated.Value);
        }
    }

    //public static class BundleExtensions
    //{
    //    // API
    //    public static IEnumerable<Resource> GetResources(this Bundle bundle)
    //    {
    //        foreach(var entry in bundle.Entry)
    //        {
    //            if (entry.Resource != null) yield return entry.Resource;
    //        }
    //    }

    //    public static IEnumerable<string> VersionIds(this Bundle bundle)
    //    {
    //        return bundle.Entry.Select(entry => new ResourceIdentity(entry.Request.Url).VersionId);
    //    }

    //    public static bool HasSameElements<T>(this IEnumerable<T> list1, IEnumerable<T> list2)
    //    {
    //        return list1.AreAllFoundIn(list2) && list2.AreAllFoundIn(list1);
    //    }

    //    public static bool AreAllFoundIn<T>(this IEnumerable<T> list1, IEnumerable<T> list2)
    //    {
    //        foreach (T item in list1)
    //        {
    //            if (!list2.Contains(item)) return false;
    //        }
    //        return true;
    //    }
    //}
}