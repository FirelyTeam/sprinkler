using System.Collections.Generic;
using Furore.Fhir.Sprinkler.XunitRunner.FhirExtensions;
using Hl7.Fhir.Model;
using Hl7.Fhir.Rest;
using Xunit;
using Furore.Fhir.Sprinkler.ClientUtilities;
using Assert = Furore.Fhir.Sprinkler.ClientUtilities.Assert;

namespace Furore.Fhir.Sprinkler.Xunit.TestSet
{
    public class ReadTest : IClassFixture<FhirClientFixture>
    {
        private FhirClient Client;

        public ReadTest(FhirClientFixture client)
        {
            Client = client.Client;
        }

        public static Patient NewPatient(string family, params string[] given)
        {
            var p = new Patient();
            var n = new HumanName();
            foreach (string g in given)
            {
                n.WithGiven(g);
            }

            n.AndFamily(family);
            p.Name = new List<HumanName>();
            p.Name.Add(n);
            return p;
        }

        [Fact]
        [Code("R01")]
        public void GetTestDataPerson()
        {
            Patient p = NewPatient("Emerald", "Caro");
            Patient entry = Client.Create(p);
            string id = entry.ResourceIdentity().MakeRelative().ToString();

            Client.Read<Patient>(id);

            Assert.HttpOk(Client);

            Assert.ValidResourceContentTypePresent(Client);
        }
    }
}
