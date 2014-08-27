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

namespace Sprinkler.Framework
{
    public class TestRunner
    {
        private readonly FhirClient _client;
        private readonly IDictionary<Type, SprinklerTestClass> _instances;
        private Action<TestResult> _log;

        public TestRunner(FhirClient client)
        {
            _client = client;
            _instances = new Dictionary<Type, SprinklerTestClass>();
        }

        public static IEnumerable<MethodInfo> TestMethodsOf(object instance, IEnumerable<string> codes = null)
        {
            var methods = instance.GetType().GetMethods();
            return methods.Where(m => IsProperTestMethod(m, codes));
        }

        private static TestResult RunTestMethod(string category, object instance, MethodInfo method)
        {
            var test = new TestResult {Category = category};
            var attribute = SprinklerTestAttribute.AttributeOf(method);
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
            catch (TargetInvocationException e)
            {
                if (e.InnerException is TestFailedException)
                {
                    test.Outcome = (e.InnerException as TestFailedException).Outcome;
                }
                else
                {
                    test.Outcome = TestOutcome.Fail;
                }

                test.Exception = new TestFailedException(e.Message, e.InnerException);
            }

            return test;
        }

        public void Run(string[] codesOrModules, Action<TestResult> log)
        {
            _log = log;
            var tests = FilterTestsForCodeOrModule(GetTestClasses(),
                codesOrModules);
            foreach (var test in tests)
            {
                var testInstance = GetInstanceOf(test.Key);
                foreach (var testMethod in test.Value)
                {
                    Run(testInstance, testMethod);
                }
            }
        }

        private static IEnumerable<KeyValuePair<Type, List<MethodInfo>>> FilterTestsForCodeOrModule(
            IEnumerable<Type> testclasses, string[] codesOrModules)
        {
            IDictionary<Type, List<MethodInfo>> methods = new Dictionary<Type, List<MethodInfo>>();
            foreach (var testclass in testclasses)
            {
                var moduleAttr = SprinklerTestModuleAttribute.AttributeOf(testclass);
                if (moduleAttr != null)
                {
                    if (codesOrModules==null || codesOrModules.Length == 0 || codesOrModules.Contains(moduleAttr.Name, StringComparer.OrdinalIgnoreCase))
                    {
                        methods.Add(testclass, GetTestMethods(testclass).ToList());
                        
                    }
                    else
                    {
                        methods.Add(testclass, GetTestMethods(testclass, codesOrModules).ToList());
                    }
                } 
            }
            return methods;
        }

        private void Run(SprinklerTestClass instance, MethodInfo methodInfo)
        {
            instance.SetClient(_client);
            RunAndLog(instance, methodInfo);
        }

        public void Run(SprinklerTestClass instance, IEnumerable<string> codes = null)
        {
            instance.SetClient(_client);
            foreach (var methodInfo in TestMethodsOf(instance, codes))
            {
                RunAndLog(instance, methodInfo);
            }
        }

        public T Run<T>() where T : SprinklerTestClass
        {
            var instance = Activator.CreateInstance<T>();
            Run(instance);
            return instance;
        }

        public static IEnumerable<Type> GetTypesWithAttribute<T>(Assembly assembly)
        {
            return assembly.GetTypes().Where(type => type.GetCustomAttributes(typeof (T), true).Length > 0);
        }

        public SprinklerTestClass GetInstanceOf(Type testclass)
        {
            if (!_instances.ContainsKey(testclass))
            {
                _instances.Add(testclass,(SprinklerTestClass)Activator.CreateInstance(testclass));
            }
            return _instances[testclass];
        }

        public void ClearInstances()
        {
            _instances.Clear();
        }

        public static IEnumerable<Type> GetTestClasses()
        {
            var assembly = Assembly.GetExecutingAssembly();
            return GetTypesWithAttribute<SprinklerTestModuleAttribute>(assembly);
        }

        public static IEnumerable<MethodInfo> GetTestMethods(Type testclass, string[] codes = null)
        {
            return testclass.GetMethods().Where(method => IsProperTestMethod(method, codes));
        }

        internal static string Category(SprinklerTestClass instance)
        {
            return SprinklerTestModuleAttribute.AttributeOf(instance.GetType()).Name;
        }

        private static bool IsProperTestMethod(MethodInfo method, IEnumerable<string> codes)
        {
            return codes == null ? IsProperTestMethod(method, null as string) : codes.Any(code => IsProperTestMethod(method, code));
        }

        private static bool IsProperTestMethod(MethodInfo method, string code)
        {
            var testAttribute = SprinklerTestAttribute.AttributeOf(method);
            return testAttribute != null && (code == null || code.Equals(testAttribute.Code,StringComparison.OrdinalIgnoreCase));
        }

        private void RunAndLog(SprinklerTestClass instance, MethodInfo methodInfo)
        {
            var test = RunTestMethod(Category(instance), instance, methodInfo);
            _log(test);
        }
    }
}