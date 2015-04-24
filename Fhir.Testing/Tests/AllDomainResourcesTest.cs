/* 
 * Copyright (c) 2014, Furore (info@furore.com) and contributors
 * See the file CONTRIBUTORS for details.
 * 
 * This file is licensed under the BSD 3-Clause license
 * available at https://raw.github.com/furore-fhir/sprinkler/master/LICENSE
 */

using Hl7.Fhir.Model;
using Hl7.Fhir.Rest;
using Hl7.Fhir.Serialization;
using Sprinkler.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

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
            catch(Exception e)
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
            catch(Exception e)
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
            catch(Exception e)
            {
                errors.Add("Cannot read " + resource.GetType().Name + ": " + e.Message);
            }           
        }      

        public void AttemptResource<T>(DomainResource resource) where T: DomainResource, new()
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
                catch(Exception e)
                {
                    errors.Add("Creation of " + resource.GetType().Name + " failed: " + e.Message);                    
                }
            }
        }
     
        private void TestSomeResource<T>() where T: DomainResource, new()
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
            TestSomeResource<MedicationPrescription>();
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

        [SprinklerTest("AR46", "Create read update delete on Supply")]
        public void TestSupply()
        {
            TestSomeResource<Supply>();
        }

        [SprinklerTest("AR47", "Create read update delete on Value Set")]
        public void TestValueSet()
        {
            TestSomeResource<ValueSet>();
        }
    }
}
