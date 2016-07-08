using Furore.Fhir.Sprinkler.FhirUtilities.ResourceManagement;
using Hl7.Fhir.Model;
using Xunit.Abstractions;

namespace Furore.Fhir.Sprinkler.XunitRunner.FhirExtensions
{
    //public class ResourceProxy<T> : T, IXunitSerializable where T:Resource
    //{
    //    public string ResourceKey { get; set; }

    //    public T Resource { get; set; }
    //    public void Deserialize(IXunitSerializationInfo info)
    //    {
    //        ResourceFixturesProvider provider = new ResourceFixturesProvider();
    //        this.ResourceKey = info.GetValue<string>("resourceKey");
    //        throw new System.NotImplementedException();
    //    }

    //    public void Serialize(IXunitSerializationInfo info)
    //    {
    //        info.AddValue("resourceKey", ResourceKey);
    //    }
    //}
}