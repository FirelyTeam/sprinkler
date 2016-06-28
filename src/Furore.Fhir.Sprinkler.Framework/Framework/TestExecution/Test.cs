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
using System.Reflection;
using Furore.Fhir.Sprinkler.Framework.Configuration;
using Furore.Fhir.Sprinkler.Runner.Contracts;

namespace Furore.Fhir.Sprinkler.Framework.Framework.TestExecution
{
    public static class Test
    {
        public static TestRunner CreateRunner(string uri, Action<TestResult> log)
        {
            return new TestRunner(uri, new ITestLoader[] {new SprinklerTestLoader(new ConfigurationAssembliesNameProvider().GetTestAssemblies())}, log);
        }

        public static TestRunner CreateRunner(string uri, Action<TestResult> log,  params IAssembliesNameProvider[] assembliesNameProviders)
        {
            return new TestRunner(uri, new ITestLoader[] { new SprinklerTestLoader(assembliesNameProviders.SelectMany(a => a.GetTestAssemblies())) }, log);
        }

        public static TestRunner CreateRunner(string uri, Action<TestResult> log, IEnumerable<string> assembliesNames)
        {
            return new TestRunner(uri, new ITestLoader[] { new SprinklerTestLoader(assembliesNames) }, log);
        }

        public static TestRunner CreateRunner(string uri, Action<TestResult> log, IEnumerable<ITestLoader> testLoaders)
        {
            return new TestRunner(uri, testLoaders, log);
        }

        public static TestRunner CreateRunner(string uri, Action<TestResult> log, params Assembly[] assemblies)
        {
            return new TestRunner(uri, new ITestLoader[] { new AssemblyTestLoader(assemblies) }, log);
        }
    }
}
