using System.IO;

namespace Furore.Fhir.Sprinkler.FhirUtilities
{
    public static class AssemblyResources
    {
        public static string GetResourcePath(string filename)
        {
            // DOES NOT WORK in dnx:
            // string file = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, resourcesDir, ZIPFILEPATH);

            // DOES NOT WORK EITHER (in dnx):
            //Assembly.GetExecutingAssembly().Location;

            string location = typeof(AssemblyResources).Assembly.Location;

            string path = Path.GetDirectoryName(location);
            string file = Path.Combine(path, "Resources", filename);
            return file;
        }
    }
}