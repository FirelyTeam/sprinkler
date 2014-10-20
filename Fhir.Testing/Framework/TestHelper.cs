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
using System.Text;
using System.Threading.Tasks;
using Furore.Attributes;

namespace Sprinkler.Framework
{
    public static class TestHelper
    {
        // A list of groups, each of them holds a list of tests
        public static List<Tuple<string, List<Tuple<string, string>>>> GetTestModules()
        {
            List<Tuple<string, List<Tuple<string, string>>>> dictModulesCodes = new List<Tuple<string, List<Tuple<string, string>>>>();
            var testclasses = TestHelper.GetTestClasses();

            foreach (var testclass in testclasses)
            {
                var testmethods =
                    TestHelper.GetTestMethods(testclass)
                        .Select(SprinklerTestAttribute.AttributeOf)
                        .Select(methodAttr => Tuple.Create(methodAttr.Code, methodAttr.Title))
                        .OrderBy(el => el.Item1)
                        .ToList();
                var moduleAttr = SprinklerTestModuleAttribute.AttributeOf(testclass);
                dictModulesCodes.Add(Tuple.Create<string, List<Tuple<string, string>>>(moduleAttr.Name, testmethods));
            }
            return dictModulesCodes;
        }

        public static IEnumerable<MethodInfo> TestMethodsOf(SprinklerTestClass instance, IEnumerable<string> codes = null)
        {
            var methods = instance.GetType().GetMethods();
            return methods.Where(m => IsProperTestMethod(m, codes));
        }

        public static IEnumerable<Type> GetTestClasses()
        {
            return Assembly.GetExecutingAssembly().GetTypesWithAttribute<SprinklerTestModuleAttribute>();
        }

        public static IEnumerable<MethodInfo> GetTestMethods(Type testclass, string[] codes = null)
        {
            return testclass.GetMethods().Where(method => IsProperTestMethod(method, codes));
        }

        internal static string GetCategory(SprinklerTestClass instance)
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
            return testAttribute != null && (code == null || code.Equals(testAttribute.Code, StringComparison.OrdinalIgnoreCase));
        }

        public static IEnumerable<KeyValuePair<Type, List<MethodInfo>>> FilterTestsForCodeOrModule(
            IEnumerable<Type> testclasses, string[] codesOrModules)
        {
            IDictionary<Type, List<MethodInfo>> methods = new Dictionary<Type, List<MethodInfo>>();
            foreach (var testclass in testclasses)
            {
                var moduleAttr = SprinklerTestModuleAttribute.AttributeOf(testclass);
                if (moduleAttr != null)
                {
                    if (codesOrModules == null || codesOrModules.Length == 0 || codesOrModules.Contains(moduleAttr.Name, StringComparer.OrdinalIgnoreCase))
                    {
                        methods.Add(testclass, TestHelper.GetTestMethods(testclass).ToList());

                    }
                    else
                    {
                        methods.Add(testclass, TestHelper.GetTestMethods(testclass, codesOrModules).ToList());
                    }
                }
            }
            return methods;
        }

    }
}
