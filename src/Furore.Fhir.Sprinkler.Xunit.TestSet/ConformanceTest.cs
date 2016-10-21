using Furore.Fhir.Sprinkler.Xunit.ClientUtilities;
using Furore.Fhir.Sprinkler.Xunit.ClientUtilities.XunitFhirExtensions.Attributes;
using Furore.Fhir.Sprinkler.Xunit.ClientUtilities.XunitFhirExtensions.ClassFixtures;
using Hl7.Fhir.Rest;
using Xunit;

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
            FhirAssert.ValidResourceContentTypePresent(client);
            FhirAssert.ContentLocationValidIfPresent(client);
        }
    }
}