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
    public class Program
    {
        public enum OutputFormat
        {
            Html,
            Xml,
            Raw
        };

        internal static class Params
        {
            public const string OUTPUT = "-o";
            public const string LIST = "-l";
            public const string FORMAT = "-f";
        }

        public static IDictionary<string, string> KnownPars = new Dictionary<string, string>
        {
            {Params.LIST, Texts.HelpForListParameter},
            {Params.FORMAT, Texts.HelpForFormatParameter},
            {Params.OUTPUT, Texts.HelpForOutputPararameter}
        };

        /**
         * Parameters:
         * -l                                    show options
         * -f                                    output format [o:Html, o:Xml]
         * -o:filepath                           output path+filename 
         * [url]                                 fhir url
         * [codes]                               space-separated list of test codes or test categories
         */

        public static void Main(string[] args)
        {
            var parameters = ReadArgs(args);
            var opts = parameters.Item1;
            var mandatoryPars = parameters.Item2;
            Console.WriteLine(Texts.ProgramTitle);
            Console.WriteLine();
            if (opts.ContainsKey(Params.LIST))
            {
                ShowModulesList();
            }
            else if (!mandatoryPars.Any())
            {
                ShowOptions();
            }
            else
            {
                try
                {
                    RunTests(opts, mandatoryPars);
                }
                catch (Exception x)
                {
                    Console.Error.WriteLine(Texts.error, x.Message);
                }
            }
        }

        private static void log(TestResult result)
        {
            string designator = string.Format("{0}/{1} {2}", result.Category, result.Code, result.Title);
            Console.WriteLine("{0}[{1}]", designator.PadRight(80, '.'), result.Outcome);
        }

        private static void RunTests(IDictionary<string, string> opts, IList<string> mandatoryPars)
        {
            var url = mandatoryPars[0];
            
            var outputFormat = GetOutputFormat(opts);
            var outputFilename = GetOutputFilename(opts, outputFormat);
            var outputWriter = outputFilename == null
                ? Console.Out
                : File.CreateText(GetOutputFilename(opts, outputFormat));
            
            if (outputFormat != OutputFormat.Raw) Console.Write(Texts.MessageTestStarted);
            //var testSet = TestSet.NewInstance(url);
            //testSet.Run(results, mandatoryPars.Skip(1).ToArray());

            var tests = mandatoryPars.Skip(1).ToArray(); 

            TestRunner runner = Test.CreateRunner(url, log);
            
            runner.Run(tests);

            Console.WriteLine();
        }

        private static string GetOutputFilename(IDictionary<string, string> opts, OutputFormat outputFormat)
        {
            if (!opts.ContainsKey(Params.OUTPUT)) return null;
            var outputFilename = opts[Params.OUTPUT];
            if (String.Empty == outputFilename)
            {
                throw new ArgumentException(string.Format(Texts.ErrorWrongValueForParameter, Params.OUTPUT, outputFilename));
            }
            var format = outputFormat.ToString().ToLowerInvariant();
            return outputFilename.EndsWith(format, StringComparison.OrdinalIgnoreCase)
                ? outputFilename
                : outputFilename + "." + format;
        }

        private static void ShowModulesList()
        {
            var list = TestHelper.GetTestModules();
            Console.WriteLine(Texts.availableModules);
            foreach (var module in list)
            {
                Console.WriteLine(module.Item1 + ":");
                foreach (var method in module.Item2)
                {
                    Console.WriteLine("\t{0} {1}", method.Item1, method.Item2);
                }
            }
        }

        private static void ShowOptions()
        {
            var executable = Path.GetFileNameWithoutExtension(Assembly.GetExecutingAssembly().Location);
            Console.WriteLine(Texts.HelpForUsage, executable, Texts.HelpForSyntax);
            Console.WriteLine(Texts.parameters);
            Console.WriteLine(Texts.HelpForParameters);
            Console.WriteLine(Texts.options);
            foreach (var opt in KnownPars)
            {
                Console.WriteLine("\t{0}\t{1}", opt.Key, opt.Value);
            }
        }

        /*
        private static void ProcessOutputOptions(TestResults results, TextWriter outputWriter, OutputFormat outputFormat)
        {
            if (outputFormat == OutputFormat.Raw) return; // nothing to do 
            TextReader xslTransform = null;
            //switch (outputFormat)
            //{
            //    case OutputFormat.Raw:
            //        xslTransform = new StringReader(Resources.xmlToRaw);
            //        break;
            //    case OutputFormat.Html:
            //        xslTransform = new StringReader(Resources.xmlToHtml);
            //        break;
            //    default:
            //        xslTransform = null;
            //        break;
            //}
            if (outputFormat == OutputFormat.Html)
            {
                xslTransform = new StringReader(Texts.xmlToHtml);
            }
            results.SerializeTo(outputWriter, xslTransform);
            outputWriter.Close();
        }
        */

        private static OutputFormat GetOutputFormat(IDictionary<string, string> opts)
        {
            var formatOpt = GetOptionValue(opts, Params.FORMAT, OutputFormat.Raw.ToString());
            OutputFormat format;
            if (!Enum.TryParse(formatOpt, true, out format))
            {
                throw new ArgumentException(string.Format(Texts.ErrorWrongValueForParameter, Params.FORMAT, formatOpt));
            }
            return format;
        }

        private static string GetOptionValue(IDictionary<string, string> opts, string optionKey,
            string defaultIfNull = null)
        {
            string ret;
            return opts.TryGetValue(optionKey, out ret) ? ret : defaultIfNull;
        }

        private static Tuple<IDictionary<string, string>, string[]> ReadArgs(string[] args)
        {
            IDictionary<string, string> options = new Dictionary<string, string>();
            IList<string> nonOptions = new List<string>();
            if (!args.Any()) return new Tuple<IDictionary<string, string>, string[]>(options, nonOptions.ToArray());
            foreach (var arg in args)
            {
                if (arg.StartsWith("-"))
                {
                    var colon = arg.IndexOf(':');
                    if (colon > -1)
                    {
                        var key = arg.Substring(0, colon);
                        var value = arg.Substring(colon + 1);
                        options.Add(key, value);
                    }
                    else
                    {
                        options.Add(arg, null);
                    }
                }
                else nonOptions.Add(arg);
            }
            return new Tuple<IDictionary<string, string>, string[]>(options, nonOptions.ToArray());
        }
    }
}