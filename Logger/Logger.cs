using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestHarness
{
    public class Logger : ILog
    {
        public bool verbose;

        List<string> logList;

        public Logger()
        {
            logList = new List<string>();

            verbose = false;
        }

        public string getLog()
        {
            string log = "";

            foreach(string l in logList)
            {
                log += String.Format("{0}\n", l);
            }

            return log;
        }

        public void Log(string tag, string log)
        {
            log = log.Replace("\n", string.Format("\n{0,-12} : ", tag));
            log = string.Format("{0,-12} : {1}", tag, log);
            logList.Add(log);
            if (verbose)
                Console.WriteLine(log);
        }

        public List<string> getLogList()
        {
            return logList;
        }
    }
}
