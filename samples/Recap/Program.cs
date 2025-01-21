using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace Recap
{
    class Program
    {
        static void Main(string[] args)
        {
            var processorNum = 1;
            // Process.GetCurrentProcess().ProcessorAffinity = (IntPtr)(1 << processorNum);

            var dict = new Dictionary<string, string>();
            dict["test"] = "test";

            var t = new Thread(() =>
            {
                while(true)
                    dict[Guid.NewGuid().ToString()] = "test";
            });
            t.Start();

            long failedAfter = 0;
            string value;
            try
            {
                while(true)
                {
                    value = dict["test"];
                    failedAfter++;
                }
            }
            catch(Exception e)
            {
                Console.WriteLine($"Failed after {failedAfter} iterations: {e}");
            }
        }
    }
}
