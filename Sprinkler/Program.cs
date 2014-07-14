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
using Sprinkler.Framework;

namespace Sprinkler
{
    public class Program
    {
        public enum OutputFormat
        {
            Html,
            Xml
        };

        private const string OutputPar = "-o";
        private const string ListPar = "-l";
        private const string FormatPar = "-f";
        private const string Header = "Sprinkler v0.6 - Conformance test suite for FHIR 0.8 (DSTU1)";

        public static IDictionary<string, string> KnownPars = new Dictionary<string, string>
        {
            {ListPar, "list all available modules and tests"},
            {FormatPar, "format: -f:Xml | -f:Html (optional. Default: Xml output)"},
            {OutputPar, "output path with filename (optional. Default: output to console)"}
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
            var pars = ReadArgs(args);
            var opts = pars.Item1;
            var mandatoryPars = pars.Item2;
            Console.WriteLine(Header);
            Console.WriteLine();
            if (opts.ContainsKey(ListPar))
            {
                ShowModulesList();
            }
            else if (!mandatoryPars.Any() || !IsValidUrl(mandatoryPars[0]))
            {
                ShowOptions();
                Console.Error.WriteLine("Missing mandatory Fhir-server URL.");
            }
            else
            {
                try
                {
                    Console.Write("Test started...");
                    RunTests(opts, mandatoryPars);
                }
                catch (Exception x)
                {
                    ShowOptions();
                    Console.Error.WriteLine(x.Message);
                }
            }
        }

        private static bool IsValidUrl(string text)
        {
            Uri uri;
            return (Uri.TryCreate(text, UriKind.Absolute, out uri) && null != uri);
        }

        private static void RunTests(IDictionary<string, string> opts, IList<string> mandatoryPars)
        {
            var url = mandatoryPars[0];
            var results = new TestResults(Header, true);
            TestSets.Run(url, results, mandatoryPars.Skip(1).ToArray());
            ProcessOutputOptions(results, opts);
        }

        private static void ShowModulesList()
        {
            var list = TestSets.GetTestModules();
            Console.WriteLine("Available modules and codes:\n");
            foreach (var module in list)
            {
                Console.WriteLine(module.Key);
                foreach (var method in module.Value)
                {
                    Console.WriteLine("\t{0} {1}", method.Item1, method.Item2);
                }
            }
        }

        private static void ShowOptions()
        {
            var executable = Path.GetFileName(Assembly.GetExecutingAssembly().CodeBase);
            Console.WriteLine("{0} [-options] <url> [<codes>]", executable);
            Console.WriteLine("\nParameters:");
            Console.WriteLine("\t<url>\tThe URL address of the FHIR server");
            Console.WriteLine(
                "\t<codes>\tA space-separated list of the test-modules or test-codes to execute (optional. Default: all tests will be executed).");
            Console.WriteLine("\nOptions:");
            foreach (var opt in KnownPars)
            {
                Console.WriteLine("\t{0}\t{1}", opt.Key, opt.Value);
            }
        }

        private static void ProcessOutputOptions(TestResults results, IDictionary<string, string> opts)
        {
            var outputWriter = opts.ContainsKey(OutputPar) ? File.CreateText(opts[OutputPar]) : Console.Out;
            var outputFormat = GetOutputFormat(opts);
            if (outputFormat == OutputFormat.Xml)
            {
                results.SerializeTo(outputWriter);
            }
            else
            {
                var path = Path.Combine(Environment.CurrentDirectory, @"xmlToHtml.xslt");
                TextReader xslReader = File.OpenText(path);
                results.SerializeTo(outputWriter, xslReader);
            }
            outputWriter.Close();
        }

        private static OutputFormat GetOutputFormat(IDictionary<string, string> opts)
        {
            var formatOpt = GetOptionValue(opts, FormatPar, OutputFormat.Xml.ToString());
            OutputFormat format;
            if (!Enum.TryParse(formatOpt, true, out format))
            {
                throw new ArgumentException(string.Format("Wrong value for parameter {0}:{1}", FormatPar, formatOpt));
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