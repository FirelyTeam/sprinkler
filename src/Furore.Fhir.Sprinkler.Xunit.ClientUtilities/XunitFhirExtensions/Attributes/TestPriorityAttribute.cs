using System;

namespace Furore.Fhir.Sprinkler.Xunit.ClientUtilities.XunitFhirExtensions.Attributes
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class TestPriorityAttribute : Attribute
    {
        public TestPriorityAttribute(int priority)
        {
            Priority = priority;
        }
        public int Priority { get; private set; }
    }
}