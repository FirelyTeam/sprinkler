using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Furore.Fhir.Sprinkler.Runner.Contracts;
using Furore.Fhir.Sprinkler.Xunit.ClientUtilities;
using Furore.Fhir.Sprinkler.Xunit.ClientUtilities.XunitFhirExtensions.Attributes;
using Furore.Fhir.Sprinkler.XunitRunner.FhirExtensions;
using Xunit.Abstractions;

namespace Furore.Fhir.Sprinkler.XunitRunner.Runner
{
    public class XUnitTestRunner : ITestRunner
    {
        private readonly string url;
        private readonly Action<TestResult> log;
        private readonly string[] testAssemblies;

        public XUnitTestRunner(string url, Action<TestResult> log, string[] testAssemblies)
        {
            this.url = url;
            this.log = log;
            this.testAssemblies = testAssemblies;
        }

        public void Run(string[] tests)
        {
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

    }
}