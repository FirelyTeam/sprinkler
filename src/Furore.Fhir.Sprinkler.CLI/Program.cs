/* 
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
using Furore.Fhir.Sprinkler.Framework.Framework;

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
            string designator = string.Format("{0}/{1} {2}", result.Category, result.Code, result.Title);
            Console.WriteLine(designator);
            Console.ForegroundColor = result.Outcome == TestOutcome.Success ? ConsoleColor.Green : ConsoleColor.Red;
            Console.WriteLine("{0}", result.Outcome);
            if (result.Outcome == TestOutcome.Fail)
            {
                Console.WriteLine("  - {0}\n", result.OperationOutcome());
                Console.ResetColor();
                Console.WriteLine("Press any key...");
                Console.ReadKey();
            }
            Console.ResetColor();
        }

        private static void RunTests()
        {
            try
            {
                var url = parameters.Values.First();

                var tests = parameters.Values.Skip(1).ToArray();
                TestRunner runner = Test.CreateRunner(url, log, new[] { "Furore.Fhir.Sprinkler.TestSet.dll" });
                runner.Run(tests);
            }
            catch (Exception x)
            {
                Console.Error.WriteLine(Resources.error, x.Message);
            }
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
       
        private static void ShowModulesList()
        {
            Console.WriteLine(Resources.availableModules);
            var url = parameters.Values.First();

            TestRunner runner = Test.CreateRunner(url, log);
            foreach (Type type in runner.GetTestModules())
            {
                Console.WriteLine("{0}:", SprinklerModule.AttributeOf(type).Name);
                
                foreach(SprinklerTest test in TestHelper.GetTestMethods(type).Select(SprinklerTest.AttributeOf))
                {
                    Console.WriteLine("\t{0}: {1}", test.Code, test.Title);
                }
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
}