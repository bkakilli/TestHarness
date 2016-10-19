using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestHarness;

namespace Driver3
{
    public class Driver3 : ITest
    {
        public bool test()
        {
            return SourceCode_3_1.goodFunc();
        }
    }
}
