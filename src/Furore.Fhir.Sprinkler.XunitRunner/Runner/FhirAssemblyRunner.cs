using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Furore.Fhir.Sprinkler.Runner.Contracts;
using Xunit;
using Xunit.Abstractions;
using Xunit.Runners;

namespace Furore.Fhir.Sprinkler.XunitRunner.Runner
{
    public class FhirAssemblyRunner : LongLivedMarshalByRefObject, IDisposable, IMessageSink
    {
        private volatile bool cancelled;
        private bool disposed;
        private readonly TestAssemblyConfiguration configuration;
        private readonly IFrontController controller;
        private readonly ManualResetEvent discoveryCompleteEvent = new ManualResetEvent(true);
        private readonly ManualResetEvent executionCompleteEvent = new ManualResetEvent(true);
        private readonly object statusLock = new object();

        public FhirAssemblyRunner(AppDomainSupport appDomainSupport,
            string assemblyFileName,
            string configFileName = null,
            bool shadowCopy = true,
            string shadowCopyFolder = null)
        {

            controller = new XunitFrontController(appDomainSupport, assemblyFileName, configFileName, shadowCopy,
                shadowCopyFolder, diagnosticMessageSink: this);
            configuration = ConfigReader.Load(assemblyFileName, configFileName);
            Status = AssemblyRunnerStatus.Idle;
        }

        public AssemblyRunnerStatus Status { get; private set; }

        public IEnumerable<TestResult> Start(Action<TestResult> log = null, IEnumerable<ITestCase> testCases = null)
        {
            testCases = testCases ?? Task.Run(() => DiscoverFromAssembly()).Result;
            return Task.Run(() => ExecuteAssembly(log, testCases)).Result;
        }

        public IEnumerable<ITestCase> DiscoverTests()
        {
           return Task.Run(() => DiscoverFromAssembly()).Result;
        }

        private IEnumerable<ITestCase> DiscoverFromAssembly()
        {
            lock (statusLock)
            {
                if (Status != AssemblyRunnerStatus.Idle)
                    throw new InvalidOperationException(
                        "Calling Start is not valid when the current status is not idle.");

                cancelled = false;
            }
            var discoveryOptions = GetDiscoveryOptions(null, null, false);
            var fhirDiscoveryVisitor = new FhirDiscoveryVisitor();
            fhirDiscoveryVisitor.TestCaseFilter = TestCaseFilter;
            controller.Find(false, fhirDiscoveryVisitor, discoveryOptions);
            Status = AssemblyRunnerStatus.Discovering;
            fhirDiscoveryVisitor.Finished.WaitOne();
            Status = AssemblyRunnerStatus.Idle;
            return fhirDiscoveryVisitor.TestCases;
        }
        private IEnumerable<TestResult> ExecuteAssembly(Action<TestResult> log, IEnumerable<ITestCase> testCases)
        {
            lock (statusLock)
            {
                if (Status != AssemblyRunnerStatus.Idle)
                    throw new InvalidOperationException(
                        "Calling Start is not valid when the current status is not idle.");

                cancelled = false;
            }

            var executionOptions = GetExecutionOptions(null, null, null);
            var fhirExecutionVisitor = new FhirExecutionVisitor();
            fhirExecutionVisitor.Log = log;
            controller.RunTests(testCases, fhirExecutionVisitor, executionOptions);
            Status = AssemblyRunnerStatus.Executing;

            fhirExecutionVisitor.Finished.WaitOne();

            Status = AssemblyRunnerStatus.Idle;
            return fhirExecutionVisitor.TestResults;

        }

        public Func<ITestCase, bool> TestCaseFilter { get; set; }

        /// <summary>
        /// Creates an assembly runner that discovers and run tests in a separate app domain.
        /// </summary>
        /// <param name="assemblyFileName">The test assembly.</param>
        /// <param name="configFileName">The test assembly configuration file.</param>
        /// <param name="shadowCopy">If set to <c>true</c>, runs tests in a shadow copied app domain, which allows
        /// tests to be discovered and run without locking assembly files on disk.</param>
        /// <param name="shadowCopyFolder">The path on disk to use for shadow copying; if <c>null</c>, a folder
        /// will be automatically (randomly) generated</param>
        public static FhirAssemblyRunner WithAppDomain(string assemblyFileName,
            string configFileName = null,
            bool shadowCopy = true,
            string shadowCopyFolder = null)
        {
            return new FhirAssemblyRunner(AppDomainSupport.Required, assemblyFileName, configFileName, shadowCopy,
                shadowCopyFolder);
        }

        /// <summary>
        /// Creates an assembly runner that discovers and runs tests without a separate app domain.
        /// </summary>
        /// <param name="assemblyFileName">The test assembly.</param>
        public static FhirAssemblyRunner WithoutAppDomain(string assemblyFileName)
        {
            return new FhirAssemblyRunner(AppDomainSupport.Denied, assemblyFileName);
        }

        bool IMessageSink.OnMessage(IMessageSinkMessage message)
        {
            return true;
        }

        public void Dispose()
        {
            lock (statusLock)
            {
                if (disposed)
                    return;

                if (Status != AssemblyRunnerStatus.Idle)
                    throw new InvalidOperationException("Cannot dispose the assembly runner when it's not idle");

                disposed = true;
            }

            controller.Dispose();
            discoveryCompleteEvent.Dispose();
            executionCompleteEvent.Dispose();
        }

        private ITestFrameworkDiscoveryOptions GetDiscoveryOptions(bool? diagnosticMessages,
          TestMethodDisplay? methodDisplay, bool? preEnumerateTheories)
        {
            var discoveryOptions = TestFrameworkOptions.ForDiscovery(configuration);
            discoveryOptions.SetSynchronousMessageReporting(true);

            if (diagnosticMessages.HasValue)
                discoveryOptions.SetDiagnosticMessages(diagnosticMessages);
            if (methodDisplay.HasValue)
                discoveryOptions.SetMethodDisplay(methodDisplay);
            if (preEnumerateTheories.HasValue)
                discoveryOptions.SetPreEnumerateTheories(preEnumerateTheories);

            return discoveryOptions;
        }

        private ITestFrameworkExecutionOptions GetExecutionOptions(bool? diagnosticMessages, bool? parallel,
            int? maxParallelThreads)
        {
            var executionOptions = TestFrameworkOptions.ForExecution(configuration);
            executionOptions.SetSynchronousMessageReporting(true);

            if (diagnosticMessages.HasValue)
                executionOptions.SetDiagnosticMessages(diagnosticMessages);
            if (parallel.HasValue)
                executionOptions.SetDisableParallelization(!parallel.GetValueOrDefault());
            if (maxParallelThreads.HasValue)
                executionOptions.SetMaxParallelThreads(maxParallelThreads);

            return executionOptions;
        }
    }
}