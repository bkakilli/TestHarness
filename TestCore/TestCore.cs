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
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Security.Policy;    // defines evidence needed for AppDomain construction
using System.Reflection;
using System.Runtime.Remoting;
using System.IO;


namespace TestHarness
{
    public class TestCore
    {
        static string TAG = "TestCore";

        bool stop;
        bool running;

        string appLocation;
        string testFolder;
        string repository;
        string logFolder = "Logs";
        BlockingQueue<string> queue;

        public Thread coreThread;
        public Logger logger;

        public TestCore(string repoPath_, Logger logger_)
        {
            appLocation = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            testFolder = @"testFolder";
            repository = repoPath_;

            logger = logger_;

            queue = new BlockingQueue<string>();
            coreThread = new Thread(new ThreadStart(run));
 
        }

        ~TestCore()
        {
            FileManager<string>.removeFolder(Path.Combine(appLocation, testFolder));
        }

        public void Start()
        {
            coreThread.Start();
        }

        public void Stop(bool forceStop = false)
        {
            //coreThread.Abort();
            stop = true;
            queue.enQ("stop command");

            logger.verbose = true;
            if (forceStop)
            {
                Thread.Sleep(100);
                coreThread.Abort();

                try
                {
                    FileManager<string>.removeFolder(Path.Combine(appLocation, testFolder)); ;
                }

                catch (System.IO.IOException ex)
                {
                    Log(TAG, string.Format("{0}\n", ex.Message));
                }
            }
            else if (running)
            {
                FileManager<string>.removeFolder(Path.Combine(appLocation, testFolder));
                Log(TAG, string.Format("Waiting for running tests to finish to quit.\n"));
            }

            logger.verbose = false;

        }

        private void run()
        {
            Console.WriteLine("Core Thread starting...\n");
            while (true)
            {
                running = false;
                string xmlPath = queue.deQ();
                running = true;
                if (stop)
                    break;
                executeRequest(xmlPath);
            }
        }

        public void enQRequest(string testRequest)
        {
            queue.enQ(testRequest);
        }

        private void executeRequest(string xmlFile)
        {
            try
            {
                string domainName = "TestingDomain";   // A unique domain name for each test request
                string libDirectory = Path.GetFullPath(Path.Combine(appLocation, testFolder, domainName));  // Create a lib folder for each test request. This folder will contain subdirectories for each test

                if (!Directory.Exists(libDirectory))
                    Directory.CreateDirectory(libDirectory);

                // Create application domain setup information for new AppDomain
                AppDomainSetup domaininfo = new AppDomainSetup();
                domaininfo.ApplicationBase = appLocation;  // defines search path for assemblies
                domaininfo.PrivateBinPath = libDirectory;

                // Create evidence for the new AppDomain from evidence of current
                Evidence adevidence = AppDomain.CurrentDomain.Evidence;

                // Create Child AppDomain with provided evidence and domain info
                AppDomain childDomain
                  = AppDomain.CreateDomain(domainName, adevidence, domaininfo);

                // Load Tester into the testing domain
                childDomain.Load("Tester");
                ObjectHandle oh = childDomain.CreateInstance("Tester", "TestHarness.Tester");
                Tester tester = oh.Unwrap() as Tester;
                tester.setVerbose(logger.verbose);

                try
                {
                    tester.executeRequest(xmlFile, appLocation, repository, libDirectory);
                }
                catch (Exception ex)
                {
                    Log(TAG, string.Format("Exception is caught during execution of test. Details:\n{0}", ex.Message));
                }

                string logFileName = "test_" + tester.testRequestID;
                string logFile = Path.GetFullPath(Path.Combine(
                    appLocation, repository, logFolder, logFileName + @".log"
                    ));
                FileManager<string>.writeToFile(logFile, tester.getLog());

                Log(TAG, string.Format("Log file saved to: {0}", logFile));

                AppDomain.Unload(childDomain);

                FileManager<string>.removeFolder(libDirectory);
            }
            catch (Exception ex)
            {
                Log(TAG, string.Format("Exeption in executeRequest function. Details:\n{0}", ex.Message));
            }
        }

        private void showAssemblies(AppDomain ad)
        {
            Assembly[] arrayOfAssems = ad.GetAssemblies();
            Log(TAG, string.Format("\n Assembly list in the domain {0}:\n", ad.FriendlyName));
            foreach (Assembly assem in arrayOfAssems)
                Log(TAG, string.Format("\n   -{0}", assem));

            Log(TAG, string.Format("\n\n"));
        }

        public string getQueueElements()
        {
            return queue.ToString();
        }

        public void setVerbose(bool v)
        {
            logger.verbose = v;
        }

        public void Log(string tag, string log)
        {
            logger.Log(tag, log);
        }

        public string getLog(string fileName)
        {
            string logDir = Path.GetFullPath(Path.Combine(
                   appLocation, repository, logFolder
                   ));
            if (!Directory.Exists(logDir))
            {
                Directory.CreateDirectory(logDir);
                Console.WriteLine("logs folder is created in repostiory.");
            }

            string logFile = Path.Combine(logDir, Path.GetFileNameWithoutExtension(fileName) + @".log");
            return FileManager<string>.readFile(logFile);
        }
#if (TestCore_TEST)

        public static void Main(string[] args)
        {
            try
            {
                string repository = args[0];
                Logger logger = new Logger();
                logger.verbose = true;

                TestCore core = new TestCore(repository, logger);

                string xmlFile = @"..\testStubFiles\sampleTestRequest.xml";

                core.Start();

                core.enQRequest(xmlFile);

                Thread.Sleep(200);
                core.Stop();
            }
            catch (Exception ex)
            {
                Console.Write("\n\n  {0}", ex.Message);
            }
        }

#endif
    }

}
