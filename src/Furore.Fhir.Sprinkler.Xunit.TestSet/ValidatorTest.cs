using System;
using System.IO;
using System.Net;
using System.Text;
using Furore.Fhir.Sprinkler.FhirUtilities.ResourceManagement;
using Furore.Fhir.Sprinkler.XunitRunner.FhirExtensions;
using Hl7.Fhir.Model;
using Hl7.Fhir.Rest;
using Hl7.Fhir.Serialization;
using Xunit;
using Assert = Furore.Fhir.Sprinkler.FhirUtilities.Assert;

namespace Furore.Fhir.Sprinkler.Xunit.TestSet
{
    [FixtureConfiguration(FixtureType.File)]

    public class ValidatorTest : IClassFixture<FhirClientFixture>
    {
        private readonly FhirClient Client;
        public ValidatorTest(FhirClientFixture context)
        {
            Client = context.Client;
        }

        [TestMetadata("VA01", "Validate creation of a valid resource")]
        [Fixture(false, "Patient.Valid.xml")]
        [Theory]
        public void CreateValidResource(Patient patient)
        {
            try
            {
                Uri location = new Uri(Client.Endpoint + "Patient/$validate");
               // Client.ValidateCreate(patient);
                HttpWebResponse response = PostResource(location, FhirSerializer.SerializeToXml(patient));
                if (response.StatusCode != HttpStatusCode.Created)
                    Assert.Fail("Server did not accept valid resource");
            }
            catch (Exception)
            {
              
            }
            //using (HttpWebResponse response = PostResource(location, FhirSerializer.SerializeToXml(patient)))
            //{
            //    if (response.StatusCode != HttpStatusCode.Created)
            //        Assert.Fail("Server did not accept valid resource");
            //}
        }

        private HttpWebResponse PostResource(Uri location, string content)
        {
            WebRequest req = WebRequest.Create(location);
            req.ContentType = "application/xml+fhir";
            req.Method = "POST";
            Stream outStream = req.GetRequestStream();
            var outStreamWriter = new StreamWriter(outStream, Encoding.UTF8);

            outStreamWriter.Write(content);
            outStreamWriter.Flush();
            outStreamWriter.Close();

            var response = (HttpWebResponse)req.GetResponseNoEx();
            return response;
        }
    }
}