using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Threading;

namespace FindTimeQuant
{
    class Program
    {
        static void Main(string[] args)
        {
            var processorNum = args.Length > 0 ? int.Parse(args[0]) - 1 : 1;
            Process.GetCurrentProcess().ProcessorAffinity = (IntPtr)(1 << processorNum);

            
        }
    }
}