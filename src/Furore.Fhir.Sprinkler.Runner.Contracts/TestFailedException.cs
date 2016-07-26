using System;

namespace Furore.Fhir.Sprinkler.Runner.Contracts
{
    [Serializable]
    public class TestFailedException : Exception
    {
        public TestOutcome Outcome = TestOutcome.Fail;

        // parameterless constructor for serialization
        public TestFailedException()
        {
        }

        // overloaded ctor: message
        public TestFailedException(string message) : base(message)
        {
        }

        // overloaded ctor: outcome
        public TestFailedException(TestOutcome outcome) : base(outcome.ToString())
        {
            Outcome = outcome;
        }

        public TestFailedException(TestOutcome outcome, string message) : base(message ?? outcome.ToString())
        {
            Outcome = outcome;
        }

        // overloaded ctor: message, inner exception
        public TestFailedException(string message, Exception inner) : base(message, inner)
        {
        }

    }
}