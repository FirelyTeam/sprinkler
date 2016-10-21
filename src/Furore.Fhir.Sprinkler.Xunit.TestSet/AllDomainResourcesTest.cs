using System.Net;
using Furore.Fhir.Sprinkler.FhirUtilities.ResourceManagement;
using Furore.Fhir.Sprinkler.Xunit.ClientUtilities;
using Furore.Fhir.Sprinkler.Xunit.ClientUtilities.XunitFhirExtensions.Attributes;
using Furore.Fhir.Sprinkler.Xunit.ClientUtilities.XunitFhirExtensions.ClassFixtures;
using Hl7.Fhir.Model;
using Xunit;

namespace Furore.Fhir.Sprinkler.Xunit.TestSet
{
    [FixtureConfiguration("examples.zip", FixtureType.ZipFile)]
    public class AllDomainResourcesTest : IClassFixture<FhirClientFixture>
    {
        private readonly FhirClientFixture client;

        public AllDomainResourcesTest(FhirClientFixture client)
        {
            this.client = client;
        }

        [Theory]
        [Fixture]
        [TestMetadata("AR{T}", "Create read update delete on {T}")]
        public void TestAllResource<T>(T resource) where T : DomainResource, new()
        {
            resource = TestCreate<T>(resource);
            TestRead<T>(GetKey(resource));
            TestUpdate<T>(resource);
            TestDelete<T>(GetKey(resource));
        }

        private string GetKey(Resource resource)
        {
            return resource.ResourceIdentity().WithoutVersion().MakeRelative().ToString();
        }

        private T TestCreate<T>(T resource) where T : DomainResource, new()
        {
            resource.Id = string.Empty;
            var created = client.Client.Create(resource);
            var key = GetKey(created);
            Assert.NotNull(key);

            return created;
        }

        private void TestUpdate<T>(T resource) where T : DomainResource, new()
        {
            Element element = new Code("unsure");
            resource.AddExtension("http://fhir.furore.com/extensions/sprinkler", element);
            client.Client.Update(resource);
        }

        private void TestDelete<T>(string location) where T : DomainResource, new()
        {
            client.Client.Delete(location);
            FhirAssert.Fails(client.Client, () => client.Client.Read<T>(location), HttpStatusCode.Gone);
        }

        private void TestRead<T>(string location) where T : DomainResource, new()
        {
            client.Client.Read<T>(location);
        }
    }
}