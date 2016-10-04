using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestHarness
{
    public interface ILog
    {
        void Log(string log);
        string getLog();
    }
}
