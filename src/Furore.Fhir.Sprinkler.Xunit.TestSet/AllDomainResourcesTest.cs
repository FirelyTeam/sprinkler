using System;
using System.Net;
using Furore.Fhir.Sprinkler.FhirUtilities.ResourceManagement;
using Furore.Fhir.Sprinkler.XunitRunner.FhirExtensions;
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

        //[Theory]
        //[Fixture(false, ResourceType.Account, ResourceType.Patient)]
        //[TestMetadata("AR{T}", "Create read update delete on {T}")]
        //public void TestSomeResource<T>(T resource) where T : DomainResource, new()
        //{
        //    resource = TestCreate<T>(resource);
        //    TestRead<T>(GetKey(resource));
        //    TestUpdate<T>(resource);
        //    TestDelete<T>(GetKey(resource));
        //}

        [Theory]
        [Fixture(false)]
        [TestMetadata("AR{T}", "Create read update delete on {T}")]
        public void TestAllResource<T>(T resource) where T : DomainResource, new()
        {
            resource = TestCreate<T>(resource);
            TestRead<T>(GetKey(resource));
            TestUpdate<T>(resource);
            TestDelete<T>(GetKey(resource));
        }

        //[Theory]
        //[Fixture(false)]
        //[TestMetadata("AR01", "Create read update delete on Patient")]
        //public void TestSomeResource(Patient resource, Account account) 
        //{
        //    resource = TestCreate(resource);
        //    TestRead<Patient>(GetKey(resource));
        //    TestUpdate(resource);
        //    TestDelete<Patient>(GetKey(resource));
        //}

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
            FhirUtilities.Assert.Fails(client.Client, () => client.Client.Read<T>(location), HttpStatusCode.Gone);
        }

        private void TestRead<T>(string location) where T : DomainResource, new()
        {
            client.Client.Read<T>(location);
        }
    }
}