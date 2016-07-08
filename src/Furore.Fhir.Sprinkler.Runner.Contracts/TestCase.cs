using System.Collections.Generic;

namespace Furore.Fhir.Sprinkler.Runner.Contracts
{
    public class TestCase
    {
        public TestCase(string code, string title)
        {
            Code = code;
            Title = title;
        }
        public string Code { get; private set; }

        public string Title { get; private set; }
    }

    public class TestModule
    {
        public TestModule(string name, IEnumerable<TestCase> testCases )
        {
            Name = name;
            TestCases = testCases;
        }
        public string Name { get; private set; }
        public IEnumerable<TestCase> TestCases { get; private set; }
    }
}