using Hl7.Fhir.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sprinkler
{
    /*abstract class Test
    {
        protected abstract TestResult Run();

        private TestResult _lastResult = null;

        public TestResult GetResult()
        {
            if (_lastResult == null)
                _lastResult = Run();    
                
            return _lastResult;
        }

        private string _name = null;

        public virtual string Name 
        { 
            get { return _name; }
            set { _name = value; }
        }
    }


    class TestSequence : Test
    {
        public IEnumerable<Test> Tests { get; protected set; }

        protected TestSequence()
        {
        }

        public TestSequence(IEnumerable<Test> tests)
        {
            Tests = tests;
        }

        protected override TestResult Run()
        {
            foreach (var test in Tests)
            {
                try
                {
                    var result = test.GetResult();
                    
                    if (result.Score != TestScore.Ok)
                        return result;
                }
                catch (Exception e)
                {
                    return TestResult.Fail(e);
                }
            }

            return TestResult.Ok;
        }       
    }


    class TestStatements : Test
    {
        public IEnumerable<TestStatement> Statements { get; protected set; }

        protected TestStatements()
        {
        }

        public TestStatements(IEnumerable<TestStatement> statements)
        {
            Statements = statements;
        }

        protected override TestResult Run()
        {
            foreach (var statement in Statements)
            {
                try
                {
                    var result = statement();
                    
                    if (result.Score != TestScore.Ok)
                        return result;
                }
                catch (Exception e)
                {
                    return TestResult.Fail(e);
                }
            }

            return TestResult.Ok;
        }
    }

    enum TestScore
    {
        Warning,
        Error,
        Fail
    }
*/

    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public sealed class SprinklerTestModuleAttribute : Attribute
    {
        // This is a positional argument
        public SprinklerTestModuleAttribute(string name)
        {
            this.Name = name;
        }

        public string Name
        {
            get;
            private set;
        }
    }


    [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
    public sealed class SprinklerTestAttribute : Attribute
    {
        // This is a positional argument
        public SprinklerTestAttribute(string title)
        {
            this.Title = title;
        }

        public string Title { get; private set; }

        // This is a named argument
//        public int NamedInt { get; set; }
    }


    public class TestFailedException : Exception
    {
        public TestFailedException(string message) : base(message) { }
        public TestFailedException(string message, Exception inner) : base(message, inner) { }

       // public TestScore Score { get; set; }
    }

    public static class TestResult
    {
        public static void Skipped()
        {
            Fail("Skipped");
        }

        public static void Fail(string message)
        {
            throw new TestFailedException(message);
        }

        public static void Fail(Exception inner, string message = "Exception caught")
        {
            throw new TestFailedException(message, inner);
        }
    }


    public static class TestRunner
    {
        public static void Run(object testInstance)
        {
            var moduleAttr = testInstance.GetType()
                .GetCustomAttributes(typeof(SprinklerTestModuleAttribute), false).FirstOrDefault()
                    as SprinklerTestModuleAttribute;

            var category = moduleAttr != null ? moduleAttr.Name : "General";

            foreach (var method in testInstance.GetType().GetMethods())
            {
                var testAttr = method.GetCustomAttributes(typeof(SprinklerTestAttribute), false).FirstOrDefault()
                                    as SprinklerTestAttribute;

                if (testAttr != null)
                {
                    Console.Write(String.Format("[{0,-12}] {1,-50} :", category, testAttr.Title));

                    if (method.GetParameters().Length == 0)
                    {
                        try
                        {
                            method.Invoke(testInstance, null);

                            Console.WriteLine("OK");
                        }
                        catch (System.Reflection.TargetInvocationException e)
                        {
                            handleInvocationException(e.InnerException);
                        }
                    }
                }
            }
        }

        public static void handleInvocationException(Exception exc)
        {
            Console.WriteLine("FAIL");

            if(!(exc is TestFailedException) )
                Console.WriteLine("(Exception caught)");

            while (exc != null)
            {
                if (!String.IsNullOrEmpty(exc.Message))
                    Console.WriteLine("*** " + exc.Message);

                if (exc is FhirOperationException)
                {
                    Console.WriteLine("*** OperationOutcome details:");
                    var foe = exc as FhirOperationException;
                    if (foe.Outcome != null && foe.Outcome.Issue != null)
                    {
                        int inr = 1;
                        foreach (var issue in foe.Outcome.Issue)
                        {
                            if( !issue.Details.StartsWith("Stack" ) )
                                Console.WriteLine(String.Format("     Issue {0}: {1}", inr, issue.Details));
                            inr++;
                        }
                    }
                }

                exc = exc.InnerException;
            }
        }
    }
}
