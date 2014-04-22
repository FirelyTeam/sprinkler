using Hl7.Fhir.Rest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Sprinkler.Framework
{

    public class TestRunner
    {
        private Action<TestResult> log;
        private FhirClient client;

        public TestRunner(FhirClient client, Action<TestResult> log)
        {
            this.client = client;
            this.log = log;
        }

        public static SprinklerTestAttribute AttributeOf(MethodInfo method)
        {
            var attribute = method.GetCustomAttributes(typeof(SprinklerTestAttribute), false).FirstOrDefault()
                                as SprinklerTestAttribute;
            return attribute;
        }
        
        public static bool IsProperTestMethod(MethodInfo method)
        {
            if (method.GetParameters().Length == 0)
            {
                if (AttributeOf(method) != null) 
                    return true;
            }
            return false;

        }

        public static IEnumerable<MethodInfo> TestMethodsOf(object instance)
        {
            MethodInfo[] methods = instance.GetType().GetMethods();
            return methods.Where(m => IsProperTestMethod(m));
        }

        public static TestResult RunTestMethod(string category, object instance, MethodInfo method)
        {
            TestResult test = new TestResult(); 
            test.Category = category;
            var attribute = AttributeOf(method);
            test.Title = attribute.Title;
            test.Code = attribute.Code;

            try
            {
                method.Invoke(instance, null);
                test.Outcome = TestOutcome.Success;
                test.Exception = null;
            }
            catch (System.Reflection.TargetInvocationException e)
            {
                if (e.InnerException is TestFailedException)
                {
                    test.Outcome = (e.InnerException as TestFailedException).Outcome;
                }
                else 
                { 
                    test.Outcome = TestOutcome.Fail; 
                }

                test.Exception = e.InnerException;
            }

            return test;
        }

        private string category(SprinklerTestClass instance)
        {
            var moduleAttr = instance.GetType()
                   .GetCustomAttributes(typeof(SprinklerTestModuleAttribute), false).FirstOrDefault()
                       as SprinklerTestModuleAttribute;

           return moduleAttr != null ? moduleAttr.Name : "General";

        }
        public void Run(SprinklerTestClass Instance)
        {
            Instance.SetClient(client);
            string category = this.category(Instance);
            foreach (var method in TestMethodsOf(Instance))
            {
                TestResult test = RunTestMethod(category, Instance, method);
                this.log(test);
            }
        }
        public T Run<T>() where T : SprinklerTestClass
        {
            T instance = Activator.CreateInstance<T>();
            Run(instance);
            return instance;
        }

        public static IEnumerable<Type> GetTypesWithAttribute<T>(Assembly assembly)
        {
            foreach (Type type in assembly.GetTypes())
            {
                if (type.GetCustomAttributes(typeof(T), true).Length > 0)
                {
                    yield return type;
                }
            }
        }

        public static IEnumerable<SprinklerTestClass> InstanciateAll()
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            IEnumerable<Type> testclasses = GetTypesWithAttribute<SprinklerTestModuleAttribute>(assembly);
            foreach(Type t in testclasses)
            {
                SprinklerTestClass c = (SprinklerTestClass) Activator.CreateInstance(t);
                yield return c;
            }
        }

        public void RunAll()
        {
            foreach(SprinklerTestClass stc in InstanciateAll())
            {
                Run(stc);
            }
        }
    }
}
