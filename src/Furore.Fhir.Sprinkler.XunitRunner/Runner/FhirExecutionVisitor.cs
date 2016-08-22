using System;
using System.Collections.Generic;
using System.Linq;
using Furore.Fhir.Sprinkler.Runner.Contracts;
using Furore.Fhir.Sprinkler.Xunit.ClientUtilities.XunitFhirExtensions.Attributes;
using Furore.Fhir.Sprinkler.XunitRunner.FhirExtensions;
using Xunit;
using Xunit.Abstractions;

namespace Furore.Fhir.Sprinkler.XunitRunner.Runner
{
    public class FhirExecutionVisitor : TestMessageVisitor<ITestAssemblyFinished>
    {
        public  FhirExecutionVisitor()
        {
            TestResults = new List<TestResult>();
        }

        public Action<TestResult> Log { get; set; }
       
        public List<TestResult> TestResults { get; private set; }

        protected override bool Visit(ITestPassed testPassed)
        {
            TestResult result = CreateTestResult(testPassed.Test);
            result.Outcome = TestOutcome.Success;
            AddResult(result);
            return base.Visit(testPassed);
        }

        protected override bool Visit(ITestFailed testFailed)
        {
            TestResult result = CreateTestResult(testFailed.Test);
            result.Outcome = TestOutcome.Fail;
            result.Messages = testFailed.Messages;

            AddResult(result);

            return base.Visit(testFailed);
        }

        protected override bool Visit(ITestSkipped testSkipped)
        {
            TestResult result = CreateTestResult(testSkipped.Test);
            result.Outcome = TestOutcome.Skipped;
            AddResult(result);

            return base.Visit(testSkipped);
        }

        private TestResult CreateTestResult(ITest test)
        {
            TestResult result = new TestResult()
            {
                Code = ReplaceGenericParameters (test, string.Join(",", test.TestCase.Traits[MetadataTraitDiscoverer.CodeKey].ToArray())),
                Title = ReplaceGenericParameters(test, test.TestCase.Traits[MetadataTraitDiscoverer.DescriptionKey].FirstOrDefault()),
                Category = test.TestCase.TestMethod.TestClass.Class.ToRuntimeType().Name
            };

            return result;
        }

        private string ReplaceGenericParameters(ITest test, string value)
        {
            if (test.TestCase.TestMethod.Method.IsGenericMethodDefinition)
            {
                string[] genericParamaters =
                    test.TestCase.TestMethod.Method.GetGenericArguments().OrderBy(t=>t.ToRuntimeType().GenericParameterPosition).Select(t => t.Name).ToArray();

                int start = test.DisplayName.IndexOf("<") + 1 ;
                int length = test.DisplayName.IndexOf(">") - start;
                string[] parameters = test.DisplayName.Substring(start, length).Split(',');

                for (int i = 0; i < genericParamaters.Length; i++)
                {
                    value = value.Replace(string.Format("{{{0}}}", genericParamaters[i]), parameters[i]);
                }
            }
            return value;
        }

        private void AddResult(TestResult result)
        {
            TestResults.Add(result);
            if (Log != null)
            {
                Log(result);
            }
        }
    }
}