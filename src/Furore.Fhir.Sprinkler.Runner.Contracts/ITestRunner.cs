using System;
using System.Collections.Generic;

namespace Furore.Fhir.Sprinkler.Runner.Contracts
{
    public interface ITestRunner
    {
        void Run(string[] tests);
        IEnumerable<TestModule> GetTestModules();
    }
}
