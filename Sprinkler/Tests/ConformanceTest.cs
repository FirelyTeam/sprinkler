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
using System.Text;
using System.Net;
using System.Net.Mime;
using Hl7.Fhir.Rest;
using Sprinkler.Framework;

namespace Sprinkler.Tests
{
    [SprinklerTestModule("Conformance")]
    public class ConformanceTest : SprinklerTestClass
    {

        [SprinklerTest("C001", "Request conformance on /metadata")]
        public void GetConformanceUsingMetadata()
        {
            var entry = client.Conformance(useOptionsVerb: false);
            checkResultHeaders();           
        }

        [SprinklerTest("C002", "Request conformance using OPTIONS")]
        public void GetConformanceUsingOptions()
        {
           client.Conformance(useOptionsVerb: true);
           checkResultHeaders();
           
        }

        private void checkResultHeaders()
        {
            HttpTests.AssertValidResourceContentTypePresent(client);
            HttpTests.AssertContentLocationValidIfPresent(client);
        }
 
    }
}
