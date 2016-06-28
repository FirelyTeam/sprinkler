using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Furore.Fhir.Sprinkler.Framework.Framework.Attributes;
using Furore.Fhir.Sprinkler.Framework.Utilities;

namespace Furore.Fhir.Sprinkler.Framework.Framework.TestExecution
{
    public static class TestHelper
    {

        public static IEnumerable<MethodInfo> TestMethodsOf(SprinklerTestClass instance, IEnumerable<string> codes = null)
        {
            var methods = instance.GetType().GetMethods();
            return methods.Where(m => IsProperTestMethod(m, codes));
        }

        public static MethodInfo GetInitializationMethod(Type testclass)
        {
            return testclass
                .GetMethods()
                .FirstOrDefault(mi => mi.GetCustomAttribute<ModuleInitializeAttribute>() != null);
        }

        public static IEnumerable<MethodInfo> GetTestMethods(Type testclass, IEnumerable<string> codes = null)
        {
            IEnumerable<MethodInfo> methods;
            //SprinklerDynamicModule sprinklerDynamicModule = testclass.GetCustomAttribute<SprinklerDynamicModule>();
            //if (sprinklerDynamicModule != null)
            //{
            //    var dynamicTestGenerator = Activator.CreateInstance(sprinklerDynamicModule.DynamicTestGenerator) as IDynamicTestGenerator;
            //    methods = dynamicTestGenerator.GetTestMethods();
            //}
            //else
            //{
            methods = testclass.GetMethods();
                
            //}
            if (codes != null && codes.Count() > 0)
            {
                return methods.Where(method => IsProperTestMethod(method, codes));
            }
            else
            {
                return methods.Where(method => IsProperTestMethod(method));
            }
        }

        internal static string GetCategory(SprinklerTestClass instance)
        {
            return SprinklerModule.AttributeOf(instance.GetType()).Name;
        }

        private static bool IsProperTestMethod(MethodInfo method, IEnumerable<string> codes)
        {
            var attribute = SprinklerTest.AttributeOf(method);
            return (attribute != null) && (codes.HasMatchFor(attribute.Code));
        }

        private static bool IsProperTestMethod(MethodInfo method, string code = null)
        {
            var attribute = SprinklerTest.AttributeOf(method);
            return attribute != null && (code == null || code.Equals(attribute.Code, StringComparison.OrdinalIgnoreCase));
        }
  
    }
}