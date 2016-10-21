﻿/* 
 * Copyright (c) 2014, Furore (info@furore.com) and contributors
 * See the file CONTRIBUTORS for details.
 * 
 * This file is licensed under the BSD 3-Clause license
 * available at https://raw.github.com/furore-fhir/sprinkler/master/LICENSE
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Furore.Fhir.Sprinkler.CLI.Properties;
using Furore.Fhir.Sprinkler.Framework.Framework.TestExecution;
using Furore.Fhir.Sprinkler.Framework.Framework.TestScripts;
using Furore.Fhir.Sprinkler.Runner.Contracts;
using Furore.Fhir.Sprinkler.XunitRunner.Runner;
using Hl7.Fhir.Rest;

namespace Furore.Fhir.Sprinkler.CLI
{

    // commands: script, test, list
    // http://localhost.fiddler:1396/fhir TA -wait
    // sprinkler script http://fhir-dev.healthintersections.com.au/open automated-test-script.xml
    // 

    // Parameter usage: 
    // sprinkler script [url] [scripts]

    // Example:
    // sprinkler script http://fhir-dev.healthintersections.com.au/open automated-test-script.xml
    // sprinkler script http://fhir-dev.healthintersections.com.au/open test*.xml pipo*.xml

    public class Program
    {
        static Parameters parameters;
        static object lockingObj= new object(); 

        public static void Main(string[] args)
        {
           
            parameters = new Parameters(args);    

            Console.WriteLine(Resources.ProgramTitle);
            Console.WriteLine();

            
            if (parameters.Command("script"))
            {
                //var ts = new Fhir.Testing.Framework.TestScript() { Base = "http://fhir-dev.healthintersections.com.au/open", WantSetup = true, Filename = "automated-test-script.xml" };
                //ts.execute();
                RunScripts();
            }
            else if (parameters.Command("test"))
            {
                RunTests();
            }
            else if (parameters.Command("list"))
            {
                ShowModulesList();
            }
            else if (!parameters.Values.Any())
            {
                ShowOptions();
            }

            if (parameters.HasOption("-wait")) // mainly for debugging purposes
            {
                Console.WriteLine("Press any key...");
                Console.ReadKey();
            }
        }

        private static void log(TestResult result)
        {
            lock (lockingObj)
            {
                string designator = string.Format("{0}/{1} {2}", result.Category, result.Code, result.Title);
                Console.WriteLine(designator);
                Console.ForegroundColor = result.Outcome == TestOutcome.Success ? ConsoleColor.Green : ConsoleColor.Red;
                Console.WriteLine("{0}", result.Outcome);
                if (result.Outcome == TestOutcome.Skipped)
                {
                    foreach (string message in result.Messages)
                    {
                        Console.WriteLine(message);
                    }
                    if (parameters.HasOption("-waitOnSkip"))
                    {
                        Console.WriteLine("Press any key...");
                        Console.ReadKey();
                    }
                }


                if (result.Outcome == TestOutcome.Fail)
                {
                    if (result.OperationOutcome() != null)
                    {
                        Console.WriteLine("  - {0}\n", result.OperationOutcome());
                    }
                    foreach (string message in result.Messages)
                    {
                        Console.WriteLine(message);
                    }
                    Console.ResetColor();
                    if (parameters.HasOption("-waitOnFail"))
                    {
                        Console.WriteLine("Press any key...");
                        Console.ReadKey();
                    }
                }
                Console.ResetColor();
            }
        }

        private static ITestRunner CreateRunner()
        {
            var url = parameters.Values.First();
            ITestRunner runner = null;
            if (parameters.HasOption("-xunit"))
            {
                runner = new XUnitTestRunner(url, log, GetAssemblies(), parameters.HasOption("-outputLogging"));
            }
            else
            {
                runner = Test.CreateRunner(url, log, new[] { "Furore.Fhir.Sprinkler.TestSet.dll" });
            }

            return runner;
        }

        private static void ShowModulesList()
        {
            Console.WriteLine(Resources.availableModules);
            foreach (TestModule module in CreateRunner().GetTestModules())
            {
                Console.WriteLine("{0}:", module.Name);

                foreach (TestCase test in module.TestCases)
                {
                    Console.WriteLine("\t{0}: {1}", test.Code, test.Title);
                }
            }
        }

        private static void RunTests()
        {
            try
            {
                var tests = parameters.Values.Skip(1).ToArray();

                CreateRunner().Run(tests);
            }
            catch (Exception x)
            {
                Console.Error.WriteLine(Resources.error, x.Message);
            }
        }


        private static string[] GetAssemblies()
        {
            string[] assemblyNames = new[] {@"Furore.Fhir.Sprinkler.Xunit.TestSet.dll"};

            string basePath = AppDomain.CurrentDomain.BaseDirectory;

            return assemblyNames.Select(a => Path.Combine(basePath, a)).ToArray();
        }

        private static List<string> scriptlist()
        {
            var tests = parameters.Values.Skip(1);
            List<string> scripts = new List<string>();
            string dir = Directory.GetCurrentDirectory();
            foreach (string path in tests)
            {
                scripts.AddRange(Directory.EnumerateFiles(dir, path));
            }
            return scripts;
        }

        private static void RunScripts()
        {
            var url = parameters.Values.First();
            var scripts = scriptlist();
            foreach(string script in scripts)
            {
                var ts = new TestScript();
                ts.Base = url;
                ts.Filename = script;
                ts.WantSetup = true; 
                ts.execute();
            }
        }
       
      

        private static void ShowOptions()
        {
            var executable = Path.GetFileNameWithoutExtension(Assembly.GetExecutingAssembly().Location);
            Console.WriteLine(Resources.HelpForUsage, executable, Resources.HelpForSyntax);
            Console.WriteLine(Resources.parameters);
            Console.WriteLine(Resources.HelpForParameters);
            Console.WriteLine(Resources.options);
            
            Console.WriteLine("\t{0}\t{1}", "LIST", Resources.HelpForListParameter);
        }

    }
    public static class ExceptionWriter
    {
        public static string OperationOutcome(this TestResult testresult)
        {
            Exception exception = testresult.Exception;
            if (exception == null) return null;

            if (exception is FhirOperationException)
            {
                IEnumerable<string> details = (exception as FhirOperationException).Outcome.Issue.Select(i => i.Diagnostics);
                return string.Join(" - \n", details);
            }
            else return exception.Message;

        }
    }
}