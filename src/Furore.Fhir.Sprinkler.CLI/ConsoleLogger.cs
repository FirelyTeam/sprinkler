using System;
using Furore.Fhir.Sprinkler.Runner.Contracts;

namespace Furore.Fhir.Sprinkler.CLI
{
    public class ConsoleLogger
    {
        private readonly bool _waitOnSkip;
        private readonly bool _waitOnFail;
        object _lockingObj = new object();
        private int failedTests = 0;
        private int succededTests = 0;
        private int skippedTests = 0;

        public ConsoleLogger(bool waitOnSkip, bool waitOnFail)
        {
            _waitOnSkip = waitOnSkip;
            _waitOnFail = waitOnFail;
        }

        private void LogSkipped(TestResult result)
        {
            if (result.Outcome != TestOutcome.Skipped)
                return;
            skippedTests += 1;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("{0}", result.Outcome);
            foreach (string message in result.Messages)
            {
                Console.WriteLine(message);
            }
            Console.ResetColor();
            if (_waitOnSkip)
            {
                Console.WriteLine("Press any key...");
                Console.ReadKey();
            }
        }

        private void LogFailled(TestResult result)
        {
            if (result.Outcome != TestOutcome.Fail)
                return;
            failedTests += 1;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("{0}", result.Outcome);
            if (result.OperationOutcome() != null)
            {
                Console.WriteLine("  - {0}\n", result.OperationOutcome());
            }
            foreach (string message in result.Messages)
            {
                Console.WriteLine(message);
            }
            Console.ResetColor();
            if (_waitOnFail)
            {
                Console.WriteLine("Press any key...");
                Console.ReadKey();
            }
        }

        private void LogSuccess(TestResult result)
        {
            if (result.Outcome != TestOutcome.Success)
                return;
            succededTests += 1;
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("{0}", result.Outcome);
           
        }

        public void log(TestResult result)
        {
            lock (_lockingObj)
            {
                string designator = string.Format("{0}/{1} {2}", result.Category, result.Code, result.Title);
                Console.WriteLine(designator);
                Console.ForegroundColor = result.Outcome == TestOutcome.Success ? ConsoleColor.Green : ConsoleColor.Red;
                LogSuccess(result);
                LogFailled(result);
                LogSkipped(result);

            }
        }

        public void PrintCounters()
        {
            System.Console.WriteLine(
                $"{succededTests} tests succedded; {failedTests} tests failed; {skippedTests} tests skipped");
            Console.WriteLine("Press any key...");
            Console.ReadKey();
        }
    }
}