using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Furore.Fhir.Sprinkler.FhirUtilities;
using Furore.Fhir.Sprinkler.XunitRunner.FhirExtensions;
using Hl7.Fhir.Model;
using Hl7.Fhir.Rest;
using Xunit;
using Assert = Furore.Fhir.Sprinkler.FhirUtilities.Assert;

namespace Furore.Fhir.Sprinkler.Xunit.TestSet
{
    public interface ISetupAndTeardown 
    {
        void Setup();
        void Teardown();
    }

    public abstract class SetupAndTeardown<T> : ISetupAndTeardown, IDisposable where T : class, ISetupAndTeardown, new()
    {

        public SetupAndTeardown()
        {
            this.Setup();
        }

        public void Dispose()
        {
            this.Teardown();
        }

        public abstract void Setup();
        public abstract void Teardown();
    }

    public class HistoryTest : IClassFixture<HistoryTest.Context>
    {
        private readonly FhirClient client;
        private readonly HistoryTest.Context context;

        public class Context : SetupAndTeardown<Context>
        {
            public DateTimeOffset CreationDate { get; private set; }

            private readonly IList<string> versions = new List<string>();

            private Patient patient;

            public IEnumerable<string> PatientVersions
            {
                get { return versions.ToList(); }
            }

            public Patient Patient
            {
                get { return patient; }
            }

            public string Location
            {
                get { return patient.GetReferenceId(); }
            }

            public override void Setup()
            {
                FhirClient client = FhirClientBuilder.CreateFhirClient();
                CreationDate = DateTimeOffset.Now;
                patient = CreatePatient();
                if (patient.Meta != null && patient.Meta.LastUpdated != null)
                {
                    CreationDate = patient.Meta.LastUpdated.Value;
                }
                patient = client.Create(patient);
                versions.Add(patient.VersionId);

                patient.Telecom.Add(new ContactPoint
                {
                    System = ContactPoint.ContactPointSystem.Email,
                    Value = "info@furore.com"
                });

                patient = client.Update(patient, true);
                versions.Add(patient.VersionId);

                patient = client.Update(patient, true);
                versions.Add(patient.VersionId);
            }

            private Patient CreatePatient(string family = "Adams", params string[] given)
            {
                var p = new Patient();
                var n = new HumanName();
                foreach (string g in given)
                {
                    n.WithGiven(g);
                }

                n.AndFamily(family);
                p.Name = new List<HumanName>();
                p.Name.Add(n);
                return p;
            }

            public override void Teardown()
            {
               FhirClientBuilder.CreateFhirClient().Delete(patient);
            }
        }

        public HistoryTest(HistoryTest.Context context)
        {
            this.context = context;
            this.client = FhirClientBuilder.CreateFhirClient();
        }

        [TestMetadata("HI01", "Request full history for specific resource")]
        [Fact]
        public void HistoryForSpecificResource()
        {
            Bundle history = client.History(context.Location);

            BundleAssert.CheckMinimumNumberOfElementsInBundle(history, context.PatientVersions.Count());
            CheckHistoryBundleBasicRequirements(history);
        }

        [TestMetadata("HI02", "Request full history for specific resource using the _since parameter (set to before the resource was created)")]
        [Fact]
        public void HistoryForSpecificResource_SinceParameterSetToBeforeResourceWasCreated()
        {
            Bundle history = client.History(context.Location, context.CreationDate);

            CheckHistoryBundleBasicRequirements(history);
            BundleAssert.ContainsAllVersionIds(history, context.PatientVersions);
        }

        [TestMetadata("HI03", "Request full history for specific resource using the _since parameter (set to a future date)")]
        [Fact]
        public void HistoryForSpecificResource_SinceParameterSetToFutureDate()
        {
            Bundle history = client.History(context.Location, DateTimeOffset.Now.AddHours(1));

            BundleAssert.CheckBundleEmpty(history);
        }

        [TestMetadata("HI04", "Fetching history of non-existing resource returns exception")]
        [Fact]
        public void HistoryForNonExistingResource()
        {
            Assert.Fails(client, () => client.History("Patient/3141592unlikely"), HttpStatusCode.NotFound);
        }

