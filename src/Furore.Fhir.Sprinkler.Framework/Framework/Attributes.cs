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

namespace Furore.Fhir.Sprinkler.Framework.Framework
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public sealed class SprinklerModule : Attribute
    {
        // This is a positional argument

        public SprinklerModule(string name)
        {
            Name = name;
        }

        public string Name { get; private set; }


        /** given the testclass type, return its SprinklerTestModuleAttribute when it exists or null if it does not. */

        public static SprinklerModule AttributeOf(Type testclass)
        {
            return testclass.GetCustomAttributes(typeof (SprinklerModule), false).FirstOrDefault() as SprinklerModule;
        }
    }

    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public sealed class SprinklerDynamicModule : Attribute
    {
        public SprinklerDynamicModule(Type dynamicTestGenerator)
        {
            DynamicTestGenerator = dynamicTestGenerator;
        }

        public Type DynamicTestGenerator { get; set; }


        /** given the testclass type, return its SprinklerTestModuleAttribute when it exists or null if it does not. */

        public static SprinklerDynamicModule AttributeOf(Type testclass)
        {
            return testclass.GetCustomAttributes(typeof(SprinklerDynamicModule), false).FirstOrDefault() as SprinklerDynamicModule;
        }
    }

    [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
    public sealed class SprinklerTest : Attribute
    {
        public string Code { get; private set; }
        public string Title { get; private set; }

        public SprinklerTest(string code, string title)
        {
            Code = code;
            Title = title;
        }

        public static SprinklerTest AttributeOf(MethodInfo method)
        {
            return method.GetCustomAttributes(typeof(SprinklerTest), false).FirstOrDefault() as SprinklerTest;
        }
    }

    public sealed class ModuleInitializeAttribute : Attribute
    {
    }

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