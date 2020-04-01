using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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

    class Program
    {
        private static void Main(string[] args)
        {
            try
            {
                // patterns are:
                // +200
                // -200
                // 8+200 or 8-200
                // 7,203+200 or 7,203-200
                // semicolon separated combinations of the above - ranges must not overlap
                var adjustPattern1 = @"(?<offset>[-+]\d+)";             // whole file + or - milliseconds
                var adjustPattern2 = @"(?<seq>\d+)(?<offset>[-+]\d+)";          // single subtitle seq+-milliseconds 234-80 or 235+993
                var adjustPattern3 = @"(?<seq1>\d+),(?<seq2>\d*)(?<offset>[-+]\d+)";    // ranges plus or minus milliseconds 7,200+572 or without end of range 7,+572
                var regex = $"(?<alt1>{adjustPattern1})|(?<alt2>{adjustPattern2})|(?<alt3>{adjustPattern3})";


                var start = 0;
                var end = int.MaxValue;
                int offset = 0;
                string filename = "";

                bool foundRegexMatch = false;
                bool foundFilename = false;
                foreach (var arg in args)
                {
                    var match = Regex.Match(args[0], regex, RegexOptions.IgnoreCase);
                    if (match.Success)
                    {
                        foundRegexMatch = true;
                        GetStartEndOffset(match, ref start, ref end, ref offset);
                    }
                    else
                    {
                        if (!foundRegexMatch)
                        {
                            throw new Exception($"Adjustment must be specified before the filename.");
                        }
                        filename = arg;
                    }
                }

                Console.WriteLine($"start is {start}");
                Console.WriteLine($"end is {end}");
                Console.WriteLine($"offset is {offset}");

                if (!string.IsNullOrEmpty(filename))
                {
                    throw new Exception($"No filename specified.");
                }

                if (!foundRegexMatch)
                {
                    throw new Exception($"No adjustment specified.");
                }

                var srt = new SrtFile(filename);
                srt.Adjust(start, end, offset);
            }
            catch (Exception ex)
            {
                var fullname = System.Reflection.Assembly.GetEntryAssembly().Location;
                var progname = Path.GetFileNameWithoutExtension(fullname);
                Console.Error.WriteLine(progname + ": Error: " + ex.Message);
            }

        }

        private static void GetStartEndOffset(Match match, ref int start, ref int end, ref int offset)
        {
            if (!string.IsNullOrEmpty(match.Groups["alt1"].Value))
            {
                offset = int.Parse(match.Groups["offset"].Value);
            }
            else if (!string.IsNullOrEmpty(match.Groups["alt2"].Value))
            {
                start = int.Parse(match.Groups["seq"].Value);
                end = start;
                offset = int.Parse(match.Groups["offset"].Value);
            }
            else if (!string.IsNullOrEmpty(match.Groups["alt3"].Value))
            {
                start = int.Parse(match.Groups["seq1"].Value);
                end = int.Parse(match.Groups["seq2"].Value);
                offset = int.Parse(match.Groups["offset"].Value);
            }
        }
    }
}
