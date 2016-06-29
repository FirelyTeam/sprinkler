using Hl7.Fhir.Rest;

namespace Furore.Fhir.Sprinkler.XunitRunner.FhirExtensions
{
    public class FhirClientFixture
    {
        public FhirClient Client { get; private set; }
        public FhirClientFixture()
        {
            Client = new FhirClient(TestConfiguration.Url);
        }
    }
}