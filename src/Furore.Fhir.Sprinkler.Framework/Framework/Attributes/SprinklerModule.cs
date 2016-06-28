using System;
using System.Linq;

namespace Furore.Fhir.Sprinkler.Framework.Framework.Attributes
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
}