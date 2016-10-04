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
    class TestCore
    {
        static bool verbose = false;

        bool stop;
        bool running;

        string appLocation;
        string testFolder;
        string repoPath;
        FileManager<string> fm;
        BlockingQueue<string> queue;
        ThreadStart childref;
        public Thread coreThread;

        public TestCore(string repoPath_, string testFolder_)
        {
            appLocation = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            testFolder = testFolder_;
            repoPath = repoPath_;

            fm = new FileManager<string>(appLocation, testFolder);
            fm.connectToRepo(repoPath);
            
            queue = new BlockingQueue<string>();

            childref = new ThreadStart(run);
            coreThread = new Thread(childref);
        }

        static void Main()
        {
            TestCore core = new TestCore(@"..\..\..", @"testFolder");
            core.Start();
            Thread.Sleep(100);

            while (true)
            {
                Console.Write("Choose wisely: ");
                string line = Console.ReadLine();
                Console.WriteLine();
                if (line == "exit")
                {
                    Console.WriteLine("Quiting");
                    core.Stop();
                    break;
                }
                    
                else
                    core.queue.enQ(line);
            }

            //core.queue.enQ(@"..\..\..\XMLFactory\TestRequest.xml");

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
            
            if(forceStop)
            {
                Thread.Sleep(100);
                coreThread.Abort();

                try
                {
                    System.IO.Directory.Delete(Path.Combine(appLocation, testFolder), true);
                }

                catch (System.IO.IOException e)
                {
                    Console.WriteLine(e.Message);
                }
            }
            else if (running)
            {
                Console.WriteLine("Waiting for running tests to finish to quit.");
            }

        }

        private void run()
        {
            Console.WriteLine("Core Thread starting...");
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

        private void executeRequest(string xmlFile)
        {
            // Sample request:
            //List<Test> tests = new List<Test>();

            //Test sampleTest = new Test();
            //sampleTest.author = "Burak_Kakillioglu";
            //sampleTest.authorType = "developer";
            //sampleTest.priority = "normal";
            //sampleTest.testCode = new List<string> {@"SourceCode.dll" };
            //sampleTest.testDriver = @"Driver.dll";
            //sampleTest.testName = "testname";
            //sampleTest.timeStamp = DateTime.Now;

            //tests.Add(sampleTest);

            //// Parse XML and create test list. For each test in test list, create a child app domain 
            //// and load all files in the test and load libraries into that child app domain.

            string domainName = "Testing Domain";

            XMLFactory xf = new XMLFactory();

            string xmlPath = Path.GetFullPath(Path.Combine(appLocation, xmlFile));
            System.IO.FileStream xml;
            try
            {
                xml = new System.IO.FileStream(xmlPath, System.IO.FileMode.Open);
            }
            catch (FileNotFoundException ex)
            {
                Console.WriteLine("XML file is not found in provided path: {0}", xmlPath);
                return;
            }
            catch (Exception ex)
            {
                Console.WriteLine("XML file could not be opened.", xmlPath);
                return;
            }
            

            if (!xf.parse(xml))
            {
                Console.WriteLine("Skiping test request.");
                return;
            }

            List<Test> testList = xf.getTests();
            foreach (Test test in testList)
            {
                // Create a unique ID for each test in test request.
                string testID = String.Format("Test_{0}_{1}",
                    test.timeStamp.ToString("yyyyMMdd_HHmmss"),
                    (test.testName != "") ? test.testName : testList.Count.ToString());
                
                Console.WriteLine("\n\n--------- Testing {0} in {1}\n", testID, domainName);

                // Create filelist to be copied from repository
                List<string> fileList = new List<string>();
                fileList.Add(test.testDriver);
                foreach (string sourceCode in test.testCode)
                    fileList.Add(sourceCode);

                // Create the temporary folder for current test to load the libraries from
                if (!fm.createTempFolder(testID, fileList))
                {
                    Console.WriteLine("Skipping test {0}.", testID);
                    continue;
                }

                string libDirectory = fm.getPath(testID);

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

                // Load libraries into 
                if (!tester.LoadLibraries(libDirectory))
                {
                    Console.WriteLine("Error: Could not load libraries.\n");
                    fm.removeTempFolder(testID);
                    continue;
                }

                if (verbose)
                    showAssemblies(AppDomain.CurrentDomain);

                // Logging here.

                string driverName = Path.GetFileNameWithoutExtension(test.testDriver);
                bool testResult;
                if (tester.setupTest(driverName))
                {
                    Console.WriteLine("Testing...");
                    testResult = tester.RunTest();
                    Console.WriteLine("Test {0} has {1}ed", testID, testResult ? "PASS" : "FAIL");
                }

                AppDomain.Unload(childDomain);
                fm.removeTempFolder(testID);
            }

            xml.Close();

        }

        private void showAssemblies(AppDomain ad)
        {
            Assembly[] arrayOfAssems = ad.GetAssemblies();
            Console.WriteLine("\n Assembly list in the domain {0}:", ad.FriendlyName);
            foreach (Assembly assem in arrayOfAssems)
                Console.Write("\n   -{0}", assem);

            Console.WriteLine("\n");
        }

    }

}
