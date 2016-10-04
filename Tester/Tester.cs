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
        // public string testerName = "No Name";
        // public string SayHello()
        // => "Hello from Tester " + AppDomain.CurrentDomain.FriendlyName;

        public Logger logger;

        ObjectHandle objHandle;
        ITest driver;
        
        public void initLogger(string testID)
        {
            logger = new Logger("Tester " + testID);
        }

        public bool LoadLibraries(string path)
        {
            Console.WriteLine("Loading libraries into child AppDomain {0}...\n", AppDomain.CurrentDomain.FriendlyName);
            string[] libs = Directory.GetFiles(path, "*.dll");
            try
            {
                foreach (string lib in libs)
                {
                    // Console.WriteLine("  Loading {0}", lib);
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

        public bool setupTest(string driverName)
        {
            string typeName = null;
            int iTestFound = 0;

            // Get or find the driver dll name and the type that has ITest interface.
            Assembly[] arrayOfAssems = AppDomain.CurrentDomain.GetAssemblies();
            foreach (Assembly assem in arrayOfAssems)
            {
                string assemblyName = assem.FullName.Split(',')[0];

                if (assemblyName == "mscorlib" || assemblyName == "ITest" || assemblyName == "Tester")
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
                Console.WriteLine("Error: Could not find a type with ITest interface in library {0}", driverName);
                showAssemblies(AppDomain.CurrentDomain);
                return false;
            }
            else if(iTestFound > 1)
            {
                Console.WriteLine("Warning: Multiple types with ITest interface are found in library {0}. Using one of them.", driverName);
            }

            objHandle = AppDomain.CurrentDomain.CreateInstance
                (driverName, typeName);
            driver = (ITest)objHandle.Unwrap();

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
                Console.WriteLine(e.Message);
                testResult = false;
            }

            return testResult;            
        }

        private void showAssemblies(AppDomain ad)
        {
            Assembly[] arrayOfAssems = ad.GetAssemblies();
            Console.WriteLine("\n Assembly list in the domain {0}:\n", ad.FriendlyName);
            foreach (Assembly assem in arrayOfAssems)
            {
                string assemblyName = assem.FullName.Split(',')[0];
                Console.WriteLine("   -{0}", assemblyName);

                if (assemblyName == "mscorlib")
                    continue;

                Console.WriteLine("    Types:");
                Type[] types = assem.GetTypes();
                foreach(Type type in types)
                {
                    Type iTestType = type.GetInterface("ITest");
                    string isITest = (iTestType == null) ? "No" : "Yes";
                    Console.WriteLine("      +{0}. Has ITest interface? {1}", type.FullName, isITest);
                }
            }

            Console.WriteLine("\n");
        }

        public void Log(string log)
        {

        }

        public string getLog()
        {
            throw new NotImplementedException();
        }
    }
}
