////////////////////////////////////////////////////////////////////////////////
//  TestCore.cs - Schedules test requests and run them in seperate AppDomains //
//  ver 0.5                                                                   //
//  Language:     C#, VS 2015, .NET Framework 4.5.2                           //
//  Platform:     Windows 10                                                  //
//  Application:  Test Harness, CSE681 - Project 2                            //
//  Author:       Burak Kakillioglu, Syracuse University                      //
//                bkakilli@syr.edu                                            //
////////////////////////////////////////////////////////////////////////////////


using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TestHarness
{
    class Client
    {
        static void Main(string[] args)
        {
            string testRequestFolderName = "TestRequests";
            string logFolderName = "Logs";
            string usage =
                "Usage information: \n" +
                "Perform an automated demonstration  a\n" +
                "Show this dialog:                   h or help\n" +
                "List available test requests:       lt\n" +
                "Run a test request:                 <testRequestfileName>\n" +
                "List available log files:           ll\n" +
                "Get log of a specific test request: gl\n" +
                "Toggle verbose:                     v\n" +
                "Show test queue:                    s\n" +
                "Quit program:                       q\n" +
                "Force quit program:                 fq\n";

            string repository;

            if (args.Length < 1)
            {
                Console.Write("Repository path is not provided.\nPlease provide a repository path: ");
                repository = Console.ReadLine();
            }
            else
                repository = args[0];

            Logger logger = new Logger();
            logger.verbose = true;
            TestCore core = new TestCore(repository, logger);
            core.Start();
            Thread.Sleep(100);

            Console.WriteLine(usage);

            while (true)
            {
                Console.Write("Choose wisely: ");
                string line = Console.ReadLine();
                if (line == "q")
                {
                    Console.WriteLine("Quiting");
                    core.Stop();
                    break;
                }
                else if (line == "fq")
                {
                    Console.WriteLine("Force quit.");
                    core.Stop(true);
                    break;
                }
                else if (line == "lt")
                {
                    string testRequestFolder = Path.GetFullPath(Path.Combine(
                        Path.GetDirectoryName(Assembly.GetEntryAssembly().Location),
                        repository, testRequestFolderName
                        ));
                    try
                    {
                        string[] filesFound = Directory.GetFiles(
                        testRequestFolder, "*.xml", SearchOption.TopDirectoryOnly);
                        foreach (string file in filesFound)
                        {
                            Console.WriteLine(Path.GetFileNameWithoutExtension(file));
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Test directory folder could not be read:\n{0}\n", ex.Message);
                    }
                    Console.WriteLine();
                }
                else if (line == "ll")
                {
                    string logFolder = Path.GetFullPath(Path.Combine(
                        Path.GetDirectoryName(Assembly.GetEntryAssembly().Location),
                        repository, logFolderName
                        ));
                    try
                    {
                        string[] filesFound = Directory.GetFiles(
                        logFolder, "*.log", SearchOption.TopDirectoryOnly);
                        foreach (string file in filesFound)
                        {
                            Console.WriteLine(Path.GetFileNameWithoutExtension(file));
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Log directory folder could not be read:\n{0}\n", ex.Message);
                    }
                    Console.WriteLine();
                }
                else if (line == "gl")
                {
                    Console.WriteLine("Enter the log file name: ");
                    string logName = Console.ReadLine();
                    Console.WriteLine(core.getLog(logName));

                }
                else if (line == "v")
                {
                    core.setVerbose(!core.logger.verbose);
                }
                else if (line == "s")
                {
                    Console.WriteLine("Test requests waiting in the queue:\n{0}", core.getQueueElements());
                }
                else if (line == "a")
                {
                    core.enQRequest("TestRequest");
                    core.enQRequest("TestRequest1");
                    core.enQRequest("TestRequest2");
                    core.enQRequest("TestRequest3");
                    core.enQRequest("someNonExistingTestRequest");
                }
                else if (line == "h" || line == "help")
                {
                    Console.WriteLine(usage);
                }

                else
                    core.enQRequest(line);
            }
        }
    }
}
