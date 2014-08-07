/* 
 * Copyright (c) 2014, Furore (info@furore.com) and contributors
 * See the file CONTRIBUTORS for details.
 * 
 * This file is licensed under the BSD 3-Clause license
 * available at https://raw.github.com/furore-fhir/sprinkler/master/LICENSE
 */

using System.Collections.Generic;
using Hl7.Fhir.Model;
using Sprinkler.Framework;

namespace Sprinkler.Tests
{
    [SprinklerTestModule("Transactions")]
    internal class TestTransactions : SprinklerTestClass
    {
        private ResourceEntry<Patient> entry;

        [SprinklerTest("TR01", "Adding a patient")]
        public void AddPatient()
        {
            Patient patient = Utils.NewPatient("Bach", "Johan", "Sebastian");
            entry = Client.Create(patient, null, true);

            Bundle bundle = Client.History(entry);
            TestResult.Assert((bundle.Entries.Count == 1), "History of patient is not valid");
        }

        [SprinklerTest("TR02", "Updating a patient")]
        public void Updating()
        {
            // Birthday of Bach on Gregorian calendar
            entry.Resource.BirthDate = "1685-03-21";
            entry = Client.Update(entry, true);

            Bundle bundle = Client.History(entry);
            TestResult.Assert((bundle.Entries.Count == 2), "History of patient is not valid");
        }

        [SprinklerTest("TR03", "Reupdating a patient")]
        public void UpdatingAgain()
        {
            // Birthday of Bach on Julian calendar
            entry.Resource.BirthDate = "1685-03-31";
            Client.Update(entry, true);

            Bundle bundle = Client.History(entry);
            TestResult.Assert((bundle.Entries.Count == 3), "History of patient is not valid");
        }

        [SprinklerTest("TR04", "Deleting record")]
        public void CountHistory()
        {
            Client.Delete(entry);

            Bundle bundle = Client.History(entry);
            TestResult.Assert((bundle.Entries.Count == 4), "History of patient is not valid");
        }

        //[SprinklerTest("TR10", "Failing data")]
        public void FailingData()
        {
            var p = new Patient();
            var item = new Identifier();
            item.Label = "hl7v2";
            item.Value = "PID-55101";
            p.Identifier = new List<Identifier>();
            p.Identifier.Add(item);
            p.BirthDate = "1974-02-20";
            ResourceEntry<Patient> r = Client.Create(p);
        }
    }
}