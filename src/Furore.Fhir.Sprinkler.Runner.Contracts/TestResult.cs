using System;
using System.Collections.Generic;

namespace Furore.Fhir.Sprinkler.Runner.Contracts
{
    public class TestResult
    {
        public string Category { get; set; }
        public string Code { get; set; }
        public string Title { get; set; }
        public TestOutcome Outcome { get; set; }
        public Exception Exception { get; set; }
    }

    public enum TestOutcome
    {
        Success,
        Fail,
        Skipped
    }

    
}