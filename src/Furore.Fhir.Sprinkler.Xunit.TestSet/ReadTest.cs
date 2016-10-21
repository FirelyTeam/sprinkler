using System.Collections.Generic;
using Hl7.Fhir.Model;
using Xunit;
using Furore.Fhir.Sprinkler.FhirUtilities.ResourceManagement;

namespace Furore.Fhir.Sprinkler.Xunit.TestSet
{
    ////[FixtureConfiguration(@"d:\projects\furore\sprinkler\src\Furore.Fhir.Sprinkler.TestSet\Resources\", FixtureType.File)]
    //[FixtureConfiguration(@"D:\Projects\Furore\sprinkler\src\Furore.Fhir.Sprinkler.TestSet\Resources\examples.zip", FixtureType.ZipFile)]
    //public class ReadTest : IClassFixture<FhirClientFixture>
    //{
    //    private readonly FhirClientFixture client;

    //    public ReadTest(FhirClientFixture client)
    //    {
    //        this.client = client;
    //    }

    //    public static TheoryData<Patient> NewPatient(string family, params string[] given)
    //    {
    //        var p = new Patient();
    //        var n = new HumanName();
    //        //foreach (string g in given)
    //        //{
    //        //    n.WithGiven(g);
    //        //}

    //        n.AndFamily("aaa");
    //        p.Name = new List<HumanName>();
    //        p.Name.Add(n);
    //        return new TheoryData<Patient>() {p};
    //    }

    //    [Theory]
    //    [MemberData("NewPatient", new object[] {"aaa", new string[] { "bbb", "ccc"}})]
    //    public void CheckResource(Patient patient)
    //    {
    //        client.Client.Create<Patient>(patient);
    //        Assert.HttpOk(client.Client);
    //        Assert.ValidResourceContentTypePresent(client.Client);

    //    }

    //   // [Theory]
    //   // [Code("R01")]
    //   // // [Fixture(fileNames: @"d:\projects\furore\sprinkler\src\Furore.Fhir.Sprinkler.TestSet\Resources\patient-example.xml")]
    //   //// [Fixture(fileNames:"patient-example.xml")]
    //   // [Fixture(fileNames: "patient-example(example).xml")]
    //   // [SpyBeforeAfterTest]
    //   // public void GetTestDataPerson(AutoSetupFixture<Resource> setupPatient)
    //   // {
    //   //     Patient patient = (Patient)setupPatient.Fixture;

    //   //     string id = patient.ResourceIdentity().MakeRelative().ToString();
    //   //     client.Client.Read<Patient>(id);
    //   //     Assert.HttpOk(client.Client);
    //   //     Assert.ValidResourceContentTypePresent(client.Client);

    //   // }
    //}
}
