using Hl7.Fhir.Model;

namespace Furore.Fhir.Sprinkler.XunitRunner.FhirExtensions
{
    public static class ResourceExtension
    {
        public static string GetReferenceId(this Resource resource)
        {
            return  string.Format("{0}/{1}", resource.TypeName, resource.Id);
        }
    }
}