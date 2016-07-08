using System;
using Furore.Fhir.Sprinkler.FhirUtilities.ResourceManagement;
using Furore.Fhir.Sprinkler.XunitRunner.FhirExtensions;
using Hl7.Fhir.Model;
using Hl7.Fhir.Rest;
using Xunit;
using Assert = Furore.Fhir.Sprinkler.FhirUtilities.Assert;
using System.Linq;
using System.Net;

namespace Furore.Fhir.Sprinkler.Xunit.TestSet
{
    [FixtureConfiguration(FixtureType.File)]
    [TestCaseOrderer("Furore.Fhir.Sprinkler.XunitRunner.FhirExtensions.PriorityOrderer", "Furore.Fhir.Sprinkler.XunitRunner")]
    public class BinaryTest : IClassFixture<TestDependencyContext<Binary>>, IClassFixture<FhirClientFixture>
    {
        private readonly TestDependencyContext<Binary> dependencyContext;
        private readonly FhirClient client;

        public BinaryTest(TestDependencyContext<Binary> dependencyContext, FhirClientFixture client)
        {
            this.dependencyContext = dependencyContext;
            this.client = client.Client;
        }

        [Theory, TestPriority(0)]
        [TestMetadata("BI01", "Create a binary")]
        [Fixture(false, "patient-example.xml")]
        public void CreateBinary(Patient patient)
        {
            client.PreferredFormat = ResourceFormat.Xml;
            client.ReturnFullResource = true;
            dependencyContext.Dependency = GetPhotoBinary(patient);

            Binary received = client.Create(dependencyContext.Dependency);

            CheckBinary(received);
            dependencyContext.Dependency = received;
        }

        [Fact, TestPriority(1)]
        [TestMetadata("BI02", "Read binary as xml")]
        public void ReadBinaryAsXml()
        {
            if (dependencyContext.Dependency == null) FhirUtilities.Assert.Skip();

            client.PreferredFormat = ResourceFormat.Xml;
            client.UseFormatParam = true;
            client.ReturnFullResource = true;

            Binary result = client.Read<Binary>(dependencyContext.Id);
            Assert.ResourceResponseConformsTo(client, client.PreferredFormat);

            CheckBinary(result);
            dependencyContext.Dependency = result;
        }

        [Fact, TestPriority(2)]
        [TestMetadata("BI03", "Read binary as json")]
        public void ReadBinaryAsJson()
        {
            if (dependencyContext.Dependency == null) FhirUtilities.Assert.Skip();

            client.PreferredFormat = ResourceFormat.Json;
            client.UseFormatParam = false;
            client.ReturnFullResource = true;

            Binary result = client.Read<Binary>(dependencyContext.Id);
            Assert.ResourceResponseConformsTo(client, client.PreferredFormat);

            CheckBinary(result);
            dependencyContext.Dependency = result;
        }

        [Fact, TestPriority(3)]
        [TestMetadata("BI04", "Update binary - This might fail because FHIR.API doesn't send binary resources in a resource envelope and the documentation is not clear if FHIR servers should accept it like that.")]
        public void UpdateBinary()
        {
            if (dependencyContext.Dependency == null) FhirUtilities.Assert.Skip();

            dependencyContext.Dependency.Content = dependencyContext.Dependency.Content.Reverse().ToArray();
            Binary result = client.Update(dependencyContext.Dependency);

            CheckBinary(result);
        }

        [TestMetadata("BI05", "Delete binary")]
        [Fact, TestPriority(4)]

        public void DeleteBinary()
        {
            if (dependencyContext.Dependency == null) FhirUtilities.Assert.Skip();

            client.Delete(dependencyContext.Id);

            Assert.Fails(client, () => client.Read<Binary>(dependencyContext.Id), HttpStatusCode.Gone);
        }

        private Binary GetPhotoBinary(Patient patient)
        {
            var bin = new Binary();

            // NB: in the default patient-example there is no photo element.
            // Copy the photo element from the current example when replacing this file!
            bin.Content = patient.Photo[0].Data;

            bin.ContentType = patient.Photo[0].ContentType;

            return bin;
        }

        private void CheckBinary(Binary result)
        {
            Assert.LocationPresentAndValid(client);
            Assert.IsTrue(dependencyContext.Dependency.ContentType == result.ContentType, "ContentType of the received binary is not correct");
            CompareData(dependencyContext.Dependency.Content, result);

        }

        private static void CompareData(byte[] data, Binary received)
        {
            if (data.Length != received.Content.Length)
                Assert.Fail("Binary data returned has a different size");
            for (int pos = 0; pos < data.Length; pos++)
                if (data[pos] != received.Content[pos])
                    Assert.Fail("Binary data returned differs from original");
        }

     
    }
}