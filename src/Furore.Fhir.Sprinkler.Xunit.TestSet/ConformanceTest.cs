using Furore.Fhir.Sprinkler.XunitRunner.FhirExtensions;
using Hl7.Fhir.Rest;
using Xunit;
using Assert = Furore.Fhir.Sprinkler.FhirUtilities.Assert;

namespace Furore.Fhir.Sprinkler.Xunit.TestSet
{
    public class ConformanceTest : IClassFixture<FhirClientFixture>
    {
        private readonly FhirClient client;

        public ConformanceTest(FhirClientFixture clientFixture)
        {
            this.client = clientFixture.Client;
        }

        [TestMetadata("CN01", "Request conformance on /metadata")]
        [Fact]
        public void GetConformanceUsingMetadata()
        {
           client.Conformance();
            Assert.ValidResourceContentTypePresent(client);
            Assert.ContentLocationValidIfPresent(client);
        }
    }
}