using System;
using System.Linq;

namespace Furore.Fhir.Sprinkler.Framework.Framework.Attributes
{
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
}