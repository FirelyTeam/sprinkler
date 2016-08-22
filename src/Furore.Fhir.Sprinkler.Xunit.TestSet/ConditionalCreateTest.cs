using System.Collections.Generic;
using System.Net;
using Furore.Fhir.Sprinkler.Xunit.ClientUtilities;
using Furore.Fhir.Sprinkler.Xunit.ClientUtilities.XunitFhirExtensions.Attributes;
using Hl7.Fhir.Model;
using Hl7.Fhir.Rest;
using Xunit;

namespace Furore.Fhir.Sprinkler.Xunit.TestSet
{
    [TestCaseOrderer("Furore.Fhir.Sprinkler.Xunit.ClientUtilities.XunitFhirExtensions.TestCaseOrderers.PriorityOrderer", "Furore.Fhir.Sprinkler.Xunit.ClientUtilities")]
    public class ConditionalCreateTest : IClassFixture<ConditionalCreateTest.SetupAndTeardownContext>
    {
        private readonly FhirClient client;
        private readonly SetupAndTeardownContext context;

        public ConditionalCreateTest(SetupAndTeardownContext context)
        {
            this.context = context;
            this.client = FhirClientBuilder.CreateFhirClient();
        }
        public class SetupAndTeardownContext : SetupAndTeardown
        {
            private Patient patient;

            public Patient Patient
            {
                get { return patient; }
                set { patient = value; }
            }

            public override void Setup()
            {
                patient = CreatePatient();
            }
            public Patient CreatePatient(string family = "Adams", params string[] given)
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

            public override void Teardown()
            {
                FhirClientBuilder.CreateFhirClient().Delete(patient);
            }
        }


        [TestMetadata("CC01", "Conditionally create a patient that doesn't yet exist")]
        [Fact, TestPriority(1)]
        public void ConditionalCreateNonExistentPatient()
        {
            Utils.AddSprinklerTag(context.Patient);
            client.OnBeforeRequest += (object sender, BeforeRequestEventArgs e) =>
            {
                e.RawRequest.Headers.Add("If-None-Exist", Utils.GetSprinklerTagCriteria(context.Patient).ToQueryString());
            };
            context.Patient = client.Create(context.Patient);

            Assert.True(client.LastResult.Status == ((int) HttpStatusCode.Created).ToString());
            Assert.NotNull(context.Patient);
        }

        [TestMetadata("CC02", "Conditionally create a patient that doesn't yet exist")]
        [Fact, TestPriority(2)]
        public void ConditionalCreateAlreadyExistentPatient()
        {
            FhirAssert.SkipWhen(context.Patient == null || context.Patient.Id == null);
            string searchCriteria = Utils.GetSprinklerTagCriteria(context.Patient).ToQueryString();
            client.OnBeforeRequest += (object sender, BeforeRequestEventArgs e) =>
            {
                e.RawRequest.Headers.Add("If-None-Exist", searchCriteria);
            };
            Patient createdPatient = client.Create(context.Patient);

            Assert.True(client.LastResult.Status == ((int)HttpStatusCode.OK).ToString());
            Assert.True(createdPatient.VersionId == context.Patient.VersionId);
        }

        [TestMetadata("CC03", "Conditionally create a patient with prefer representation")]
        [Fact, TestPriority(3)]
        public void ConditionalCreateAlreadyExistentPatientWithPreferRepresentation()
        {
            FhirAssert.SkipWhen(context.Patient == null || context.Patient.Id == null);
            string searchCriteria = Utils.GetSprinklerTagCriteria(context.Patient).ToQueryString();
            client.ReturnFullResource = false;
            client.OnBeforeRequest += (object sender, BeforeRequestEventArgs e) =>
            {
                e.RawRequest.Headers.Add("If-None-Exist", searchCriteria);
            };
            Patient createdPatient = client.Create(context.Patient);

            Assert.True(client.LastResult.Status == ((int)HttpStatusCode.OK).ToString());
            Assert.Null(createdPatient);
            //assert location
        }

        [TestMetadata("CC04", "Conditionally create a patient with ambigous search criteria")]
        [Fact, TestPriority(4)]
        public void ConditionalCreateWithAmbigousCondition()
        {
            FhirAssert.SkipWhen(context.Patient == null || context.Patient.Id == null);

            Patient otherPatient = context.CreatePatient();
            Utils.AddSprinklerTag(otherPatient, Utils.GetSprinklerTag(context.Patient));
            client.Create(otherPatient);


            string searchCriteria = Utils.GetSprinklerTagCriteria(context.Patient).ToQueryString();
            client.OnBeforeRequest += (object sender, BeforeRequestEventArgs e) =>
            {
                e.RawRequest.Headers.Add("If-None-Exist", searchCriteria);
            };

            context.Patient.Id = null;
            context.Patient.Meta.VersionId = null;
            FhirAssert.Fails(client, () => client.Create(context.Patient), HttpStatusCode.PreconditionFailed);
            Assert.True(client.LastBodyAsResource is OperationOutcome);
        }

      
    }
}