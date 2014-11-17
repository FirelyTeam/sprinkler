/* 
 * Copyright (c) 2014, Furore (info@furore.com) and contributors
 * See the file CONTRIBUTORS for details.
 * 
 * This file is licensed under the BSD 3-Clause license
 * available at https://raw.github.com/furore-fhir/sprinkler/master/LICENSE
 */

using System;
using System.Collections.Generic;
using Hl7.Fhir.Model;
using Sprinkler.Framework;

namespace Sprinkler.Tests
{
    [SprinklerModule("Validation")]
    public class ValidatorTest : SprinklerTestClass
    {
        [SprinklerTest("V001", "Validate a valid resource")]
        public void ValidateCreateResource()
        {
            Patient patient = DemoData.GetDemoPatient();

            OperationOutcome oo;
            if (!Client.TryValidateCreate(patient, out oo, null))
                Assert.Fail("Validation incorrectly reported failure.");
        }

        [SprinklerTest("V002", "Validate an invalid resource")]
        public void ValidateInvalidCreateResource()
        {
            Patient patient = DemoData.GetDemoPatient();
            patient.Identifier = new List<Identifier> {new Identifier {System = "urn:oid:hallo" }};

            OperationOutcome oo;
            if (!Client.TryValidateCreate(patient, out oo, null))
                Assert.Fail("Validation incorrectly reported failure.");
        }

        [SprinklerTest("V003", "Validate a valid resource update")]
        public void ValidateUpdateResource()
        {
            Patient patient = DemoData.GetDemoPatient(); 
            ResourceEntry<Patient> result = Client.Create(patient);

            OperationOutcome oo;
            if (!Client.TryValidateUpdate(result, out oo))
                Assert.Fail("Validation incorrectly reported failure.");
        }

        [SprinklerTest("V004", "Validate an invalid resource update")]
        public void ValidateInvalidUpdateResource()
        {
            Patient patient = DemoData.GetDemoPatient();
            ResourceEntry<Patient> result = Client.Create(patient);
            patient.Identifier = new List<Identifier> {new Identifier {System = "urn:oid:hallo" }};

            OperationOutcome oo;
            if (!Client.TryValidateUpdate(result, out oo))
                Assert.Fail("Validation incorrectly reported failure.");
        }
    }
}