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
using Furore.Fhir.Sprinkler.Framework.Framework.Attributes;
using Furore.Fhir.Sprinkler.Framework.Utilities;
using Furore.Fhir.Sprinkler.Runner.Contracts;
using Hl7.Fhir.Rest;

namespace Furore.Fhir.Sprinkler.Framework.Framework.TestExecution
{
    public class TestRunner : ITestRunner
    {
        private readonly FhirClient _client;
        private readonly IDictionary<Type, SprinklerTestClass> modules;
        private Action<TestResult> log;
        private readonly IEnumerable<ITestLoader> testLoaders;

        public TestRunner(string uri, IEnumerable<ITestLoader> testLoaders, Action<TestResult> log = null)
        {
            _client = new FhirClient(uri);
            this.log = log;
            this.testLoaders = testLoaders;
            modules = new Dictionary<Type, SprinklerTestClass>();
        }

        public void Log(TestResult testresult)
        {
            if (log != null) log(testresult);
        }

        private static TestResult RunTestMethod(string category, SprinklerTestClass instance, MethodInfo method)
        {
            var test = new TestResult {Category = category};
            var attribute = SprinklerTest.AttributeOf(method);
            var resourcePrerequisiteAttribute =
                method.GetCustomAttributes(typeof (ResourcePrerequisiteAttribute), false)
                    .OfType<ResourcePrerequisiteAttribute>();
            ResourcePrerequisitesHandler handler = new ResourcePrerequisitesHandler(resourcePrerequisiteAttribute);
           var x= handler.HandlePrerequisities();

            if (attribute != null)
            {
                test.Title = attribute.Title;
                test.Code = attribute.Code;
            }
            var dynamicAttribute = SprinklerDynamicTest.AttributeOf(method);
            if (dynamicAttribute != null)
            {
                test.Title = dynamicAttribute.GetTitle(method);
                test.Code = dynamicAttribute.GetCode(method);
            }
            try
            {
                method.Invoke(instance, null);
                test.Outcome = TestOutcome.Success;
                test.Exception = null;
            }
            catch (Exception e)
            {
                test.Outcome = TestOutcome.Fail;
                test.Exception = e.InnerException;
            }
            finally
            {

            }

            return test;
        }

        public void Run(params string[] codes)
        {
            foreach (Type type in GetTestModules())
            {
                RunModule(type, codes);
            }
        }

        public IEnumerable<Type> GetTestModules()
        {
            return testLoaders.SelectMany(l => l.GetTestModules());
        }

        private void RunAndLog(SprinklerTestClass instance, MethodInfo methodInfo)
        {
            var test = RunTestMethod(TestHelper.GetCategory(instance), instance, methodInfo);
            Log(test);
        }

        private SprinklerTestClass RunModule(Type moduleType, params string[] codes)
        {
            SprinklerTestClass module = GetInstanceOf(moduleType);
            module.SetClient(_client);
            var testMethods = TestHelper.GetTestMethods(moduleType, codes).ToList();
            if (testMethods.Any())
            {
                MethodInfo intializationMethod = TestHelper.GetInitializationMethod(moduleType);
                TestResult moduleCorrecltyInitialized = null;
                if (intializationMethod != null)
                {
                    moduleCorrecltyInitialized = RunTestMethod("Initialization", module, intializationMethod);
                }

                if (moduleCorrecltyInitialized == null || moduleCorrecltyInitialized.Outcome == TestOutcome.Success)
                {
                    foreach (var methodInfo in testMethods)
                    {
                        RunAndLog(module, methodInfo);
                    }
                }
            }
            return module;
        }

        public T Run<T>() where T : SprinklerTestClass
        {
           return (T)RunModule(typeof(T));
        }

        public SprinklerTestClass GetInstanceOf(Type testclass)
        {
            if (!modules.ContainsKey(testclass))
            {
                modules.Add(testclass,(SprinklerTestClass)Activator.CreateInstance(testclass));
            }
            return modules[testclass];
        }

        public void Clear()
        {
            modules.Clear();
        }
        
    }
    
} 