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
using Hl7.Fhir.Rest;
using Fhir.Testing.Framework;

namespace Sprinkler.Framework
{
    public class TestRunner
    {
        private readonly FhirClient _client;
        private readonly IDictionary<Type, SprinklerTestClass> modules;
        private Action<TestResult> log;

        public TestRunner(FhirClient client, Action<TestResult> log = null)
        {
            _client = client;
            this.log = log;
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
            if (attribute != null)
            {
                test.Title = attribute.Title;
                test.Code = attribute.Code;
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

            return test;
        }

        public void Run(params string[] codes)
        {
            foreach(Type type in TestHelper.GetModules())
            {
                var module = GetInstanceOf(type);
                var tests = TestHelper.GetTestMethods(type, codes);
                foreach (var test in tests)
                {
                    Run(module, test);
                }
            }
        }

        private void RunAndLog(SprinklerTestClass instance, MethodInfo methodInfo)
        {
            var test = RunTestMethod(TestHelper.GetCategory(instance), instance, methodInfo);
            Log(test);
        }

        private void Run(SprinklerTestClass module, MethodInfo methodInfo)
        {
            module.SetClient(_client);
            RunAndLog(module, methodInfo);
        }

        public void Run(SprinklerTestClass instance, IEnumerable<string> codes = null)
        {
            instance.SetClient(_client);
            foreach (var methodInfo in TestHelper.TestMethodsOf(instance, codes))
            {
                Run(instance, methodInfo);
            }
        }

        public T Run<T>() where T : SprinklerTestClass
        {
            var instance = Activator.CreateInstance<T>();
            Run(instance);
            return instance;
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