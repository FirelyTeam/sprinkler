using System;
using System.Linq;
using System.Reflection;

namespace Furore.Fhir.Sprinkler.Framework.Framework.Attributes
{
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