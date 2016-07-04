using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using Furore.Fhir.Sprinkler.Runner.Contracts;
using Furore.Fhir.Sprinkler.XunitRunner.FhirExtensions;
using Xunit;
using Xunit.Abstractions;
using Xunit.Runners;

namespace Furore.Fhir.Sprinkler.XunitRunner.Runner
{
    public class XUnitTestRunner : ITestRunner
    {
        private readonly string url;
        private readonly string[] testAssemblies;
        object consoleLock = new object();

        // Use an event to know when we're done
        ManualResetEvent finished = new ManualResetEvent(false);

        // Start out assuming success; we'll set this to 1 if we get a failed test
        int result = 0;

        public XUnitTestRunner(string url, Action<TestResult> log, string[] testAssemblies)
        {
            this.url = url;
            this.testAssemblies = testAssemblies;
        }

        public void Run(string[] tests)
        {
            TestConfiguration.Url = url;
            foreach (string testAssembly in testAssemblies)
            {
                using (var runner = AssemblyRunner.WithoutAppDomain(testAssembly))
                {
                    runner.OnDiscoveryComplete = OnDiscoveryComplete;
                    runner.OnExecutionComplete = OnExecutionComplete;
                    runner.OnTestFailed = OnTestFailed;
                    runner.OnTestSkipped = OnTestSkipped;
                    if (tests.Any())
                    {
                        runner.TestCaseFilter = @case => TestCaseFilter(tests, @case);
                    }

                    Console.WriteLine("Discovering...");
                    runner.Start();

                    finished.WaitOne();
                    finished.Dispose();
                }
            }

        }

        public IEnumerable<Type> GetTestModules()
        {
            return testAssemblies.SelectMany(a=>Assembly.LoadFrom(a).GetTypes());
        }

        private bool TestCaseFilter(string[] tests, ITestCase testCase)
        {
            return tests.Contains<string>(testCase.Traits.SingleOrDefault(x => x.Key == CodeTraitDiscoverer.KEY).Value.First().ToString());
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

            finished.Set();
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