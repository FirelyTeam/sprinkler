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

        public static string GetBasicId(this ResourceEntry entry)
        {
            return new ResourceIdentity(entry.Id).OperationPath.ToString();
        }

    }

}
