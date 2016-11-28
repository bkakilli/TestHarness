using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestHarness
{

    public class Test
    {
        public string ID { get; set; }
        public string testName { get; set; }
        public string author { get; set; }
        public DateTime timeStamp { get; set; }
        public string testDriver { get; set; }
        public List<string> testCode { get; set; }
        public string tostr { get; set; }

        override public string ToString()
        {
            string res = "";
            res += string.Format("\n  {0,-12} : {1}", "test name", testName);
            res += string.Format("\n  {0,12} : {1}", "author", author);
            res += string.Format("\n  {0,12} : {1}", "time stamp", timeStamp);
            res += string.Format("\n  {0,12} : {1}", "test driver", testDriver);
            foreach (string library in testCode)
            {
                res += string.Format("\n  {0,12} : {1}", "library", library);
            }
            res += string.Format("\n");

            return res;
        }
    }

    public class TestRequest : MarshalByRefObject
    {
        public string xmlFileName { get; set; }
        public string xmlPath { get; set; } // Absolute path
        public List<Test> tests { get; set; }
        public string libDirectory { get; set; } // Absolute path
        public string testName { get; set; }
        public string author { get; set; }
        public string ID { get; set; }
        public string clientID { get; set; }

        public ulong setupTime { get; set; }
        public ulong executionTime { get; set; }

        public string serialize()
        {
            string serialized = "";

            foreach(Test test in tests)
            {
                serialized += "!!!!!" + test.ID + "!!!" + test.testDriver + "!!!" + test.ToString();
            }
            serialized = ID + "!!!!!!!" + libDirectory + "!!" + serialized;
            
            return serialized;
        }

        public static TestRequest deserialize(string serialized)
        {
            TestRequest tr = new TestRequest();
            tr.tests = new List<Test>();

            string[] stringSeparators = new string[] { "!!!!!!!" };
            string[] result;
            result = serialized.Split(stringSeparators,
                                  StringSplitOptions.RemoveEmptyEntries);

            tr.ID = result[0];
            tr.libDirectory = result[1];

            string tests = result[2];
            stringSeparators = new string[] { "!!!!!" };
            result = tests.Split(stringSeparators,
                                  StringSplitOptions.RemoveEmptyEntries);

            stringSeparators = new string[] { "!!!" };
            string[] testElems;
            foreach (string t in result)
            {
                testElems = t.Split(stringSeparators,
                                  StringSplitOptions.RemoveEmptyEntries);
                Test test = new Test();
                test.ID = testElems[0];
                test.testDriver = testElems[1];
                test.tostr = testElems[2];

                tr.tests.Add(test);
            }

            return tr;
        }

        public static void Main(string[] args)
        {
            TestRequest tr = new TestRequest();
            tr.author = "Burak Kakillioglu";
            tr.tests = new List<Test>();

            Test t1 = new Test();
            t1.testName = "test1";
            t1.testCode = new List<string>();
            t1.testCode.Add("source1");
            t1.testCode.Add("source2");
            t1.testDriver = "driver1";
            tr.tests.Add(t1);

            string serialized = tr.serialize();
            Console.WriteLine(serialized);
        }
        
    }
}
