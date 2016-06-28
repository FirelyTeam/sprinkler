/* 
 * Copyright (c) 2014, Furore (info@furore.com) and contributors
 * See the file CONTRIBUTORS for details.
 * 
 * This file is licensed under the BSD 3-Clause license
 * available at https://raw.github.com/furore-fhir/sprinkler/master/LICENSE
 */

using System;
using System.Globalization;
using System.Linq;
using System.Reflection;

namespace Furore.Fhir.Sprinkler.Framework.Framework.Attributes
{
    [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
    public sealed class SprinklerDynamicTest : Attribute
    {
        private string code;
        private string title;

        public SprinklerDynamicTest(string code, string title)
        {
            this.code = code;
            this.title = title;
        }

        public string GetCode(MethodInfo methodInfo)
        {
            return string.Format(CultureInfo.InvariantCulture, code, methodInfo.GetGenericArguments().Select(t => t.Name).ToArray());
        }

        public string GetTitle(MethodInfo methodInfo)
        {
            return string.Format(CultureInfo.InvariantCulture, title, methodInfo.GetGenericArguments().Select(t => t.Name).ToArray());
        }

        public static SprinklerDynamicTest AttributeOf(MethodInfo method)
        {
            return method.GetCustomAttributes(typeof(SprinklerDynamicTest), false).FirstOrDefault() as SprinklerDynamicTest;
        }
    }
}