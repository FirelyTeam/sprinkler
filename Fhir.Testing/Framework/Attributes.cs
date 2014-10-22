/* 
 * Copyright (c) 2014, Furore (info@furore.com) and contributors
 * See the file CONTRIBUTORS for details.
 * 
 * This file is licensed under the BSD 3-Clause license
 * available at https://raw.github.com/furore-fhir/sprinkler/master/LICENSE
 */

using System;
using System.Linq;
using System.Reflection;

namespace Sprinkler.Framework
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
}