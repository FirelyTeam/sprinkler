using Hl7.Fhir.Rest;

namespace Furore.Fhir.Sprinkler.Xunit.ClientUtilities
{
    public static class FhirClientBuilder
    {
        public static FhirClient CreateFhirClient()
        {
            //return new FhirClient(TestConfiguration.Url);
            return new LoggerFhirClient(TestConfiguration.Url);
        }
    }
    public static class TestConfiguration
    {
        public static string Url { get; set; }
        public static string AssemblyRootDirectory { get; set; }

    }
}