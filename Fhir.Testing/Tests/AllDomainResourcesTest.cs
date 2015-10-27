/* 
 * Copyright (c) 2014, Furore (info@furore.com) and contributors
 * See the file CONTRIBUTORS for details.
 * 
 * This file is licensed under the BSD 3-Clause license
 * available at https://raw.github.com/furore-fhir/sprinkler/master/LICENSE
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Hl7.Fhir.Model;
using Sprinkler.Framework;

namespace Sprinkler.Tests
{
    [SprinklerModule("All DomainResources Test")]
    public class AllDomainResourcesTest : SprinklerTestClass
    {
        List<DomainResource> resources = DemoData.GetListofResources();
        int tempid = 0;
        List<String> errors = new List<string>();
      
        private void TryToUpdate<T>(DomainResource resource, string location) where T : DomainResource, new()
        {
            DomainResource res = Client.Read<T>(location);
            Element element = new Code("unsure");
            try
            {
                res.AddExtension("http://fhir.furore.com/extensions/sprinkler", element);
                Client.Update(res);
            }
            catch (Exception e)
            {
                errors.Add("Update of " + resource.GetType().Name + " failed: " + e.Message);
            }
        }

        public void AndTryDelete<T>(DomainResource resource, string location) where T : DomainResource, new()
        {
            try
            {
                Client.Delete(location);
                Assert.Fails(Client, () => Client.Read<T>(location), HttpStatusCode.Gone);
            }
            catch (Exception e)
            {
                errors.Add("Deletion of " + resource.GetType().Name + " failed: " + e.Message);
            }
        }

        private void TryToRead<T>(DomainResource resource, string location) where T : DomainResource, new()
        {
            try
            {
                Client.Read<T>(location);
            }
            catch (Exception e)
            {
                errors.Add("Cannot read " + resource.GetType().Name + ": " + e.Message);
            }
        }

        public void AttemptResource<T>(DomainResource resource) where T : DomainResource, new()
        {
            string key = null;

            if (typeof(T) == resource.GetType())
            {
                DomainResource created = null;
                try
                {
                    created = Client.Create((T)resource);
                    key = created.ResourceIdentity().WithoutVersion().MakeRelative().ToString();
                    if (key != null)
                    {
                        TryToRead<T>(resource, key);
                        TryToUpdate<T>(resource, key);
                        AndTryDelete<T>(resource, key);
                    }
                }
                catch (Exception e)
                {
                    errors.Add("Creation of " + resource.GetType().Name + " failed: " + e.Message);
                }
            }
        }

        private void TestSomeResource<T>() where T : DomainResource, new()
        {
            errors.Clear();
            string id = "sprink" + tempid++.ToString();
            Type type = typeof(T);

            DomainResource resource = GetFirstResourceOfType(type);
            if (resource != null)
            {
                T typedresource = (T)resource;
                AttemptResource<T>(typedresource);

                if (errors.Count() != 0)
                {
                    string errormessage = "";
                    foreach (string s in errors)
                    {
                        errormessage = errormessage + s + "\r\n";
                    }
                    Assert.Fail(errormessage);
                }
            }
            else
            {
                errors.Add("No test data for resource of type " + type.Name);
            }
        }

        private DomainResource GetFirstResourceOfType(Type type)
        {
            //string id = "sprink" + tempid++.ToString();
            IEnumerable<DomainResource> resource =
                from r in resources
                where r.GetType() == type
                select r;

            return resource.FirstOrDefault();
        }

        //[SprinklerTest("AR01", "Create read update delete on Adverse Reaction")]
        //public void TestAdversereaction()
        //{
        //    TestSomeResource<AdverseReaction>();
        //}

        //[SprinklerTest("AR02", "Create read update delete on Alert")]
        //public void TestAlert()
        //{
        //    TestSomeResource<Alert>();
        //}

        [SprinklerTest("AR03", "Create read update delete on Allergy Intolerance")]
        public void TestAllergyIntolerancet()
        {
            TestSomeResource<AllergyIntolerance>();
        }

        [SprinklerTest("AR04", "Create read update delete on Care Plan")]
        public void TestCarePlan()
        {
            TestSomeResource<CarePlan>();
        }

        [SprinklerTest("AR05", "Create read update delete on Composition")]
        public void TestCompositiont()
        {
            TestSomeResource<Composition>();
        }

        [SprinklerTest("AR06", "Create read update delete on Concept Map")]
        public void TestConceptMap()
        {
            TestSomeResource<ConceptMap>();
        }

        [SprinklerTest("AR07", "Create read update delete on Condition")]
        public void TestCondition()
        {
            TestSomeResource<Condition>();
        }

        [SprinklerTest("AR08", "Create read update delete on Conformance")]
        public void TestConformance()
        {
            TestSomeResource<Conformance>();
        }

        [SprinklerTest("AR09", "Create read update delete on Device")]
        public void TestDevice()
        {
            TestSomeResource<Device>();
        }

        //[SprinklerTest("AR10", "Create read update delete on Device Observation Report")]
        //public void TestDeviceObservationReport()
        //{
        //    TestSomeResource<DeviceObservationReport>();
        //}

        [SprinklerTest("AR11", "Create read update delete on Diagnostic Order")]
        public void TestDiagnosticOrdert()
        {
            TestSomeResource<DiagnosticOrder>();
        }

        [SprinklerTest("AR12", "Create read update delete on Diagnostic Report")]
        public void TestDiagnosticReport()
        {
            TestSomeResource<DiagnosticReport>();
        }

        [SprinklerTest("AR13", "Create read update delete on Document Manifest")]
        public void TestDocumentManifest()
        {
            TestSomeResource<DocumentManifest>();
        }

        [SprinklerTest("AR14", "Create read update delete on Document Reference")]
        public void TestDocumentReference()
        {
            TestSomeResource<DocumentReference>();
        }

        [SprinklerTest("AR15", "Create read update delete on Encounter")]
        public void TestEncounter()
        {
            TestSomeResource<Encounter>();
        }

        [SprinklerTest("AR16", "Create read update delete on Family History")]
        public void TestFamilyHistory()
        {

            TestSomeResource<FamilyMemberHistory>();
        }

        [SprinklerTest("AR17", "Create read update delete on Group")]
        public void TestGroup()
        {
            TestSomeResource<Group>();
        }

        [SprinklerTest("AR18", "Create read update delete on Imaging Study")]
        public void TestImagingStudy()
        {
            TestSomeResource<ImagingStudy>();
        }

        [SprinklerTest("AR19", "Create read update delete on Immunization")]
        public void TestImmunization()
        {
            TestSomeResource<Immunization>();
        }

        [SprinklerTest("AR20", "Create read update delete on Immunization Recommendation")]
        public void TestImmunizationRecommendation()
        {
            TestSomeResource<ImmunizationRecommendation>();
        }

        [SprinklerTest("AR21", "Create read update delete on List")]
        public void TestList()
        {
            TestSomeResource<List>();
        }

        [SprinklerTest("AR22", "Create read update delete on Location")]
        public void TestLocation()
        {
            TestSomeResource<Location>();
        }

        [SprinklerTest("AR23", "Create read update delete on Media")]
        public void TestMedia()
        {
            TestSomeResource<Media>();
        }

        [SprinklerTest("AR24", "Create read update delete on Medication")]
        public void TestMedication()
        {
            TestSomeResource<Medication>();
        }

        [SprinklerTest("AR25", "Create read update delete on Medication Administration")]
        public void TestMedicationAdministration()
        {
            TestSomeResource<MedicationAdministration>();
        }

        [SprinklerTest("AR26", "Create read update delete on Medication Dispense")]
        public void TestMedicationDispense()
        {
            TestSomeResource<MedicationDispense>();
        }

        [SprinklerTest("AR27", "Create read update delete on Medication Prescription")]
        public void TestMedicationPrescription()
        {
            TestSomeResource<MedicationOrder>();
        }

        [SprinklerTest("AR28", "Create read update delete on Medication Statement")]
        public void TestMedicationStatement()
        {
            TestSomeResource<MedicationStatement>();
        }

        [SprinklerTest("AR29", "Create read update delete on Message Header")]
        public void TestMessageHeader()
        {
            TestSomeResource<MessageHeader>();
        }

        [SprinklerTest("AR30", "Create read update delete on Observation")]
        public void TestObservation()
        {
            TestSomeResource<Observation>();
        }

        [SprinklerTest("AR31", "Create read update delete on Operation Outcome")]
        public void TestOperationOutcome()
        {
            TestSomeResource<OperationOutcome>();
        }

        [SprinklerTest("AR32", "Create read update delete on Order")]
        public void TestOrder()
        {
            TestSomeResource<Order>();
        }

        [SprinklerTest("AR33", "Create read update delete on Order Response")]
        public void TestOrderResponse()
        {
            TestSomeResource<OrderResponse>();
        }

        [SprinklerTest("AR34", "Create read update delete on Organization")]
        public void TestOrganization()
        {
            TestSomeResource<Organization>();
        }

        //[SprinklerTest("AR35", "Create read update delete on Other")]
        //public void TestOther()
        //{
        //    TestSomeResource<Base>();
        //}

        [SprinklerTest("AR36", "Create read update delete on Patient")]
        public void TestPatient()
        {
            TestSomeResource<Patient>();
        }

        [SprinklerTest("AR37", "Create read update delete on Practitioner")]
        public void TestPractitioner()
        {
            TestSomeResource<Practitioner>();
        }

        [SprinklerTest("AR38", "Create read update delete on Procedure")]
        public void TestProcedure()
        {
            TestSomeResource<Procedure>();
        }

        //[SprinklerTest("AR39", "Create read update delete on Profile")]
        //public void TestProfile()
        //{
        //    TestSomeResource<Profile>();
        //}

        [SprinklerTest("AR40", "Create read update delete on Provenance")]
        public void TestProvenance()
        {
            TestSomeResource<Provenance>();
        }

        //[SprinklerTest("AR41", "Create read update delete on Query")]
        //public void TestQuery()
        //{
        //    TestSomeResource<Query>();
        //}

        [SprinklerTest("AR42", "Create read update delete on Questionnaire")]
        public void TestQuestionnaire()
        {
            TestSomeResource<Questionnaire>();
        }

        [SprinklerTest("AR43", "Create read update delete on Related Person")]
        public void TestRelatedPerson()
        {
            TestSomeResource<RelatedPerson>();
        }

        //[SprinklerTest("AR44", "Create read update delete on Security Event")]
        //public void TestSecurityEvent()
        //{
        //    TestSomeResource<SecurityEvent>();
        //}

        [SprinklerTest("AR45", "Create read update delete on Specimen")]
        public void TestSpecimen()
        {
            TestSomeResource<Specimen>();
        }

        [SprinklerTest("AR46", "Create read update delete on SupplyRequest")]
        public void TestSupplyRequest()
        {
            TestSomeResource<SupplyRequest>();
        }

        [SprinklerTest("AR46", "Create read update delete on SupplyDelivery")]
        public void TestSupplyDelivery()
        {
            TestSomeResource<SupplyDelivery>();
        }

        [SprinklerTest("AR47", "Create read update delete on Value Set")]
        public void TestValueSet()
        {
            TestSomeResource<ValueSet>();
        }

        [SprinklerTest("AR48", "Create read update delete on Account")]
        public void TestAccount()
        {
            TestSomeResource<Account>();
        }

        [SprinklerTest("AR49", "Create read update delete on DetectedIssue")]
        public void TestDetectedIssue()
        {
            TestSomeResource<DetectedIssue>();
        }


        [SprinklerTest("AR50", "Create read update delete on ImplementationGuide")]
        public void TestImplementationGuide()
        {
            TestSomeResource<ImplementationGuide>();
        }

        [SprinklerTest("AR51", "Create read update delete on QuestionnaireResponse")]
        public void TestQuestionnaireResponse()
        {
            TestSomeResource<QuestionnaireResponse>();
        }

        [SprinklerTest("AR52", "Create read update delete on TestScript")]
        public void TestTestScript()
        {
            TestSomeResource<TestScript>();
        }

        [SprinklerTest("AR53", "Create read update delete on StructureDefinition")]
        public void TestStructureDefinition()
        {
            TestSomeResource<StructureDefinition>();
        }

        [SprinklerTest("AR54", "Create read update delete on AppointmentResponse")]
        public void TestAppointmentResponse()
        {
            TestSomeResource<AppointmentResponse>();
        }

        [SprinklerTest("AR55", "Create read update delete on Appointment")]
        public void TestAppointment()
        {
            TestSomeResource<Appointment>();
        }

        [SprinklerTest("AR56", "Create read update delete on AuditEvent")]
        public void TestAuditEvent()
        {
            TestSomeResource<AuditEvent>();
        }

        [SprinklerTest("AR57", "Create read update delete on Basic")]
        public void TestBasic()
        {
            TestSomeResource<Basic>();
        }

        [SprinklerTest("AR58", "Create read update delete on BodySite")]
        public void TestBodySite()
        {
            TestSomeResource<BodySite>();
        }

        [SprinklerTest("AR59", "Create read update delete on Claim")]
        public void TestClaim()
        {
            TestSomeResource<Claim>();
        }

        [SprinklerTest("AR60", "Create read update delete on ClaimResponse")]
        public void TestClaimResponse()
        {
            TestSomeResource<ClaimResponse>();
        }

        [SprinklerTest("AR61", "Create read update delete on ClinicalImpression")]
        public void TestClinicalImpression()
        {
            TestSomeResource<ClinicalImpression>();
        }

        [SprinklerTest("AR62", "Create read update delete on Communication")]
        public void TestCommunication()
        {
            TestSomeResource<Communication>();
        }

        [SprinklerTest("AR63", "Create read update delete on CommunicationRequest")]
        public void TestCommunicationRequest()
        {
            TestSomeResource<CommunicationRequest>();
        }

        [SprinklerTest("AR64", "Create read update delete on Contract")]
        public void TestContract()
        {
            TestSomeResource<Contract>();
        }

        [SprinklerTest("AR65", "Create read update delete on Coverage")]
        public void TestCoverage()
        {
            TestSomeResource<Coverage>();
        }

        [SprinklerTest("AR66", "Create read update delete on DataElement")]
        public void TestDataElement()
        {
            TestSomeResource<DataElement>();
        }

        [SprinklerTest("AR67", "Create read update delete on DeviceComponent")]
        public void TestDeviceComponent()
        {
            TestSomeResource<DeviceComponent>();
        }

        [SprinklerTest("AR68", "Create read update delete on DeviceMetric")]
        public void TestDeviceMetric()
        {
            TestSomeResource<DeviceMetric>();
        }

        [SprinklerTest("AR69", "Create read update delete on DeviceUseRequest")]
        public void TestDeviceUseRequest()
        {
            TestSomeResource<DeviceUseRequest>();
        }

        [SprinklerTest("AR70", "Create read update delete on DeviceUseStatement")]
        public void TestCDeviceUseStatement()
        {
            TestSomeResource<DeviceUseStatement>();
        }

        [SprinklerTest("AR71", "Create read update delete on EligibilityRequest")]
        public void TestEligibilityRequest()
        {
            TestSomeResource<EligibilityRequest>();
        }

        [SprinklerTest("AR72", "Create read update delete on EligibilityResponse")]
        public void TestEligibilityResponse()
        {
            TestSomeResource<EligibilityResponse>();
        }


        [SprinklerTest("AR73", "Create read update delete on EnrollmentRequest")]
        public void TestEnrollmentRequest()
        {
            TestSomeResource<EnrollmentRequest>();
        }


        [SprinklerTest("AR74", "Create read update delete on EnrollmentResponse")]
        public void TestEnrollmentResponse()
        {
            TestSomeResource<EnrollmentResponse>();
        }


        [SprinklerTest("AR75", "Create read update delete on EpisodeOfCare")]
        public void TestEpisodeOfCare()
        {
            TestSomeResource<EpisodeOfCare>();
        }


        [SprinklerTest("AR76", "Create read update delete on ExplanationOfBenefit")]
        public void TestExplanationOfBenefit()
        {
            TestSomeResource<ExplanationOfBenefit>();
        }


        [SprinklerTest("AR77", "Create read update delete on Flag")]
        public void TestFlag()
        {
            TestSomeResource<Flag>();
        }


        [SprinklerTest("AR78", "Create read update delete on Goal")]
        public void TestGoal()
        {
            TestSomeResource<Goal>();
        }


        [SprinklerTest("AR79", "Create read update delete on HealthcareService")]
        public void TestHealthcareService()
        {
            TestSomeResource<HealthcareService>();
        }


        [SprinklerTest("AR80", "Create read update delete on ImagingObjectSelection")]
        public void TestImagingObjectSelection()
        {
            TestSomeResource<ImagingObjectSelection>();
        }


        [SprinklerTest("AR81", "Create read update delete on NamingSystem")]
        public void TestNamingSystem()
        {
            TestSomeResource<NamingSystem>();
        }


        [SprinklerTest("AR82", "Create read update delete on NutritionOrder")]
        public void TestNutritionOrder()
        {
            TestSomeResource<NutritionOrder>();
        }


        [SprinklerTest("AR83", "Create read update delete on OperationDefinition")]
        public void TestOperationDefinition()
        {
            TestSomeResource<OperationDefinition>();
        }


        [SprinklerTest("AR84", "Create read update delete on PaymentNotice")]
        public void TestPaymentNotice()
        {
            TestSomeResource<PaymentNotice>();
        }


        [SprinklerTest("AR85", "Create read update delete on PaymentReconciliation")]
        public void TestPaymentReconciliation()
        {
            TestSomeResource<PaymentReconciliation>();
        }


        [SprinklerTest("AR86", "Create read update delete on Person")]
        public void TestPerson()
        {
            TestSomeResource<Person>();
        }


        [SprinklerTest("AR87", "Create read update delete on ProcedureRequest")]
        public void TestProcedureRequest()
        {
            TestSomeResource<ProcedureRequest>();
        }


        [SprinklerTest("AR88", "Create read update delete on ProcessRequest")]
        public void TestProcessRequest()
        {
            TestSomeResource<ProcessRequest>();
        }


        [SprinklerTest("AR89", "Create read update delete on ProcessResponse")]
        public void TestProcessResponse()
        {
            TestSomeResource<ProcessResponse>();
        }


        [SprinklerTest("AR90", "Create read update delete on ReferralRequest")]
        public void TestReferralRequest()
        {
            TestSomeResource<ReferralRequest>();
        }


        [SprinklerTest("AR91", "Create read update delete on RiskAssessment")]
        public void TestRiskAssessment()
        {
            TestSomeResource<RiskAssessment>();
        }

        [SprinklerTest("AR92", "Create read update delete on Schedule")]
        public void TestSchedule()
        {
            TestSomeResource<Schedule>();
        }


        [SprinklerTest("AR93", "Create read update delete on SearchParameter")]
        public void TestSearchParameter()
        {
            TestSomeResource<SearchParameter>();
        }


        [SprinklerTest("AR94", "Create read update delete on Slot")]
        public void TestSlot()
        {
            TestSomeResource<Slot>();
        }


        [SprinklerTest("AR95", "Create read update delete on Subscription")]
        public void TestSubscription()
        {
            TestSomeResource<Subscription>();
        }

        [SprinklerTest("AR96", "Create read update delete on SupplyDelivery")]
        public void TestSubstance()
        {
            TestSomeResource<Substance>();
        }

        [SprinklerTest("AR97", "Create read update delete on VisionPrescription")]
        public void TestVisionPrescription()
        {
            TestSomeResource<VisionPrescription>();
        }
    }
}