        [TestMetadata("HI05", "Get all history for a resource type with _since (set to before test initialization data was created)")]
        [Fact]
        public void HistoryForResourceType_SinceParameterSetToBeforeTestDataWasCreated()
        {
            Bundle history = client.TypeHistory(ResourceType.Patient.ToString(), context.CreationDate);

            CheckHistoryBundleBasicRequirements(history);
            BundleAssert.CheckConditionForResources(history, r => r.Id != null || r.VersionId != null, "Resources must have id/versionId information");
        }

        [TestMetadata("HI06", "Get all history for a resource type with _since (set to a future date)")]
        [Fact]
        public void HistoryForResourceType_SinceParameterSetToFutureDate()
        {
            Bundle history = client.TypeHistory(ResourceType.Patient.ToString(), DateTimeOffset.Now.AddHours(1));

            BundleAssert.CheckBundleEmpty(history);
        }


        [TestMetadata("HI07", "Get the history for the whole system with _since (set to the moment test initialization data was created)")]
        [Fact]
        public void HistoryForWholeSysteme_SinceParameterSetToBeforeTestDataWasCreated()
        {
            Bundle history = client.WholeSystemHistory(context.CreationDate);

            CheckHistoryBundleBasicRequirements(history);
            BundleAssert.CheckConditionForResources(history, r => r.Id != null || r.VersionId != null,
                "Resources must have id/versionId information");
            BundleAssert.CheckMinimumNumberOfElementsInBundle(history, context.PatientVersions.Count());
        }

        [TestMetadata("HI08", "Fetch first page of full histroy")]
        [Fact]
        public void FullHistory()
        {
            Bundle history = client.WholeSystemHistory();
            BundleAssert.CheckMinimumNumberOfElementsInBundle(history, context.PatientVersions.Count());
            CheckHistoryBundleBasicRequirements(history);
        }

        [TestMetadata("HI09", "Get the history for the whole system with _since (set to a future date)")]
        [Fact]
        public void HistoryForWholeSystem()
        {
            Bundle history = client.WholeSystemHistory(DateTimeOffset.Now.AddHours(1));
            BundleAssert.CheckBundleEmpty(history);
        }

        [TestMetadata("HI10", "Paging forward and backward through a resource type history")]
        [Fact]
        public void PageThroughResourceHistory()
        {
            int pageSize = 1;
            Bundle page = client.TypeHistory(ResourceType.Patient.ToString(), context.CreationDate, pageSize: pageSize);
            int patientVersions = context.PatientVersions.Count();

            int forwardCount = TestBundlePages(page, PageDirection.Next, pageSize);
            int backwardsCount = TestBundlePages(client.Continue(page, PageDirection.Last), PageDirection.Previous,
                pageSize);
          

            Assert.IsTrue(forwardCount == backwardsCount,
                String.Format("Paging forward returns {0} entries, backwards returned {1}",
                    forwardCount, backwardsCount));
            Assert.IsTrue(forwardCount >= patientVersions, string.Format("Bundle should have at least {0} pages", patientVersions));
        }

        private int TestBundlePages(Bundle page, PageDirection direction, int pageSize)
        {
            int pageCount = 0;
            while (page != null)
            {
                pageCount++;
                BundleAssert.CheckConditionForResources(page, r => r.Id != null || r.VersionId != null,
                    "Resources must have id/versionId information");
                BundleAssert.CheckMaximumNumberOfElementsInBundle(page, pageSize);

                page = client.Continue(page, direction);
            }
            return pageCount;
        }

        private void CheckHistoryBundleBasicRequirements(Bundle history)
        {
            Assert.EntryIdsArePresentAndAbsoluteUrls(history);
            BundleAssert.CheckConditionForAllElements(history, e => e.Request != null,
                "A history entry must contain a transaction element");
            BundleAssert.CheckConditionForAllElements(history, e => new ResourceIdentity(e.Request.Url).HasVersion,
                "A history entry must contain a versioned url");
            BundleAssert.CheckConditionForResourcesWithIdInformation(history, r => r.Meta != null,
                "A resource in a history entry must contain a meta element");
            BundleAssert.CheckConditionForResourcesWithIdInformation(history, r => r.Meta.LastUpdated != null,
                "A resource in a history entry must contain LastUpdate information");
            BundleAssert.CheckResourcesInReverseOrder(history, r => r.Meta.LastUpdated.Value);
        }
    }
}