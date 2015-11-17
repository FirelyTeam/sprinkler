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
using Hl7.Fhir.Model;
using Hl7.Fhir.Rest;

namespace Sprinkler.Tests
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
                Family = new List<string> {family},
                Given = new List<string> {given}
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

        public static bool IsDeleted(this Bundle.BundleEntryComponent entry)
        {
            if (entry.Request == null) return false;
            return (entry.Request.Method == Bundle.HTTPVerb.DELETE);
        }

        public static IEnumerable<Bundle.BundleEntryComponent> GetDeleted(this IEnumerable<Bundle.BundleEntryComponent> entries)
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

        public static Patient GetNewPatient(string family = "Adams" , params string[] given)
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
    }
}