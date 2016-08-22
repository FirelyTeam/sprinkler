using System;
using System.Collections.Generic;
using Xunit;
using Xunit.Abstractions;

namespace Furore.Fhir.Sprinkler.XunitRunner.Runner
{
    public class FhirDiscoveryVisitor : TestMessageVisitor<IDiscoveryCompleteMessage>
    {
        public FhirDiscoveryVisitor()
        {
            TestCases = new List<ITestCase>();
        }

        public List<ITestCase> TestCases { get; private set; }

        public Func<ITestCase, bool> TestCaseFilter { get; set; }

        protected override bool Visit(ITestCaseDiscoveryMessage testCaseDiscovered)
        {
            if (TestCaseFilter == null || TestCaseFilter(testCaseDiscovered.TestCase))
                TestCases.Add(testCaseDiscovered.TestCase);

            return true;
        }
    }
}