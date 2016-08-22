using System;
using System.Collections.Generic;
using System.Linq;
using Hl7.Fhir.Model;
using Hl7.Fhir.Rest;

namespace Furore.Fhir.Sprinkler.Xunit.ClientUtilities
{
    public static class Utils
    {
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

        public static void AddContact(this Patient patient, HumanName name, Address address)
        {
            patient.Contact = new List<Patient.ContactComponent>();
            var contact = new Patient.ContactComponent();
            contact.Name = name;
            contact.Address = address;
            patient.Contact.Add(contact);
        }

        public static void AddAddress(this Patient patient, string family, string given, string country, string state,
            string city, params string[] lines)
        {
            var name = new HumanName
            {
                Family = new List<string> { family },
                Given = new List<string> { given }
            };
            var address = new Address
            {
                Country = country,
                State = state,
                City = city,
                Line = (lines == null) ? null : lines.ToList()
            };
            patient.AddContact(name, address);
        }

        public static bool HasGiven(this Patient patient, string given)
        {
            return patient.Name.Exists(n => n.Given.Contains(given));
        }

        public static IEnumerable<T> ResourcesOf<T>(this Bundle bundle) where T : Resource, new()
        {
            return bundle.Entry.ByResourceType<T>();
        }

        public static bool IsDeleted(this Bundle.EntryComponent entry)
        {
            if (entry.Request == null) return false;
            return (entry.Request.Method == Bundle.HTTPVerb.DELETE);
        }

        public static IEnumerable<Bundle.EntryComponent> GetDeleted(this IEnumerable<Bundle.EntryComponent> entries)
        {
            //return entries.Where(entry => entry.IsDeleted());
            return entries.Where(IsDeleted);

        }

        public static bool ContainsResource(this Bundle bundle, string id)
        {
            var entries = from ent in bundle.Entry
                          where ent.Resource.Id == id
                          select ent;

            return entries.Count() > 0;

        }

        public static Patient GetNewPatient(string family = "Adams", params string[] given)
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

        public static Resource AddSprinklerTag(Resource resource, Guid? tag = null)
        {
            tag = tag ?? Guid.NewGuid();
            return AddSprinklerTag(resource, tag.ToString());
         
        }

        public static Resource AddSprinklerTag(Resource resource, string tag)
        {
            if (resource.Meta == null)
            {
                resource.Meta = new Meta();
            }

            if (resource.Meta.Tag.All(c => c.System != @"http://example.org/sprinkler"))
            {
                resource.Meta.Tag.Add(new Coding(@"http://example.org/sprinkler", tag));
            }

            return resource;
        }

        public static UriParamList GetSprinklerTagCriteria(Resource resource)
        {
            string tag = GetSprinklerTag(resource);
            if(String.IsNullOrEmpty(tag))
                return null;
            return
                GetSprinklerTagCriteria(tag);
        }

        public static string GetSprinklerTag(Resource resource)
        {
            if (resource.Meta == null)
                return null;
            return resource.Meta.Tag.Single(c => c.System == (@"http://example.org/sprinkler")).Code;
        }



        public static UriParamList GetSprinklerTagCriteria(string tag)
        {
            if (tag == null)
                return null;
            UriParamList paramList = new UriParamList();
            paramList.Add("_tag", String.Format(@"http://example.org/sprinkler|{0}", tag));
            return paramList;
        }

        public static string GenerateRandomSprinklerTag()
        {
            return Guid.NewGuid().ToString();
        }

        public static string GetReferenceId(this Resource resource)
        {
            return String.Format("{0}/{1}", resource.TypeName, resource.Id);
        }   
    }
}