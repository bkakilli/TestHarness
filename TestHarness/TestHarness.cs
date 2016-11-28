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
using System.Runtime.InteropServices;

namespace TestHarness
{
    public class TestHarness
    {
        static string TAG = "TestHarness";

        int threadCount;

        string appLocation;
        string testFolder;
        string logFolder;

        BlockingQueue<TestRequest> queue;
        ComServer server;
        Logger logger;

        public TestHarness()
        {

            appLocation = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            logFolder = Path.Combine(appLocation, @"Logs");
            testFolder = Path.Combine(appLocation, @"testFolder");

            logger = new Logger();
            logger.verbose = true;

            queue = new BlockingQueue<TestRequest>();

            server = new ComServer(new RequestHandler(enQRequest));
            

        }

        private void Start()
        {
            threadCount = 0;
            server.Start();
            server.connectToRepository("http://localhost:4040/RepoChannel");
            // Main loop of the TestHarness
            while (true)
            {
                TestRequest testRequest = queue.deQ();
                if (testRequest.xmlFileName == "quit")
                    break;

                // Parse and copy libraries
                threadCount++;
                testRequest.xmlPath = Path.Combine(server.getUploadDir(), testRequest.xmlFileName);
                
                // Execute the request in a seperate thread. Queue that thread into thread pool.
                ThreadPool.QueueUserWorkItem(
                    new WaitCallback(executeRequest), testRequest
                    );

            }
            server.DisconnectFromSender("repository");
            server.Stop();

            if (threadCount != 0)
            {
                Console.WriteLine("Waiting for running tests to finish to quit...\n");
                Log(TAG, string.Format("Waiting for running tests to finish to quit...\n"));
            }
            while (threadCount != 0)
                Thread.Sleep(500);

            FileManager<string>.removeFolder(testFolder);

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

        public void enQRequest(TestRequest testRequest)
        {
            queue.enQ(testRequest);
        }

        private void executeRequest(Object obj)
        {
            TestRequest testRequest = (TestRequest)obj;
            try
            {
                Console.WriteLine("Starting executing test request {0}", testRequest.xmlFileName);
                HiResTimer hrt = new HiResTimer();
                hrt.Start();
                if (ParseAndCopyFiles(testRequest))
                {
                    AppDomain childDomain = CreateAppDomain(testRequest);
                    // Load Tester into the testing domain
                    childDomain.Load("Tester");
                    ObjectHandle oh = childDomain.CreateInstance("Tester", "TestHarness.Tester");
                    Tester tester = oh.Unwrap() as Tester;
                    tester.setVerbose(logger.verbose);

                    hrt.Stop();
                    testRequest.setupTime = hrt.ElapsedMicroseconds;
                    hrt.Start();
                    try
                    {
                        tester.executeRequest(testRequest.serialize());
                    }
                    catch (Exception ex)
                    {
                        Log(TAG, string.Format("Exception is caught during execution of test. Details:\n{0}", ex.Message));
                    }
                    hrt.Stop();
                    testRequest.executionTime = hrt.ElapsedMicroseconds;
                    string log = tester.getLog();

                    SaveLogToRepository(log, testRequest);
                    AppDomain.Unload(childDomain);
                }
                else
                {
                    Message msg = new Message();
                    msg.command = Message.Command.TestRequestSetupError;
                    msg.text = "Parse and copy files error: " + testRequest.xmlFileName;
                    server.NotifyClient(testRequest.clientID, msg);
                }
                FileManager<string>.removeFolder(testRequest.libDirectory);
            }
            catch (Exception ex)
            {
                Log(TAG, string.Format("Exeption in executeRequest function. Details:\n{0}", ex.StackTrace));
            }

            threadCount--;
        }

        private void SaveLogToRepository(string log, TestRequest testRequest)
        {
            log +=
                          "\nElapsed time for test setup (Including threading and communication time): " + testRequest.setupTime + "ms"
                        + "\nElapsed time for execution: " + testRequest.executionTime + "ms"
                        + "\nTotal elapsed time: " + (testRequest.setupTime + testRequest.executionTime) + "ms";

            Console.WriteLine(log);
            string logFileName = "test_" + testRequest.ID;
            string logFile = Path.Combine(logFolder, logFileName + @".log");
            FileManager<string>.WriteToFile(logFile, log);

            Log(TAG, string.Format("Log file saved to: {0}", logFile));
            server.UploadToRepository(logFile);
        }

        private AppDomain CreateAppDomain(TestRequest testRequest)
        {
            // Create application domain setup information for new AppDomain
            AppDomainSetup domaininfo = new AppDomainSetup();
            domaininfo.ApplicationBase = appLocation;  // defines search path for assemblies
            domaininfo.PrivateBinPath = testRequest.libDirectory;

            // Create evidence for the new AppDomain from evidence of current
            Evidence adevidence = AppDomain.CurrentDomain.Evidence;

            // Create Child AppDomain with provided evidence and domain info
            AppDomain childDomain
              = AppDomain.CreateDomain(testRequest.ID, adevidence, domaininfo);

            return childDomain;
        }

        private bool ParseAndCopyFiles(TestRequest testRequest)
        {
            testRequest.ID = Path.GetFileNameWithoutExtension(testRequest.xmlPath) + DateTime.Now.ToString("_yyyyMMdd_HHmmss");
            //// Parse XML and create test list. For each test in test list, create a child app domain 
            //// and load all files in the test and load libraries into that child app domain.
            
            testRequest.tests = ParseAndGetLists(testRequest);
            if (testRequest.tests == null)
                return false;

            // Create a lib folder for each test request. This folder will contain subdirectories for each test
            string libDirectory = Path.Combine(testFolder, testRequest.ID);
            testRequest.libDirectory = libDirectory;

            if (!Directory.Exists(libDirectory))
                Directory.CreateDirectory(libDirectory);

            string filesToFetch = "";
            foreach (Test test in testRequest.tests)
            {
                // Create a unique ID for each test in test request.
                test.ID = string.Format("Test_{0}_{1}",
                    test.timeStamp.ToString("yyyyMMdd_HHmmss"),
                    (test.testName != "") ? test.testName : testRequest.tests.Count.ToString());
                
                filesToFetch += test.testDriver + ",";
                foreach (string sourceCode in test.testCode)
                    filesToFetch += sourceCode + ",";
            }

            // Copy all files required for this tests into libDirectory
            // First check the cache (which is the download directory)
            string missingFiles = "";
            char[] delimitter = { ',' };
            foreach (string file in filesToFetch.Split(delimitter, StringSplitOptions.RemoveEmptyEntries))
            {
                string sourceFilePath = Path.Combine(server.GetCache(), Path.GetFileName(file));
                string targetFilePath = Path.Combine(testRequest.libDirectory, Path.GetFileName(file));
                if (File.Exists(sourceFilePath))
                    FileManager<string>.copyFile(sourceFilePath, targetFilePath);
                else
                    missingFiles += file + ",";
            }
            
            // Fetch files from repository
            missingFiles = server.FetchFilesFromSource(missingFiles, testRequest.libDirectory);
            if (missingFiles == "")
                return true;

            return false;
        }

        private List<Test> ParseAndGetLists(TestRequest testRequest)
        {
            FileManager<string> fm = new FileManager<string>(logger);

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
                return null;
            }
            catch (Exception ex)
            {
                Log(TAG, string.Format("Test request file could not be opened.\n", xmlPath));
                return null;
            }
            if (!xf.parse(xml))
            {
                Log(TAG, string.Format("Parse error! Skiping test request.\n"));
                return null;
            }

            if (xf.getTests().Count < 1)
            {
                Log(TAG, string.Format("No tests found in test request! Skiping test request.\n"));
                return null;
            }

            // A unique domain name for each test request
            {   // Block for settign testRequest ID
                Test test = xf.getTests()[0];
                testRequest.ID =
                    test.author +
                    Path.GetFileName(testRequest.xmlPath) +
                    test.timeStamp.ToString("_yyyyMMdd_HHmmss");
                testRequest.ID = testRequest.ID.Replace(" ", "_");
            }
            xml.Close();

            return xf.getTests();
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
            if (!Directory.Exists(logFolder))
            {
                Directory.CreateDirectory(logFolder);
                Console.WriteLine("logs folder is created in repostiory.");
            }

            string logFile = Path.Combine(logFolder, Path.GetFileNameWithoutExtension(fileName) + @".log");
            return FileManager<string>.readFile(logFile);
        }

        public static void Main(string[] args)
        {
            try
            {
                Console.Title = "TestHarness";

                TestHarness core = new TestHarness();

                core.Start();

            }
            catch (Exception ex)
            {
                Console.Write("\n\n  {0}", ex.Message);
            }
        }

    }

}
