using Hl7.Fhir.Rest;

namespace Furore.Fhir.Sprinkler.XunitRunner.FhirExtensions
{
    public static class TestConfiguration
    {
        public static string Url { get; internal set; }
        public static string AssemblyRootDirectory { get; internal set; }

    }

    public static class FhirClientBuilder
    {
        public static FhirClient CreateFhirClient()
        {
            //return new FhirClient(TestConfiguration.Url);
            return new LoggerFhirClient(TestConfiguration.Url);
        }
    }
}