using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Remoting;
using System.Text;
using System.Threading.Tasks;

namespace TestHarness

{
    public class Tester : MarshalByRefObject, ILog
    {
        public Logger logger;
        string TAG = "Tester";

        FileManager<string> fm;
        ObjectHandle objHandle;
        ITest driver;

        public Tester()
        {
            logger = new Logger();
            Log(TAG, string.Format("The test request is starting to run in a seperate AppDomain {0}",
                AppDomain.CurrentDomain.FriendlyName));
        }
        
        public bool executeRequest(string xmlFile, string appLocation, string repository, string libDirectory)
        {
            //// Parse XML and create test list. For each test in test list, create a child app domain 
            //// and load all files in the test and load libraries into that child app domain.
            
            fm = new FileManager<string>(appLocation, libDirectory, logger);
            fm.connectToRepo(repository);
            //FileManager<string>.removeFolder(Path.Combine(appLocation, libDirectory));

            XMLFactory xf = new XMLFactory(logger);

            string xmlPath = Path.GetFullPath(Path.Combine(appLocation, xmlFile));
            System.IO.FileStream xml;
            try
            {
                xml = new System.IO.FileStream(xmlPath, System.IO.FileMode.Open);
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
                Log(TAG, string.Format("Skiping test request.\n"));
                return false;
            }

            Log(TAG, string.Format("\nSummary of tests which will be executed in test request:\n"));
            List<Test> testList = xf.getTests();
            int count = 0;
            foreach(Test test in testList)
            {
                count++;
                Log(TAG, string.Format("Test #{0}:\n{1}\n", count, test.ToString()));
            }


            Log(TAG, string.Format("Executing tests in test request one at a time..."));
            foreach (Test test in testList)
            {
                // Create a unique ID for each test in test request.
                string testID = string.Format("Test_{0}_{1}",
                    test.timeStamp.ToString("yyyyMMdd_HHmmss"),
                    (test.testName != "") ? test.testName : testList.Count.ToString());

                Log(TAG, string.Format("\n--------- Testing {0} in {1}\n",
                    testID, AppDomain.CurrentDomain.FriendlyName));
                Log(TAG, string.Format("Test info:\n{0}\n", test.ToString()));

                // Create filelist to be copied from repository
                List<string> fileList = new List<string>();
                fileList.Add(test.testDriver);
                foreach (string sourceCode in test.testCode)
                    fileList.Add(sourceCode);

                // Create the temporary folder for current test to load the libraries from
                if (!fm.copyLibraries(testID, fileList))
                {
                    Log(TAG, string.Format("Skipping test {0}.\n", testID));
                    continue;
                }        

                // Load libraries into 
                if (!LoadLibraries(libDirectory))
                {
                    Log(TAG, string.Format("Error: Could not load libraries.\n\n"));
                    continue;
                }

                Log(TAG, getAssemblyList(AppDomain.CurrentDomain));

                // Logging here.

                string driverName = Path.GetFileNameWithoutExtension(test.testDriver);
                bool testResult;
                if (setupTest(driverName))
                {
                    Log(TAG, string.Format("Testing...\n"));
                    testResult = RunTest();
                    Log(TAG, string.Format("Test {0} has {1}ed\n", testID, testResult ? "PASS" : "FAIL"));
                }
            }

            xml.Close();

            return true;
        }

        public bool RunTest()
        {
            bool testResult = false;

            try
            {
                testResult = driver.test();
            }
            catch (Exception e)
            {
                Log(TAG, string.Format("Exception caught in Test Driver:\n{0}", e.Message));
                testResult = false;
            }

            return testResult;
        }

        public bool setupTest(string driverName)
        {
            string typeName = null;
            int iTestFound = 0;

            // Get or find the driver dll name and the type that has ITest interface.
            Assembly[] arrayOfAssems = AppDomain.CurrentDomain.GetAssemblies();
            foreach (Assembly assem in arrayOfAssems)
            {
                string assemblyName = assem.FullName.Split(',')[0];

                if (assemblyName == "mscorlib"
                    || assemblyName == "Microsoft.VisualStudio.HostingProcess.Utilities"
                    || assemblyName == "Tester"
                    || assemblyName == "Logger"
                    || assemblyName == "XMLFactory"
                    || assemblyName == "FileManager"
                    || assemblyName == "System.Xml.Linq"
                    || assemblyName == "System.Xml"
                    || assemblyName == "System.Core"
                    || assemblyName == "System")
                    continue;

                Type[] types = assem.GetTypes();
                foreach (Type type in types)
                {
                    Type iTestType = type.GetInterface("ITest");
                    if (iTestType != null && driverName == assemblyName)
                    {
                        typeName = type.FullName;
                        iTestFound++;
                    }
                }
            }

            if(iTestFound == 0)
            {
                Log(TAG, string.Format("Error: Could not find a type with ITest interface in library {0}", driverName));
                Log(TAG, string.Format("{0}\n", getAssemblyList(AppDomain.CurrentDomain)));
                return false;
            }
            else if(iTestFound > 1)
            {
                Log(TAG, string.Format("Warning: Multiple types with ITest interface are found in library {0}. Using the last one.", driverName));
            }
            
            try
            {
                objHandle = AppDomain.CurrentDomain.CreateInstance
                    (driverName, typeName);
                driver = (ITest)objHandle.Unwrap();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            
            return true;
        }

        public bool LoadLibraries(string path)
        {
            Log(TAG, string.Format("Loading libraries into child AppDomain {0}...\n", AppDomain.CurrentDomain.FriendlyName));
            string[] libs = Directory.GetFiles(path, "*.dll");
            try
            {
                foreach (string lib in libs)
                {
                    Assembly.LoadFile(Path.GetFullPath(lib));
                }

                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return false;
            }

        }

        private string getAssemblyList(AppDomain ad)
        {
            string res = "";

            Assembly[] arrayOfAssems = ad.GetAssemblies();
            res += string.Format("Assembly list in the domain {0}:\n", ad.FriendlyName);

            foreach (Assembly assem in arrayOfAssems)
            {
                string assemblyName = assem.FullName.Split(',')[0];

                if (assemblyName == "mscorlib"
                    || assemblyName == "Microsoft.VisualStudio.HostingProcess.Utilities"
                    || assemblyName == "Tester"
                    || assemblyName == "Logger"
                    || assemblyName == "XMLFactory"
                    || assemblyName == "FileManager"
                    || assemblyName == "System.Xml.Linq"
                    || assemblyName == "System.Xml"
                    || assemblyName == "System.Core"
                    || assemblyName == "System")
                    continue;

                res += string.Format("   -{0}\n", assemblyName);
                res += string.Format("    Types:\n");
                Type[] types = assem.GetTypes();
                foreach(Type type in types)
                {
                    Type iTestType = type.GetInterface("ITest");
                    string isITest = (iTestType == null) ? "No" : "Yes";
                    res += string.Format("      +{0}. Has ITest interface? {1}\n", type.FullName, isITest);
                }
            }

            return res;
        }

        public void setVerbose(bool v)
        {
            logger.verbose = v;
        }

        public void Log(string tag, string log)
        {
            logger.Log(tag, log);
        }

        public string getLog()
        {
            return logger.getLog();
        }
    }
}
