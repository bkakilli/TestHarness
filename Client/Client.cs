using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TestHarness
{
    class Client
    {
        static void Main(string[] args)
        {
            string usage =
                "Usage information: \n" +
                "Show this dialog:                   h or help\n" +
                "Send a test request:                relative\\path\\to\\testrequest.xml\n" +
                "Get log of a specific test request: l\n" +
                "Quit program:                       q\n" +
                "Force quit program:                 fq\n";

            string repostiory;

            if (args.Length < 1)
            {
                Console.Write("Repository path is not provided.\nPlease provide a repository path: ");
                repostiory = Console.ReadLine();
            }
            else
                repostiory = args[0];

            Logger logger = new Logger();
            TestCore core = new TestCore(repostiory, logger);
            core.Start();
            Thread.Sleep(100);

            Console.WriteLine(usage);

            while (true)
            {
                Console.Write("Choose wisely: ");
                string line = Console.ReadLine();
                Console.WriteLine();
                if (line == "q")
                {
                    Console.WriteLine("Quiting");
                    core.Stop();
                    break;
                }
                if (line == "fq")
                {
                    Console.WriteLine("Force quit.");
                    core.Stop(true);
                    break;
                }
                else if (line == "l")
                {
                    Console.WriteLine("Enter the log file name: ");
                    string logName = Console.ReadLine();
                    Console.WriteLine(core.getLog(logName));
                }
                else if (line == "v")
                {
                    core.setVerbose(!core.logger.verbose);
                }
                else if (line == "h" || line == "help")
                {
                    Console.WriteLine(usage);
                }

                else
                    core.enQRequest("TestRequest.xml");
            }
        }
    }
}
