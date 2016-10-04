using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TestHarness
{
    public class Driver : MarshalByRefObject, ITest
    {
        
        public bool test()
        {
            bool result = true;

            SourceCode sourceCode = new SourceCode(3);
            int fact1 = sourceCode.getFact();
            sourceCode.sourceNo = -2;
            int fact2 = sourceCode.getFact();

            //Console.WriteLine("Factorial: " + fact1);
            Console.WriteLine("Inside driver");
            Thread.Sleep(5000);

            return result;
        }

    }

}
