using System;
using System.Collections.Generic;
using System.Net;
using Furore.Fhir.Sprinkler.Xunit.ClientUtilities;
using Furore.Fhir.Sprinkler.Xunit.ClientUtilities.FhirClientTestExtensions;
using Furore.Fhir.Sprinkler.Xunit.ClientUtilities.XunitFhirExtensions.Attributes;
using Hl7.Fhir.Model;
using Hl7.Fhir.Rest;
using Xunit;

namespace Furore.Fhir.Sprinkler.Xunit.TestSet
{
    public class ConditionalDeleteTest : IClassFixture<ConditionalDeleteTest.SetupAndTeardownContext>
    {

        private readonly FhirClient client;
        private readonly SetupAndTeardownContext context;

        public ConditionalDeleteTest(SetupAndTeardownContext context)
        {
            this.context = context;
            this.client = FhirClientBuilder.CreateFhirClient();
        }
        public class SetupAndTeardownContext : SetupAndTeardown
        {
            private Patient patient;

            public DateTimeOffset CreationDate { get; private set; }

            public string Location
            {
                get { return patient.GetReferenceId(); }
            }

            public Patient Patient
            {
                get { return patient; }
                set { patient = value; }
            }

            public override void Setup()
            {
                CreationDate = DateTimeOffset.Now;
                patient = FhirClientBuilder.CreateFhirClient().CreateTagged(CreatePatient());
            }
            private Patient CreatePatient(string family = "Adams", params string[] given)
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

        [TestMetadata("CD01", "Conditionally delete a patient that doesn't exist")]
        [Fact]
        public void ConditionalDeleteNonExistentPatient()
        {
            SearchParams parameters =  SearchParams.FromUriParamList(Utils.GetSprinklerTagCriteria(Utils.GenerateRandomSprinklerTag()));
            Assert.Throws<FhirOperationException>(() => client.Delete("Patient", parameters));

            Assert.True(client.LastResult.Status == ((int)HttpStatusCode.NotFound).ToString());
        }

        [TestMetadata("CD02", "Conditionally delete a patient that exists")]
        [Fact]
        public void ConditionalDeleteExistentPatient()
        {
            FhirAssert.SkipWhen(context.Patient == null || context.Patient.Id == null);

            client.Delete("Patient", SearchParams.FromUriParamList(Utils.GetSprinklerTagCriteria(context.Patient)));

            Assert.True(client.LastResult.Status == ((int)HttpStatusCode.NoContent).ToString());
        }
    }
}