using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestHarness
{
    public class Driver2 : ITest
    {

        public bool test()
        {
            SourceCode2_1 sc = new SourceCode2_1();

            return sc.runDivideByZero();
        }

        public static void Main(string[] args)
        {

        }
    }
}
