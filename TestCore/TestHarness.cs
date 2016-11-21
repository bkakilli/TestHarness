////////////////////////////////////////////////////////////////////////////////
//  TestHarness.cs - Schedules test requests and run them in seperate AppDomains //
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
    public class TestHarness
    {
        static string TAG = "TestHarness";

        int threadCount;

        string appLocation;
        string testFolder;
        string repository;
        string logFolder = "Logs";

        BlockingQueue<string> queue;
        ComServer server;
        public Logger logger;

        public Action<string> println;

        public TestHarness(string repoPath_)
        {
            println = str => Console.WriteLine(str);


            appLocation = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            testFolder = @"testFolder";
            repository = repoPath_;

            logger = new Logger();
            logger.verbose = true;

            queue = new BlockingQueue<string>();
            
            server = new ComServer(new RequestHandler(enQRequest));
            server.appLocation = appLocation;

            //ts = new ParameterizedThreadStart();
 
        }

        ~TestHarness()
        {
            FileManager<string>.removeFolder(Path.Combine(appLocation, testFolder));
        }

        private void Start()
        {
            println("Core Thread starting...\n");
            threadCount = 0;
            server.Start();
            // Main loop of the TestHarness
            while (true)
            {
                string xmlFileName = queue.deQ();
                if (xmlFileName == "quit")
                {
                    break;
                }
                // Parse and copy libraries
                TestRequest testRequest = new TestRequest();
                testRequest.xmlPath = Path.Combine(server.getUploadDir(), xmlFileName);

                // Execute the request in a seperate thread. Queue that thread into thread pool.
                threadCount++;
                ThreadPool.QueueUserWorkItem(
                    new WaitCallback(executeRequest), testRequest
                    );
            }

            server.Stop();

            if (threadCount != 0)
            {
                Console.WriteLine("Waiting for running tests to finish to quit...\n");
                Log(TAG, string.Format("Waiting for running tests to finish to quit...\n"));
            }
            while (threadCount != 0)
                Thread.Sleep(500);

            FileManager<string>.removeFolder(Path.Combine(appLocation, testFolder));

            Console.WriteLine("\nTest Harness shutdown.");
        }

        //public void Stop(bool forceStop = false)
        //{
        //    //coreThread.Abort();
        //    stop = true;
        //    queue.enQ("stop command");

        //    logger.verbose = true;
        //    if (forceStop)
        //    {
        //        Thread.Sleep(100);

        //        try
        //        {
        //            FileManager<string>.removeFolder(Path.Combine(appLocation, testFolder));
        //        }

        //        catch (System.IO.IOException ex)
        //        {
        //            Log(TAG, string.Format("{0}\n", ex.Message));
        //        }
        //    }
        //    else if (running)
        //    {
        //        FileManager<string>.removeFolder(Path.Combine(appLocation, testFolder));
        //        Log(TAG, string.Format("Waiting for running tests to finish to quit.\n"));
        //    }

        //    logger.verbose = false;

        //}
        
        public void enQRequest(string testRequest)
        {
            queue.enQ(testRequest);
        }

        private void executeRequest(Object obj)
        {
            TestRequest testRequest = (TestRequest)obj;
            try
            {
                parseAndCopyFiles(testRequest);

                // Create application domain setup information for new AppDomain
                AppDomainSetup domaininfo = new AppDomainSetup();
                domaininfo.ApplicationBase = appLocation;  // defines search path for assemblies
                domaininfo.PrivateBinPath = testRequest.libDirectory;

                // Create evidence for the new AppDomain from evidence of current
                Evidence adevidence = AppDomain.CurrentDomain.Evidence;

                // Create Child AppDomain with provided evidence and domain info
                AppDomain childDomain
                  = AppDomain.CreateDomain(testRequest.ID, adevidence, domaininfo);

                // Load Tester into the testing domain
                childDomain.Load("Tester");
                ObjectHandle oh = childDomain.CreateInstance("Tester", "TestHarness.Tester");
                Tester tester = oh.Unwrap() as Tester;
                tester.setVerbose(logger.verbose);

                try
                {
                    tester.executeRequest(testRequest.serialize());
                }
                catch (Exception ex)
                {
                    Log(TAG, string.Format("Exception is caught during execution of test. Details:\n{0}", ex.Message));
                }

                string logFileName = "test_" + testRequest.ID;
                string logFile = Path.GetFullPath(Path.Combine(
                    appLocation, repository, logFolder, logFileName + @".log"
                    ));
                FileManager<string>.writeToFile(logFile, tester.getLog());

                Log(TAG, string.Format("Log file saved to: {0}", logFile));

                AppDomain.Unload(childDomain);

                FileManager<string>.removeFolder(testRequest.libDirectory);
            }
            catch (Exception ex)
            {
                Log(TAG, string.Format("Exeption in executeRequest function. Details:\n{0}", ex.Message));
            }

            threadCount--;
        }

        private bool parseAndCopyFiles(TestRequest testRequest)
        {
            testRequest.ID = "nonExistingTestRequest";
            //// Parse XML and create test list. For each test in test list, create a child app domain 
            //// and load all files in the test and load libraries into that child app domain.

            FileManager<string> fm = new FileManager<string>(logger);
            //FileManager<string>.removeFolder(Path.Combine(appLocation, libDirectory));

            XMLFactory xf = new XMLFactory(logger);

            string xmlPath = testRequest.xmlPath;
            if (Path.GetExtension(xmlPath) == "")
                Path.ChangeExtension(xmlPath, "xml");

            FileStream xml;
            try
            {
                xml = new FileStream(xmlPath, FileMode.Open);
            }
            catch (FileNotFoundException)
            {
                Log(TAG, string.Format("Test request file is not found in provided path: {0}\n", xmlPath));
                return false;
            }
            catch (Exception)
            {
                Log(TAG, string.Format("Test request file could not be opened.\n", xmlPath));
                return false;
            }

            if (!xf.parse(xml))
            {
                Log(TAG, string.Format("Parse error! Skiping test request.\n"));
                return false;
            }

            if (xf.getTests().Count < 1)
            {
                Log(TAG, string.Format("No tests found in test request! Skiping test request.\n"));
                return false;
            }

            // A unique domain name for each test request
            {   // Block for settign testRequest ID
                Test test = xf.getTests()[0];
                testRequest.ID = test.author + test.timeStamp.ToString("_yyyyMMdd_HHmmss");
                testRequest.ID = testRequest.ID.Replace(" ", "_");
            }


            testRequest.tests = xf.getTests();

            xml.Close();


            // Create a lib folder for each test request. This folder will contain subdirectories for each test
            string libDirectory = Path.GetFullPath(Path.Combine(appLocation, testFolder, testRequest.ID));
            testRequest.libDirectory = libDirectory;

            if (!Directory.Exists(libDirectory))
                Directory.CreateDirectory(libDirectory);

            foreach (Test test in testRequest.tests)
            {
                // Create a unique ID for each test in test request.
                test.ID = string.Format("Test_{0}_{1}",
                    test.timeStamp.ToString("yyyyMMdd_HHmmss"),
                    (test.testName != "") ? test.testName : testRequest.tests.Count.ToString());

                // Create filelist to be copied from repository for each test
                // Note that files in the list are not paths! So search for repository for each of them
                List<string> fileList = new List<string>();
                fileList.Add(test.testDriver);
                foreach (string sourceCode in test.testCode)
                    fileList.Add(sourceCode);

                // Copy all files required for this tests into libDirectory
                List<string> missingFiles = server.fetchFilesFromRepository(fileList, testRequest.libDirectory);
                if (missingFiles.Count != 0)
                {
                    Log(TAG, string.Format("Skipping test {0}.\n", test.ID));
                    return false;
                }
            }


            return true;
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

        public static void Main(string[] args)
        {
            try
            {
                Console.Title = "TestHarness";

                string repository = args[0];

                TestHarness core = new TestHarness(repository);

                Thread th = new Thread(() => core.Start());
                th.Start();

                //Thread.Sleep(1000);

                //core.Stop();

                th.Join();
            }
            catch (Exception ex)
            {
                Console.Write("\n\n  {0}", ex.Message);
            }
        }
        
    }

}
