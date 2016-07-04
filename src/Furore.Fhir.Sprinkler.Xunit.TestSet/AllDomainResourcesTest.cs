using System;
using System.Net;
using Furore.Fhir.Sprinkler.ClientUtilities.ResourceManagement;
using Furore.Fhir.Sprinkler.XunitRunner.FhirExtensions;
using Hl7.Fhir.Model;
using Xunit;

namespace Furore.Fhir.Sprinkler.Xunit.TestSet
{
    [FixtureConfiguration(@"D:\Projects\Furore\sprinkler\src\Furore.Fhir.Sprinkler.TestSet\Resources\examples.zip", FixtureType.ZipFile)]
    public class AllDomainResourcesTest : IClassFixture<FhirClientFixture>
    {
        private readonly FhirClientFixture client;

        public AllDomainResourcesTest(FhirClientFixture client)
        {
            this.client = client;
        }

        [Theory]
        [Fixture(false)]
        public  void TestSomeResource<T>(T resource)  where T: DomainResource
        {
            //resource.Id = string.Empty;
            //var created = client.Client.Create(resource);
            //var key = created.ResourceIdentity().WithoutVersion().MakeRelative().ToString();
            //Assert.NotNull(key);
          
            //TryToRead<T>(key);
            //TryToUpdate<T>(resource);
            //TryDelete<T>(key);
        }

        private void TryToUpdate<T>(DomainResource resource) 
        {
            Element element = new Code("unsure");
            try
            {
                resource.AddExtension("http://fhir.furore.com/extensions/sprinkler", element);
                client.Client.Update(resource);
            }
            catch (Exception e)
            {
                //?errors.Add("Update of " + resource.GetType().Name + " failed: " + e.Message);
            }
        }

        private void TryDelete<T>(string location) where T : DomainResource, new()
        {
            try
            {
                client.Client.Delete(location);
                ClientUtilities.Assert.Fails(client.Client, () => client.Client.Read<T>(location), HttpStatusCode.Gone);
            }
            catch (Exception e)
            {
                //?errors.Add("Deletion of " + resource.GetType().Name + " failed: " + e.Message);
            }
        }

        private void TryToRead<T>(string location) where T : DomainResource, new()
        {
            try
            {
                client.Client.Read<T>(location);
            }
            catch (Exception e)
            {
                //?errors.Add("Cannot read " + resource.GetType().Name + ": " + e.Message);
            }
        }
    }
}