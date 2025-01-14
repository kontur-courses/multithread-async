using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;

namespace LockFree
{
    class VolatileIsTrickyTest
    {
        private volatile bool A;
        private volatile bool B;
        private volatile bool A_Won;
        private volatile bool B_Won;

        void ThreadA()
        {
            A = true;
            if(!B)
                A_Won = true;
        }

        void ThreadB()
        {
            B = true;
            if(!A)
                B_Won = true;
        }

        [Test]
        public void Test()
        {
            for(int i = 0; i < 100_000_000; i++)
            {
                A = B = A_Won = B_Won = false;

                var t1 = Task.Run(ThreadA);
                var t2 = Task.Run(ThreadB);
                Task.WaitAll(t1, t2);

                if(i % 10 == 0)
                    Console.WriteLine($"{i} iterations done");

                if(A_Won && B_Won)
                    throw new Exception("That's impossible!");
            }
        }
    }
}