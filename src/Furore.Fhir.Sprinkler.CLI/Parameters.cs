using System;
using System.Collections.Generic;
using System.Linq;

namespace Furore.Fhir.Sprinkler.CLI
{
    public class Parameters
    {
        string _command;
        public List<string> Values = new List<string>();
        public Dictionary<string, string> Options = new Dictionary<string,string>();

        public Parameters(string[] args)
        {
            parse(args);
        }

        public bool Command(string cmd)
        {
            return string.Compare(cmd, _command, ignoreCase: true) == 0 ;
        }

        public bool HasOption(string option)
        {
            return Options.ContainsKey(option);
        }

        public string Option(string option, string _default)
        {
            if (HasOption(option))
            {
                return Options[option];
            }
            else 
            {
                if (_default != null) return _default;
                else throw new Exception("Invalid option: "+option);
            }
        }

        private void parse(string[] args)
        {
            _command = args.FirstOrDefault();
            foreach (var arg in args.Skip(1))
            {
                if (arg.StartsWith("-"))
                {
                    var colon = arg.IndexOf(':');
                    if (colon > -1)
                    {
                        var key = arg.Substring(0, colon);
                        var value = arg.Substring(colon + 1);
                        Options.Add(key, value);
                    }
                    else
                    {
                        Options.Add(arg, null);
                    }
                }
                else Values.Add(arg);
            }
        }

        
    }
}
