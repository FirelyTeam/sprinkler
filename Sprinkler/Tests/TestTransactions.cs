/* 
 * Copyright (c) 2014, Furore (info@furore.com) and contributors
 * See the file CONTRIBUTORS for details.
 * 
 * This file is licensed under the BSD 3-Clause license
 * available at https://raw.github.com/furore-fhir/sprinkler/master/LICENSE
 */
using Hl7.Fhir.Model;
using Sprinkler.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sprinkler.Tests
{
    [SprinklerTestModule("Transactions")]
    class TestTransactions : SprinklerTestClass
    {
        ResourceEntry<Patient> entry;

        [SprinklerTest("TR01", "Adding a patient")]
        public void AddPatient()
        {
            Patient patient = Utils.NewPatient("Bach", "Johan", "Sebastian");
            entry = client.Create<Patient>(patient, null, true);

            Bundle bundle = client.History(entry);
            TestResult.Assert((bundle.Entries.Count == 1), "History of patient is not valid");
        }

        [SprinklerTest("TR02", "Updating a patient")]
        public void Updating()
        {
            // Gregorian calendar
            entry.Resource.BirthDate = "16850321";
            client.Update<Patient>(entry, true);

            Bundle bundle = client.History(entry);
            TestResult.Assert((bundle.Entries.Count == 2), "History of patient is not valid");
        }

        [SprinklerTest("TR02", "Reupdating a patient")]
        public void UpdatingAgain()
        {
            // Julian calendar
            entry.Resource.BirthDate = "16850331";
            client.Update<Patient>(entry, true);

            Bundle bundle = client.History(entry);
            TestResult.Assert((bundle.Entries.Count == 3), "History of patient is not valid");
        }

        [SprinklerTest("TR03", "Deleting record")]
        public void CountHistory()
        {
            client.Delete(entry);
            
            Bundle bundle = client.History(entry);
            TestResult.Assert((bundle.Entries.Count == 4), "History of patient is not valid");
        }

        //[SprinklerTest("TR10", "Failing data")]
        public void FailingData()
        {
            Patient p = new Patient();
            Identifier item = new Identifier();
            item.Label = "hl7v2";
            item.Value = "PID-55101";
            p.Identifier = new List<Identifier>();
            p.Identifier.Add(item);
            p.BirthDate = "1974-02-20";
            ResourceEntry<Patient> r = client.Create<Patient>(p);
        }
    }

  
}
