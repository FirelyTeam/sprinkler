﻿/* 
 * Copyright (c) 2014, Furore (info@furore.com) and contributors
 * See the file CONTRIBUTORS for details.
 * 
 * This file is licensed under the BSD 3-Clause license
 * available at https://raw.github.com/furore-fhir/sprinkler/master/LICENSE
 */
using System;
using System.Globalization;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using Hl7.Fhir.Rest;

namespace Sprinkler.Framework
{
    public enum TestOutcome
    {
        Success,
        Fail,
        Skipped
    }

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

        // overloaded ctor: message, inner exception
        public TestFailedException(string message, Exception inner) : base(message, inner)
        {
        }

    }

    public class TestResult
    {
        public string Category { get; set; }
        public string Code { get; set; }
        public string Title { get; set; }
        public TestOutcome Outcome { get; set; }
        public Exception Exception { get; set; }

        public static void Skip()
        {
            throw new TestFailedException(TestOutcome.Skipped);
        }
    }

    
}