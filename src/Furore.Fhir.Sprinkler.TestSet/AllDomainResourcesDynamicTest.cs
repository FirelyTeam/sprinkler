using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Furore.Fhir.Sprinkler.FhirUtilities;
using Furore.Fhir.Sprinkler.Framework.Framework;
using Furore.Fhir.Sprinkler.Framework.Framework.Attributes;
using Furore.Fhir.Sprinkler.Framework.Utilities;
using Hl7.Fhir.Model;

namespace Furore.Fhir.Sprinkler.TestSet
{
   // [SprinklerDynamicModule(typeof(AllDomainResourcesTestGenerator))]
    //[SprinklerModule("All DomainResources Test")]
    public class AllDomainResourcesDynamicTest : SprinklerTestClass
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
                        //TryToUpdate<T>(resource, key);
                        //AndTryDelete<T>(resource, key);
                    }
                }
                catch (Exception e)
                {
                    errors.Add("Creation of " + resource.GetType().Name + " failed: " + e.Message);
                }
            }
        }

        [SprinklerTest("ADR", "Create read update delete on Device")]
        [SprinklerDynamicTest("ADR{0}", "Create read update delete on {0}")]
        public void TestSomeResource<T>() where T : DomainResource, new()
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
 
    }
}