using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ReaderWriterLock;

public class SharedResourceLock : SharedResourceBase
{
    private string Data;
    private readonly object @lock = new object();

    public override void Write(string data)
    {
        lock (@lock)
        {
            Data = data;
            // Console.WriteLine($"поток на запись: записал {Data}");
        }
    }

    public override string Read()
    {
        lock (@lock)
        {
            // Console.WriteLine($"поток на чтение: прочитал {Data}");
            return Data;
        }
    }

    public override long ComputeFactorial(int number)
    {
        return Factorial(number);
    }
}