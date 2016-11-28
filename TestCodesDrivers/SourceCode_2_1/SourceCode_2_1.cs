using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestHarness
{
    public class SourceCode2_1
    {
        public bool runDivideByZero()
        {
            int a = 5;
            int b = 0;
            return (a / b) == 0;
        }

        public static void Main(string[] args)
        {

        }
    }
}
