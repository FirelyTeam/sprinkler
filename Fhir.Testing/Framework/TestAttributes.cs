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
    public sealed class SprinklerTestModuleAttribute : Attribute
    {
        // This is a positional argument
        public SprinklerTestModuleAttribute(string name)
        {
            Name = name;
        }

        public string Name { get; private set; }


        /** given the testclass type, return its SprinklerTestModuleAttribute when it exists or null if it does not. */

        public static SprinklerTestModuleAttribute AttributeOf(Type testclass)
        {
            return testclass.GetCustomAttributes(typeof (SprinklerTestModuleAttribute), false).FirstOrDefault()
                as SprinklerTestModuleAttribute;
        }
    }

    [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
    public sealed class SprinklerTestAttribute : Attribute
    {
        public SprinklerTestAttribute(string code, string title)
        {
            Code = code;
            Title = title;
        }

        public string Title { get; private set; }
        public string Code { get; private set; }

        // This is a positional argument

        /** given a method, return its SprinklerTestAttribute when it exists or null if it does not. */

        public static SprinklerTestAttribute AttributeOf(MethodInfo method)
        {
            return method.GetCustomAttributes(typeof (SprinklerTestAttribute), false).FirstOrDefault()
                as SprinklerTestAttribute;
        }
    }
}