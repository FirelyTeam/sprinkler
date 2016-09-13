namespace Furore.Fhir.Sprinkler.FhirUtilities.ResourceManagement
{
    public class FixtureConfiguration
    {
        public string FixturesRootPath { get; set; }
        public FixtureType FixtureType { get; set; }

        public KeyProvider KeyProvider { get; set; }
    }

    public enum FixtureType
    {
        File,
        Resx,
        ZipFile
    }

    public enum KeyProvider
    {
        MatchFixtureName,
        MatchFixtureType,
        All
    }
}