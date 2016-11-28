/////////////////////////////////////////////////////////////////////////////
//  Tester.cs - Receives an XML test request, runs it, and provides log    //
//  ver 0.5                                                                //
//  Language:     C#, VS 2015, .NET Framework 4.5.2                        //
//  Platform:     Windows 10                                               //
//  Application:  Test Harness, CSE681 - Project 2                         //
//  Author:       Burak Kakillioglu, Syracuse University                   //
//                bkakilli@syr.edu                                         //
/////////////////////////////////////////////////////////////////////////////
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
        Logger logger;
        //public string testRequestID;
        string TAG = "Tester";

        ObjectHandle objHandle;
        ITest driver;

        public Tester()
        {
            logger = new Logger();
            Log(TAG, string.Format("The test request is starting to run in a seperate AppDomain {0}",
                AppDomain.CurrentDomain.FriendlyName));
        }

        public bool executeRequest(string serializedTestRequest)
        {

            TestRequest testRequest = TestRequest.deserialize(serializedTestRequest);

            Log(TAG, string.Format("ID of the test request: {0}", testRequest.ID));
            Log(TAG, string.Format("Summary of tests which will be executed in test request:\n"));
            List<Test> testList = testRequest.tests;
            int count = 0;
            foreach (Test test in testList)
            {
                count++;
                Log(TAG, string.Format("Test #{0}:\n{1}\n", count, test.tostr));
            }


            Log(TAG, string.Format("Executing tests in test request one at a time..."));
            foreach (Test test in testList)
            {

                Log(TAG, string.Format("\n--------- Testing {0} in {1}\n",
                    test.ID, AppDomain.CurrentDomain.FriendlyName));
                Log(TAG, string.Format("Test info:\n{0}\n", test.tostr));

                // Load libraries into 
                if (!LoadLibraries(testRequest.libDirectory))
                {
                    Log(TAG, string.Format("Error: Could not load libraries.\n\n"));
                    continue;
                }

                //Log(TAG, getAssemblyList(AppDomain.CurrentDomain));

                // Logging here.

                string driverName = Path.GetFileNameWithoutExtension(test.testDriver);
                bool testResult;
                if (setupTest(driverName))
                {
                    Log(TAG, string.Format("Testing...\n"));
                    testResult = RunTest();
                    Log(TAG, string.Format("Test {0} has {1}ed\n", test.ID, testResult ? "PASS" : "FAIL"));
                }
            }

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
                        typeName = type.FullName;
                }
            }

            if (typeName == null)
            {
                Log(TAG, string.Format("Error: Could not find a type with ITest interface in library {0}", driverName));
                Log(TAG, string.Format("{0}\n", getAssemblyList(AppDomain.CurrentDomain)));
                return false;
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
                foreach (Type type in types)
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

#if (Tester_TEST)
        public static void Main(string[] args)
        {
            try
            {
                Console.Write("\n  Testing Tester Project");
                Console.Write("\n =======================\n");
                Tester tester = new Tester();
                tester.setVerbose(true);

                string xmlFile = args[0];
                string repository = args[1];
                string libDirectory = "testLibDirectory";
                string appLocation = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);

                //tester.executeRequest(xmlFile);

                Console.WriteLine("Testing of 'Tester' project is almost finished. getLog() function will print above once again.");
                Console.WriteLine(tester.getLog());
            }
            catch (Exception ex)
            {
                Console.Write("\n\n  {0}", ex.Message);
            }
        }
#endif
    }
}
