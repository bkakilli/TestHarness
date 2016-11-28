using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestHarness;

namespace SomeNewNamespace
{
    public class Driver4 : ITest
    {
        public bool test()
        {
            SourceCode_4_1.spendSomeTime();
            return true;
        }

        public static void Main(string[] args)
        {

        }
    }
}
