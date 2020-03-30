﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


// 82
// 00:04:24,611 --> 00:04:27,512
// Sir, this is your last warning.
// You gotta be kidding me!
   
// 83
// 00:04:27,546 --> 00:04:29,314
// You got about two
// seconds to stand down,



namespace SrtMod
{

    class ProgramOptions
    {
        private readonly List<string> _invalidOptions = new List<string>();
        private readonly Dictionary<string, List<string>> _validOptionsAndParams = new Dictionary<string, List<string>>();

        public class OptionSpec
        {
            public string Name { get; set; }
            public int NumParams { get; set; }
        }


        public ProgramOptions(string[] args, OptionSpec[] specs)
        {
            var validOptions = specs.ToDictionary(x => x.Name, x => x.NumParams);

            var skip = 0;
            string currentOption = "";

            foreach (var arg in args)
            {
                if (skip-- > 0)
                {
                    DictUtils.AddEntry(_validOptionsAndParams, currentOption, arg);
                }
                else if (arg.StartsWith("--"))
                {
                    var option = arg.Substring(2);
                    if (!validOptions.TryGetValue(option, out var nParams))
                    {
                        _invalidOptions.Add(option);
                        continue;
                    }
                    currentOption = option;
                    skip = nParams;
                }
            }
            if (skip != 0)
            {
                throw new Exception($"Not enough arguments supplied for option --{currentOption}");
            }
        }

        List<string> GetParams(string option)
        {
            var ret = _validOptionsAndParams.TryGetValue(option, out var paramList);
            if (ret)
            {
                return paramList;
            }
            return new List<string>();
        }

        public List<string> InvalidOptions => _invalidOptions;

    }




    class Program
    {
        private static void Main(string[] args)
        {
            try
            {
                var srt = new SrtFile(args[0]);
            }
            catch (Exception ex)
            {
                var fullname = System.Reflection.Assembly.GetEntryAssembly().Location;
                var progname = Path.GetFileNameWithoutExtension(fullname);
                Console.Error.WriteLine(progname + ": Error: " + ex.Message);
            }

        }
    }
}
