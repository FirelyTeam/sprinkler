namespace Furore.Fhir.Sprinkler.ClientUtilities.ResourceManagement
{
    public class FixtureConfiguration
    {
        public string FixturesRootPath { get; set; }
        public FixtureType FixtureType { get; set; }

        public KeyProvider KeyProvider { get; set; }


        //public string FixturePath { get; set; }
        //public Type ResourceType { get; set; }
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
        MatchFixtureType
    }
}