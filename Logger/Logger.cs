using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestHarness
{
    public class Logger : ILog
    {
        List<string> logList;
        string tag;

        public Logger(string tag_)
        {
            tag = tag_;
            logList = new List<string>();
        }

        public string getLog()
        {
            string log = "";

            foreach(string l in logList)
            {
                log += String.Format("{0}\t:{1}\n", tag, l);
            }

            return log;
        }

        public void Log(string log)
        {
            logList.Add(log);
        }

        public List<string> getLogList()
        {
            return logList;
        }

        public void appendLog(List<string> newList)
        {
            logList.AddRange(newList);
        }
    }
}
