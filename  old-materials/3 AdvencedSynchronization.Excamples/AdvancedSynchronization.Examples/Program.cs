using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AdvancedSynchronization.Examples
{
    public class Program
    {
        public static void Main(string[] args)
        {
            // InterlockedCompareExchangeDemo();
            // BankDemo1();
            // BankDemoLive();
            // BankDemo2();
        }

        private static int a;
        public static void InterlockedCompareExchangeDemo()
        {
            Interlocked.CompareExchange(ref a, 0, 42);

            int NotSafeCompareExchange(ref int variable, int value, int comparand)
            {
                if (variable == comparand)	
                {
                    variable = value;
                    return comparand;
                }
	
                return variable;
            }
        }
        
        private static void BankDemo1()
        {
            static void SendMoney(BankAccount from, BankAccount to, long value)
            {
                if (from.Rubles < value) 
                    return;
                
                from.Rubles -= value;
                to.Rubles += value;
            }
            
            for (var i = 0; i < 42; i++)
            {
                var vasya = new BankAccount("Vasya") {Rubles = 100};            
                var petya = new BankAccount("Petya");
                var sasha = new BankAccount("Sasha");

                var thread1 = StartBckThread(() =>
                {
                    lock (vasya)
                    {
                        lock (petya)
                        {
                            SendMoney(vasya, petya, 100);
                        }
                    }
                });
                
                var thread2 = StartBckThread(() =>
                {
                    lock (vasya)
                    {
                        lock (sasha)
                        {
                            SendMoney(vasya, sasha, 100);
                        }
                    }
                });

                thread1.Join();
                thread2.Join();

                Console.WriteLine($"ATTEMPT #{i:00}: {vasya}, {petya}, {sasha}");
            }
        }
        
        private static void BankDemoLive()
        {
            static void SendMoney(BankAccount from, BankAccount to, long value)
            {
                var spinWait = new SpinWait();
                while (true)
                {
                    var fromRubles = from.Rubles;
                
                    if (fromRubles < value) 
                        return;

                    if (Interlocked.CompareExchange(ref from.Rubles, fromRubles - value, fromRubles) == fromRubles)
                        break;

                    spinWait.SpinOnce();
                }

                Interlocked.Add(ref to.Rubles, value);
            }
            
            for (var i = 0; i < 42; i++)
            {
                var vasya = new BankAccount("Vasya") {Rubles = 100};            
                var petya = new BankAccount("Petya");
                var sasha = new BankAccount("Sasha");

                var thread1 = StartBckThread(() =>
                {
                    SendMoney(vasya, petya, 100);
                });
                
                var thread2 = StartBckThread(() =>
                {
                    SendMoney(vasya, sasha, 100);
                });

                thread1.Join();
                thread2.Join();

                Console.WriteLine($"ATTEMPT #{i:00}: {vasya}, {petya}, {sasha}");
            }
        }
        
        private static void BankDemo2()
        {
            static void SendMoney(BankAccount from, BankAccount to, long value)
            {
                var wait = new SpinWait();

                while (true)
                {
                    var fromRubles = from.Rubles;
                    if (fromRubles < value)
                        return;

                    if (Interlocked.CompareExchange(ref from.Rubles, fromRubles - value, fromRubles) == fromRubles)
                        break;
                    
                    wait.SpinOnce();
                }

                Interlocked.Add(ref to.Rubles, value);
            }
            
            for (var i = 0; i < 42; i++)
            {
                var vasya = new BankAccount("Vasya") {Rubles = 100};            
                var petya = new BankAccount("Petya");
                var sasha = new BankAccount("Sasha");

                var thread1 = StartBckThread(() =>
                {
                    SendMoney(vasya, petya, 100);
                });
                
                var thread2 = StartBckThread(() =>
                {
                    SendMoney(vasya, sasha, 100);
                });

                thread1.Join();
                thread2.Join();

                Console.WriteLine($"ATTEMPT #{i:00}: {vasya}, {petya}, {sasha}");
            }
        }

        private static void ArithmeticDemo()
        {
            var x = 0;
            
            Interlocked.Add(ref x, -42); x += -42;
            Interlocked.Increment(ref x); x++;
            Interlocked.Decrement(ref x); x--;
        }

        private static long x;
        private static long variable;
        private static void ReadWriteDemo()
        {
            var value = Interlocked.Read(ref x);
            
            var newValue = 10;
            var oldValue = Interlocked.Exchange(ref variable, newValue);
        }

        private static void QueueDemo()
        {
            var queue = new Queue<int>();
            queue.Enqueue(1);
            if (queue.Any())
                Console.WriteLine(queue.Dequeue());
            
            var concurrentQueue = new ConcurrentQueue<int>();
            concurrentQueue.Enqueue(1);
            if (concurrentQueue.TryDequeue(out var item))
                Console.WriteLine(item);
            
            var stack = new Stack<int>();
            stack.Push(1);
            if (stack.Any())
                Console.WriteLine(stack.Pop());
            
            var concurrentStack = new ConcurrentStack<int>();
            concurrentStack.Push(1);
            if (concurrentStack.TryPop(out item))
                Console.WriteLine(item);
        }

        private static void DictionaryDemo()
        {
            var concurrentDictionary = new ConcurrentDictionary<int, int>();
            var dictionary = new Dictionary<int, int>();

            // ====================================================
            concurrentDictionary.TryUpdate(1, 3, 2);

            if (dictionary.TryGetValue(1, out var value) && value == 2)
                dictionary[1] = value;
            // ====================================================
            
            // ====================================================
            var result = concurrentDictionary.GetOrAdd(1, 2);

            if (!dictionary.TryGetValue(1, out result))
                result = dictionary[1] = 2;
            // ====================================================
            
            // ====================================================
            concurrentDictionary.AddOrUpdate(1, key => 2, (key, oldValue) => oldValue + 1);

            if (!dictionary.TryGetValue(1, out var oldValue))
                dictionary[1] = 2;
            else
                dictionary[1] = oldValue + 1;
            // ====================================================

            foreach (var (key, v) in concurrentDictionary)
            {
                if (key == 42)
                    concurrentDictionary.TryRemove(key, out _);
            }
        }

        private static Thread StartBckThread(Action action)
        {
            var thread = new Thread(() => action())
            {
                IsBackground = true
            };

            thread.Start();
            
            return thread;
        }
    }
}