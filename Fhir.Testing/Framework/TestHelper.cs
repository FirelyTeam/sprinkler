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

        public static IEnumerable<MethodInfo> TestMethodsOf(SprinklerTestClass instance, IEnumerable<string> codes = null)
        {
            var methods = instance.GetType().GetMethods();
            return methods.Where(m => IsProperTestMethod(m, codes));
        }

        public static IEnumerable<Type> GetModules()
        {
            return Assembly.GetExecutingAssembly().GetTypesWithAttribute<SprinklerModule>();
        }

        public static IEnumerable<MethodInfo> GetTestMethods(Type testclass, string[] codes = null)
        {
            IEnumerable<MethodInfo> methods = testclass.GetMethods();
            if (codes != null)
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
            return (attribute != null) && (codes.Contains(attribute.Code));
        }

        private static bool IsProperTestMethod(MethodInfo method, string code = null)
        {
            var attribute = SprinklerTest.AttributeOf(method);
            return attribute != null && (code == null || code.Equals(attribute.Code, StringComparison.OrdinalIgnoreCase));
        }
  
    }
}
