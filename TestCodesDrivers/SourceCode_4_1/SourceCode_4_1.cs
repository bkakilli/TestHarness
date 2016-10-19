using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SomeNewNamespace
{
    public class SourceCode_4_1
    {
        public static void spendSomeTime()
        {
            Console.WriteLine("Waiting for 10 secs in test code.");
            Thread.Sleep(10000);
        }
    }
}
