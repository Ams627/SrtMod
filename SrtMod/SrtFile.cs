using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace SrtMod
{
    internal class SrtFile
    {
        internal enum State
        {
            Init, HaveSerial, HaveTime, Text
        }

        internal class SrtEntry
        {
            public int Serial { get; set; }
            public int MilliSecondsStart { get; set; }
            public int MilliSecondsEnd { get; set; }
            public List<string> Text { get; set; }
        }

        private State _state = State.Init;
        private const string timePattern = @"^\s+\d\d:\d\d:\d\d,\d\d\d\s+-->\s+\d\d:\d\d:\d\d,\d\d\d\s+$";
        private List<string> _lineErrors = new List<string>();

        private string _filename;

        
        public SrtFile(string filename)
        {
            this._filename = filename;

            var currentSrtEntry = new SrtEntry();

            foreach (var line in File.ReadAllLines(filename).Select(x=>x.Trim()))
            {
                if (_state == State.Init)
                {
                    if (string.IsNullOrEmpty(line))
                    {
                        continue;
                    }
                    else if (line.All(char.IsDigit))
                    {
                        _state = State.HaveSerial;
                        currentSrtEntry.Serial = int.Parse(line);
                    }
                    else
                    {
                        _lineErrors.Add(line);
                    }
                }
                else if (_state == State.HaveSerial)
                {
                    var match = Regex.Match(line, timePattern);
                    if (match.Success)
                    {
                        GetMilliSeconds(line, out var start, out var end);
                        currentSrtEntry.MilliSecondsStart = start;
                        currentSrtEntry.MilliSecondsEnd = end;
                        _state = State.HaveTime;
                    }
                    else
                    {
                        _lineErrors.Add(line);
                    }
                }
                else if (_state == State.HaveTime)
                {
                    if (string.IsNullOrEmpty(line))
                    {
                        _state = State.Init;
                    }
                    else
                    {
                        currentSrtEntry.Text.Add(line);
                    }
                }
            }
        }

        private void GetMilliSeconds(string line, out int start, out int end)
        {
            var h = 10 * (line[0] - '0') + line[1] - '0';
            var m = 10 * (line[3] - '0') + line[4] - '0';
            var s = 10 * (line[6] - '0') + line[7] - '0';
            var ms = 100 * (line[9] - '0') + 10 * (line[10] - '0') + line[11] - '0';

            start = h * 3_600_000 + m * 60_000 * s * 1000 + ms;

            var offset = line.IndexOf('>');
            while (!(char.IsDigit(line[offset])))
            {
                offset++;
            }

            h = 10 * (line[offset + 0] - '0') + line[offset + 1] - '0';
            m = 10 * (line[offset + 3] - '0') + line[offset + 4] - '0';
            s = 10 * (line[offset + 6] - '0') + line[offset + 7] - '0';
            ms = 100 * (line[offset + 9] - '0') + 10 * (line[offset + 10] - '0') + line[offset + 11] - '0';

            end = h * 3_600_000 + m * 60_000 * s * 1000 + ms;
        }
    }
}