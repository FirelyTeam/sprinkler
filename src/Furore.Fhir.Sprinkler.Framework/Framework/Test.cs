/* 
 * Copyright (c) 2014, Furore (info@furore.com) and contributors
 * See the file CONTRIBUTORS for details.
 * 
 * This file is licensed under the BSD 3-Clause license
 * available at https://raw.github.com/furore-fhir/sprinkler/master/LICENSE
 */

using System;
using Furore.Fhir.Sprinkler.Framework.Configuration;

namespace Furore.Fhir.Sprinkler.Framework.Framework
{
    public static class Test
    {
        public static TestRunner CreateRunner(string uri, Action<TestResult> log)
        {
            return new TestRunner(uri,new ITestLoader[]{new SprinklerConfigurationTestLoader()}, log);
        }

    }
}
