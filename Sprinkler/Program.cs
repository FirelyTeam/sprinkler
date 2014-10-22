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
using Sprinkler.Properties;
using Sprinkler.Framework;

namespace Sprinkler
{
    
    public static class Options
    {
        //public const string OUTPUT = "-o";
        public const string LIST = "-l";
        //public const string FORMAT = "-f";
    }

    public class Program
    {
        static Parameters parameters;

        public static void Main(string[] args)
        {
            parameters = new Parameters(args);    

            Console.WriteLine(Resources.ProgramTitle);
            Console.WriteLine();

            if (parameters.HasOption(Options.LIST))
            {
                ShowModulesList();
            }
            else if (!parameters.Values.Any())
            {
                ShowOptions();
            }
            else
            {
                RunTests();
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
            Console.WriteLine("{0}[{1}]", designator.PadRight(80, '.'), result.Outcome);
        }

        private static void RunTests()
        {
            try
            {
                var url = parameters.Values.First(); 
                var tests = parameters.Values.Skip(1).ToArray(); 
                TestRunner runner = Test.CreateRunner(url, log);
                runner.Run(tests);
            }
            catch (Exception x)
            {
                Console.Error.WriteLine(Resources.error, x.Message);
            }
        }
       
        private static void ShowModulesList()
        {
            Console.WriteLine(Resources.availableModules);
            foreach(Type type in TestHelper.GetModules())
            {
                Console.WriteLine(SprinklerModule.AttributeOf(type).Name);
                
                foreach(SprinklerTest test in TestHelper.GetTestMethods(type).Select(SprinklerTest.AttributeOf))
                {
                    Console.WriteLine("{0}: {1}", test.Code, test.Title);
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
            
            Console.WriteLine("\t{0}\t{1}", Options.LIST, Resources.HelpForListParameter);
        }

    }
}