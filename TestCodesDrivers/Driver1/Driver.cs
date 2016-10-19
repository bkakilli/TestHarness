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
            SourceCode1_1 sourceCode = new SourceCode1_1();
            
            return sourceCode.getFromSource2();
        }

    }

}
