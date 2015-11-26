/* 
 * Copyright (c) 2014, Furore (info@furore.com) and contributors
 * See the file CONTRIBUTORS for details.
 * 
 * This file is licensed under the BSD 3-Clause license
 * available at https://raw.github.com/furore-fhir/sprinkler/master/LICENSE
 */

using Hl7.Fhir.Rest;
using Sprinkler.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sprinkler.Framework
{
    public static class Test
    {
        public static string serverUri;
        public static string ServerUri { get { return serverUri; } }

        public static TestRunner CreateRunner(string uri, Action<TestResult> log)
        {
            serverUri = uri;
            FhirClient client = new FhirClient(uri);
            return new TestRunner(client, log);
        }

    }
}
