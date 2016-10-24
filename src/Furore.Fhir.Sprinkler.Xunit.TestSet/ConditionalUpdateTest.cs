using System.Collections.Generic;
using System.Linq;
using System.Net;
using Furore.Fhir.Sprinkler.Xunit.ClientUtilities;
using Furore.Fhir.Sprinkler.Xunit.ClientUtilities.XunitFhirExtensions.Attributes;
using Hl7.Fhir.Model;
using Hl7.Fhir.Rest;
using Xunit;

namespace Furore.Fhir.Sprinkler.Xunit.TestSet
{
    [TestCaseOrderer("Furore.Fhir.Sprinkler.Xunit.ClientUtilities.XunitFhirExtensions.TestCaseOrderers.PriorityOrderer", "Furore.Fhir.Sprinkler.Xunit.ClientUtilities")]
    public class ConditionalUpdateTest : IClassFixture<ConditionalUpdateTest.SetupAndTeardownContext>
    {
        private readonly FhirClient client;
        private readonly SetupAndTeardownContext context;

        public ConditionalUpdateTest(SetupAndTeardownContext context)
        {
            this.context = context;
            this.client = FhirClientBuilder.CreateFhirClient();
        }
        public class SetupAndTeardownContext : SetupAndTeardown
        {
            private Patient patient;

            public string Location
            {
                get { return patient.GetReferenceId(); }
            }

            public Patient Patient
            {
                get { return patient; }
                set { patient = value; }
            }

            public SearchParams TagSearchParams
            {
                get { return SearchParams.FromUriParamList(Utils.GetSprinklerTagCriteria(Patient)); }
            }

            public SetupAndTeardownContext()
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

        [TestMetadata("UC01", "Conditionally PUT a patient that doesn't yet exist")]
        [Fact, TestPriority(1)]
        public void ConditionalPutNonExistentPatient()
        {
            Utils.AddSprinklerTag(context.Patient);

            context.Patient = client.Update(context.Patient, context.TagSearchParams);

            Assert.True(client.LastResult.Status == ((int)HttpStatusCode.Created).ToString());
            Assert.NotNull(context.Patient);
        }

        [TestMetadata("UC02", "Conditionally PUT a patient already exists")]
        [Fact, TestPriority(2)]
        public void ConditionalPutExistentPatient()
        {
            FhirAssert.SkipWhen(context.Patient.Id == null, "Test used for patient creation failled.");
            context.Patient.Name[0].WithGiven("John");

            context.Patient = client.Update(context.Patient, context.TagSearchParams);

            Assert.True(client.LastResult.Status == ((int)HttpStatusCode.OK).ToString());
            Assert.NotNull(context.Patient);
            Assert.True(context.Patient.Name[0].Given.Any(g => g== "John"));
        }

        [TestMetadata("UC03", "Conditionally PUT with ambigous condition")]
        [Fact, TestPriority(3)]
        public void ConditionalPutWithAmbigousCondition()
        {
            FhirAssert.SkipWhen(context.Patient.Id == null, "Test used for patient creation failled.");
            Patient otherPatient = context.CreatePatient();

            Utils.AddSprinklerTag(otherPatient, Utils.GetSprinklerTag(context.Patient));
            client.Create(otherPatient);

            FhirAssert.Fails(client, () => client.Update(context.Patient, context.TagSearchParams), HttpStatusCode.PreconditionFailed);
            Assert.True(client.LastBodyAsResource is OperationOutcome);
        }

    }
}