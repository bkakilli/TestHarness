using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestHarness
{
    public class SourceCode
    {
        public int sourceNo;

        public SourceCode(int sourceInput = 5)
        {
            sourceNo = sourceInput;
        }

        public int getFact()
        {
            return fact(sourceNo);
        }

        private int fact(int n)
        {
            int result = 0;
            if (n == 1)
                return 1;
            if (n > 1)
                return result = n * fact(n - 1);
            else
                return n;
        }
        
    }
}
