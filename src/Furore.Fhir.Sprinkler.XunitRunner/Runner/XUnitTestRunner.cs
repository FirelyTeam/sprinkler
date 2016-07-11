using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using Furore.Fhir.Sprinkler.Runner.Contracts;
using Furore.Fhir.Sprinkler.XunitRunner.FhirExtensions;
using Xunit;
using Xunit.Abstractions;
using Xunit.Runners;
using System.Linq;

namespace Furore.Fhir.Sprinkler.XunitRunner.Runner
{
    public class XUnitTestRunner : ITestRunner
    {
        private readonly string url;
        private readonly Action<TestResult> log;
        private readonly string[] testAssemblies;
        object consoleLock = new object();

        // Use an event to know when we're done

        // Start out assuming success; we'll set this to 1 if we get a failed test
        int result = 0;

        public XUnitTestRunner(string url, Action<TestResult> log, string[] testAssemblies)
        {
            this.url = url;
            this.log = log;
            this.testAssemblies = testAssemblies;
        }

        public void Run(string[] tests)
        {
            tests = new string[] { "SE" };
            TestConfiguration.Url = url;
            foreach (string testAssembly in testAssemblies)
            {
                using (var runner = FhirAssemblyRunner.WithoutAppDomain(testAssembly))
                {
                    TestConfiguration.AssemblyRootDirectory = Path.GetDirectoryName(testAssembly);

                    if (tests.Any())
                    {
                        runner.TestCaseFilter = @case => TestCaseFilter(tests, @case);
                    }
                    runner.Start(log);
                }
            }

        }

        public IEnumerable<TestModule> GetTestModules()
        {
            IEnumerable<TestModule> modules = Enumerable.Empty<TestModule>();
            foreach (string testAssembly in testAssemblies)
            {
                using (var runner = FhirAssemblyRunner.WithoutAppDomain(testAssembly))
                {
                    modules = modules.UnionAll(runner.DiscoverTests().GroupBy(t => t.TestMethod.TestClass.Class.Name)
                        .Select(g => new TestModule(g.Key, g.AsEnumerable().Select(testCase =>
                            new TestCase(testCase.Traits[MetadataTraitDiscoverer.CodeKey].First(),
                                testCase.Traits[MetadataTraitDiscoverer.DescriptionKey].First())))));
                }
            }
            return modules;
        }

        private bool TestCaseFilter(string[] tests, ITestCase testCase)
        {
            return tests.Any(t => testCase.Traits.SingleOrDefault(x => x.Key == MetadataTraitDiscoverer.CodeKey).Value.First().Contains(t));
        }

        void OnDiscoveryComplete(DiscoveryCompleteInfo info)
        {
            lock (consoleLock)
                Console.WriteLine($"Running {info.TestCasesToRun} of {info.TestCasesDiscovered} tests...");
        }

        void OnExecutionComplete(ExecutionCompleteInfo info)
        {
       
            lock (consoleLock)
                Console.WriteLine($"Finished: {info.TotalTests} tests in {Math.Round(info.ExecutionTime, 3)}s ({info.TestsFailed} failed, {info.TestsSkipped} skipped)");

        }

        void OnTestFailed(TestFailedInfo info)
        {
            lock (consoleLock)
            {
                Console.ForegroundColor = ConsoleColor.Red;

                Console.WriteLine("[FAIL] {0}: {1}", info.TestDisplayName, info.ExceptionMessage);
                if (info.ExceptionStackTrace != null)
                    Console.WriteLine(info.ExceptionStackTrace);

                Console.ResetColor();
            }

            result = 1;
        }

        void OnTestSkipped(TestSkippedInfo info)
        {
            lock (consoleLock)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("[SKIP] {0}: {1}", info.TestDisplayName, info.SkipReason);
                Console.ResetColor();
            }
        }
    }
}