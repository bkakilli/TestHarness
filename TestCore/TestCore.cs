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
        string logFolder = "logs";
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

            string domainName = "TestingDomain";   // A unique domain name for each test request
            string libDirectory = Path.Combine(appLocation, testFolder, domainName);  // Create a lib folder for each test request. This folder will contain subdirectories for each test
            
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

            tester.executeRequest(xmlFile, appLocation, repository, libDirectory);

            string logFile = Path.GetFullPath(Path.Combine(
                appLocation, repository, logFolder, Path.GetFileNameWithoutExtension(xmlFile) + @".log"
                ));
            FileManager<string>.writeToFile(logFile, tester.getLog());

            AppDomain.Unload(childDomain);

            FileManager<string>.removeFolder(libDirectory);
        }

        private void showAssemblies(AppDomain ad)
        {
            Assembly[] arrayOfAssems = ad.GetAssemblies();
            Log(TAG, string.Format("\n Assembly list in the domain {0}:\n", ad.FriendlyName));
            foreach (Assembly assem in arrayOfAssems)
                Log(TAG, string.Format("\n   -{0}", assem));

            Log(TAG, string.Format("\n\n"));
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
            string logFile = Path.GetFullPath(Path.Combine(
                   appLocation, repository, logFolder, Path.GetFileNameWithoutExtension(fileName) + @".log"
                   ));
            return FileManager<string>.readFile(logFile);
        }
    }

}
