using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace bmviewer
{
    class PerfStopwatch
    {
        static Stopwatch sw = null;
        static string label;
        public static void Start(string l)
        {
            label = l;
            sw = new Stopwatch();
            sw.Start();
        }

        public static long Stop()
        {
            sw.Stop();
            Console.WriteLine($"{label}: {sw.ElapsedMilliseconds}ms");
            return sw.ElapsedMilliseconds;
        }

    }
}
