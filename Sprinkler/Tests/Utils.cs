/* 
 * Copyright (c) 2014, Furore (info@furore.com) and contributors
 * See the file CONTRIBUTORS for details.
 * 
 * This file is licensed under the BSD 3-Clause license
 * available at https://raw.github.com/furore-fhir/sprinkler/master/LICENSE
 */
using Hl7.Fhir.Model;
using Hl7.Fhir.Rest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sprinkler.Tests
{
    public static class Utils
    {
        public static Patient NewPatient(string family, params string[] given)
        {
            Patient p = new Patient();
            HumanName n = new HumanName();
            foreach (string g in given) { n.WithGiven(g); }

            n.AndFamily(family);
            p.Name = new List<HumanName>();
            p.Name.Add(n);
            return p;
        }

        public static string GetBasicId(this BundleEntry entry)
        {
            return new ResourceIdentity(entry.Id).OperationPath.ToString();
        }

        public static IEnumerable<string>GetBasicIds(this Bundle bundle)
        {
            return bundle.Entries.Select(be => be.GetBasicId()).ToArray();
        }

        public static bool Has(this Bundle bundle, string id)
        {
            return bundle.Entries.FirstOrDefault(e => e.GetBasicId() == id) != null;
        }
    

    }

}
