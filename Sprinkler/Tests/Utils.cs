/* 
 * Copyright (c) 2014, Furore (info@furore.com) and contributors
 * See the file CONTRIBUTORS for details.
 * 
 * This file is licensed under the BSD 3-Clause license
 * available at https://raw.github.com/furore-fhir/sprinkler/master/LICENSE
 */

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

        public static string GetBasicId(this BundleEntry entry)
        {
            return new ResourceIdentity(entry.Id).OperationPath.ToString();
        }

        public static IEnumerable<string> GetBasicIds(this Bundle bundle)
        {
            return bundle.Entries.Select(be => be.GetBasicId()).ToArray();
        }

        public static bool Has(this Bundle bundle, string id)
        {
            return bundle.Entries.FirstOrDefault(e => e.GetBasicId() == id) != null;
        }

        public static IEnumerable<T> ResourcesOf<T>(this Bundle bundle) where T : Resource, new()
        {
            IEnumerable<ResourceEntry<T>> entries = bundle.Entries.ByResourceType<T>();
            return entries.Select(e => e.Resource);
        }
    }
}