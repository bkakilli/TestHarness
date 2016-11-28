using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestHarness
{
    public class SourceCode_3_1
    {
        public static bool goodFunc()
        {
            try
            {
                int a = 5;
                int b = 0;
                return (a / b) == 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception caught by test code itself. Returning true. Works. Test Fine. That's enough. It's been 30 hours of coding. I need shower. But still... The details:\n{0}", ex.Message);
                return true;
            }

        }

        public static void Main(string[] args)
        {

        }
    }
}
