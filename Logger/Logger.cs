/////////////////////////////////////////////////////////////////////////////
//  Logger.cs - Stores and organizes logs in application with TAGs         //
//  ver 0.5                                                                //
//  Language:     C#, VS 2015, .NET Framework 4.5.2                        //
//  Platform:     Windows 10                                               //
//  Application:  Test Harness, CSE681 - Project 2                         //
//  Author:       Burak Kakillioglu, Syracuse University                   //
//                bkakilli@syr.edu                                         //
/////////////////////////////////////////////////////////////////////////////
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TestHarness
{
    public class Logger : ILog
    {
        public bool verbose { get; set; }

        List<string> logList;

        public Logger()
        {
            logList = new List<string>();

            verbose = false;
        }

        public string getLog()
        {
            string log = "";

            foreach (string l in logList)
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

#if (Logger_TEST)

        public static void Main(string[] args)
        {
            try
            {
                Console.Write("\n  Testing Logger Project");
                Console.Write("\n =======================\n");

                string TAG = "LoggerTest";
                Logger logger = new Logger();

                logger.verbose = false;

                logger.Log(TAG, "Sample log 1");
                logger.Log(TAG, "Sample log 2");
                logger.Log(TAG, "Sample log 3");

                Console.WriteLine(logger.getLog());

                logger.verbose = true;
                logger.Log(TAG, "Sample log 4");
                logger.Log(TAG, "Sample log 5");

                Console.WriteLine(logger.getLog());
                Console.WriteLine("\nPrinting logs from the list");

                foreach (string log in logger.getLogList())
                    Console.WriteLine(log);
            }
            catch (Exception ex)
            {
                Console.Write("\n\n  {0}", ex.Message);
            }

        }
#endif
    }
}
