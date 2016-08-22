using System;
using System.Collections.Generic;
using System.Linq;
using Furore.Fhir.Sprinkler.Xunit.ClientUtilities;
using Furore.Fhir.Sprinkler.Xunit.ClientUtilities.XunitFhirExtensions.Attributes;
using Furore.Fhir.Sprinkler.Xunit.ClientUtilities.XunitFhirExtensions.ClassFixtures;
using Furore.Fhir.Sprinkler.XunitRunner.FhirExtensions;
using Hl7.Fhir.Model;
using Hl7.Fhir.Rest;
using Xunit;

namespace Furore.Fhir.Sprinkler.Xunit.TestSet
{
    public class RequestFormatTestContext : DisposableTestDependencyContext<Patient>
    {
        private readonly IList<string> versions = new List<string>();

        public IEnumerable<string> PatientVersions
        {
            get { return versions.ToList(); }
        }

        public Patient Patient
        {
            get { return Dependency; }
        }

        public DateTimeOffset CreationDate { get; private set; }

        public RequestFormatTestContext()
        {
            var patient = new Patient();
            patient.Name.Add(HumanName.ForFamily("Bach").WithGiven("Johan").WithGiven("Sebastian"));
            Client.ReturnFullResource = true;
            Dependency = Client.Create(patient);
        }
    }
    public class RequestFormatTest : IClassFixture<RequestFormatTestContext>
    {
        private readonly FhirClient Client;
        private readonly RequestFormatTestContext context;

        public RequestFormatTest(RequestFormatTestContext context)
        {
            this.Client = context.Client;
            this.context = context;
        }

        [TestMetadata("CT01", "request xml using accept")]
        [Fact]
        public void XmlAccept()
        {
            Client.PreferredFormat = ResourceFormat.Xml;
            Client.UseFormatParam = false;
            Client.Read<Patient>(context.Location);
            FhirAssert.ResourceResponseConformsTo(Client, Client.PreferredFormat);
        }

        [TestMetadata("CT02", "request xml using _format")]
        [Fact]
        public void XmlFormat()
        {
            Client.PreferredFormat = ResourceFormat.Xml;
            Client.UseFormatParam = true;
            Client.Read<Patient>(context.Location);
            FhirAssert.ResourceResponseConformsTo(Client, Client.PreferredFormat);
        }

        [TestMetadata("CT03", "request json using accept")]
        [Fact]
        public void JsonAccept()
        {
            Client.PreferredFormat = ResourceFormat.Json;
            Client.UseFormatParam = false;
            Client.Read<Patient>(context.Location);
            FhirAssert.ResourceResponseConformsTo(Client, Client.PreferredFormat);
        }

        [TestMetadata("CT04", "request json using _format")]
        [Fact]
        public void JsonFormat()
        {
            Client.PreferredFormat = ResourceFormat.Json;
            Client.UseFormatParam = true;
            Client.Read<Patient>(context.Location);
            FhirAssert.ResourceResponseConformsTo(Client, Client.PreferredFormat);
        }

    }
}