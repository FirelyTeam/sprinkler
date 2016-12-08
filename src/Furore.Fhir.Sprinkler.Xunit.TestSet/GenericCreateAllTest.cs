using Furore.Fhir.Sprinkler.FhirUtilities.ResourceManagement;
using Furore.Fhir.Sprinkler.Xunit.ClientUtilities;
using Furore.Fhir.Sprinkler.Xunit.ClientUtilities.XunitFhirExtensions.Attributes;
using Hl7.Fhir.Model;
using Hl7.Fhir.Rest;
using Xunit;

namespace Furore.Fhir.Sprinkler.Xunit.TestSet
{
    [FixtureConfiguration("examples.zip", FixtureType.ZipFile)]
    public class GenericCreateAllTest
    {
        [Theory]
        [Fixture(true)]
        [TestMetadata(new[] { "AR{T}", "SparkPluggable", "CreateAllExamples" }, "Create all {T}")]
        public void CreateAllResource<T>(T resource) where T : DomainResource, new()
        {
            TestCreate<T>(resource);
        }

        private T TestCreate<T>(T resource) where T : DomainResource, new()
        {
            FhirClient client = FhirClientBuilder.CreateFhirClient();
            resource.Id = string.Empty;
            var created = client.Create(resource);
            var key = GetKey(created);
            Assert.NotNull(key);

            return created;
        }

        private string GetKey(Resource resource)
        {
            return resource.ResourceIdentity().WithoutVersion().MakeRelative().ToString();
        }
    }
}