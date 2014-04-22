/* 
 * Copyright (c) 2014, Furore (info@furore.com) and contributors
 * See the file CONTRIBUTORS for details.
 * 
 * This file is licensed under the BSD 3-Clause license
 * available at https://raw.github.com/furore-fhir/sprinkler/master/LICENSE
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sprinkler.Framework
{
    public enum TestOutcome { Success, Fail, Skipped }

    public class TestFailedException : Exception
    {
        public TestOutcome Outcome = TestOutcome.Fail;
        public TestFailedException(string message) : base(message) { }
        public TestFailedException(TestOutcome outcome)
            : base(outcome.ToString())
        {
            this.Outcome = outcome;
        }
        public TestFailedException(string message, Exception inner) : base(message, inner) { }
    }
   
    public class TestResult
    {
        public string Category { get; set; }
        public string Code { get; set; }
        public string Title { get; set; }
        public TestOutcome Outcome { get; set; }
        public Exception Exception { get; set; }

        public static void Skipped()
        {
            throw new TestFailedException(TestOutcome.Skipped);
        }

        public static void Fail(string message)
        {
            throw new TestFailedException(message);
        }

        public static void Fail(Exception inner, string message = "Exception caught")
        {
            throw new TestFailedException(message, inner);
        }

        public static void Assert(bool assertion, string message)
        {
            if (!assertion)
                Fail(message);
        }
    }

}
