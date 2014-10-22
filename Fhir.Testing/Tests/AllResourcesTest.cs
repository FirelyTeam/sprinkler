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
    [SprinklerModule("All Resources Test")]
    public class AllResourcesTest : SprinklerTestClass
    {
        FhirClient client = new FhirClient("http://spark.furore.com/fhir");
        List<Resource> resources = DemoData.GetListofResources();
        int tempid = 0;
        List<String> errors = new List<string>();

        //public void NowTryToUpdate<T>(Resource resource)
        //{
        //    if (resource.GetType() == typeof(Patient))
        //    {
        //        Patient p = (Patient)resource;
        //        p.Name == ...
        //    }
        //}

        public void AndTryDelete<T>(Resource resource, string  id) where T : Resource, new()
        {
            string location = resource.GetType().Name + "/" + id;
            try
            {
                client.Delete(location);
                Assert.Fails(Client, () => Client.Read<T>(location), HttpStatusCode.Gone);
            }
            catch(Exception e)
            {
                errors.Add("Deletion of " + resource.GetType().Name + " failed: " + e.Message);
            }           
        }

        private void TryToRead<T>(Resource resource, string id) where T : Resource, new()
        {
            string location = resource.GetType().Name + "/" + id;
            try
            {
                client.Read<T>(location);
            }
            catch(Exception e)
            {
                errors.Add("Cannot read " + resource.GetType().Name + ": " + e.Message);
            }           
        }      

        public void AttemptResource<T>(Resource resource, string id) where T: Resource, new()
        {
            if (typeof(T) == resource.GetType())
            {
                
                
                ResourceEntry<T> created = null;
                
                try
                {
                     created = client.Create((T)resource, id);
                     Uri createdid = created.Id;
                     CheckForCreation(createdid, id);                     
                }
                catch(Exception e)
                {
                    errors.Add("Creation of " + resource.GetType().Name + " failed: " + e.Message);                    
                }
                TryToRead<T>(resource, id);
                AndTryDelete<T>(resource, id);
                
            }
        }

       


        [SprinklerTest("A001", "create all resources")]
        public void CreateResources()
        {            
            foreach(Resource r in resources)
            {
                Type type = r.GetType();
                string id = "sprink" + tempid++.ToString();
                AttemptResource<AdverseReaction>(r, id);
                AttemptResource<Alert>(r, id);
                AttemptResource<AllergyIntolerance>(r, id);
                AttemptResource<CarePlan>(r, id);
                AttemptResource<Composition>(r, id);
                AttemptResource<ConceptMap>(r, id);
                AttemptResource<Condition>(r, id);
                AttemptResource<Conformance>(r, id);
                AttemptResource<Device>(r, id);
                AttemptResource<DeviceObservationReport>(r, id);
                AttemptResource<DiagnosticOrder>(r, id);
                AttemptResource<DiagnosticReport>(r, id);
                AttemptResource<DocumentManifest>(r, id);
                AttemptResource<Encounter>(r, id);
                AttemptResource<FamilyHistory>(r, id);
                AttemptResource<Hl7.Fhir.Model.Group>(r, id);
                AttemptResource<ImagingStudy>(r, id);
                AttemptResource<Immunization>(r, id);
                AttemptResource<List>(r, id);
                AttemptResource<Location>(r, id);
                AttemptResource<Media>(r, id);
                AttemptResource<Medication>(r, id);
                AttemptResource<MedicationAdministration>(r, id);
                AttemptResource<MedicationPrescription>(r, id);
                AttemptResource<MedicationStatement>(r, id);
                AttemptResource<MessageHeader>(r, id);
                AttemptResource<Observation>(r, id);
                AttemptResource<OperationOutcome>(r, id);
                AttemptResource<Order>(r, id);
                AttemptResource<OrderResponse>(r, id);
                AttemptResource<Organization>(r, id);
                AttemptResource<Other>(r, id);
                AttemptResource<Patient>(r, id);
                AttemptResource<Practitioner>(r, id);
                AttemptResource<Profile>(r, id);
                AttemptResource<Procedure>(r, id);
                AttemptResource<Provenance>(r, id);
                AttemptResource<Query>(r, id);
                AttemptResource<Questionnaire>(r, id);
                AttemptResource<RelatedPerson>(r, id);
                AttemptResource<SecurityEvent>(r, id);
                AttemptResource<Specimen>(r, id);
                AttemptResource<Supply>(r, id);       
            }

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

        private void CheckForCreation(Uri created, string id)
        {
            var ep = new RestUrl(client.Endpoint);

            if (!ep.IsEndpointFor(created))            
                Assert.Fail("Location of created resource is not located within server endpoint");       

            var rl = new ResourceIdentity(created);
            if (rl.Id != id)
                Assert.Fail("Server refused to honor client-assigned id");
        }
    }
}
