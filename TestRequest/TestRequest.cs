using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestHarness
{
    public class TestRequest : MarshalByRefObject
    {
        public string xmlPath; // Absolute path
        public List<Test> tests;
        public string libDirectory; // Absolute path
        public string testName;
        public string author;
        public string ID;

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
        
    }

    public class Test
    {
        public string ID;
        public string testName { get; set; }
        public string author { get; set; }
        public string authorType { get; set; }
        public string priority { get; set; }
        public DateTime timeStamp { get; set; }
        public string testDriver { get; set; }
        public List<string> testCode { get; set; }
        public string tostr { get; set; }

        override public string ToString()
        {
            string res = "";
            res += string.Format("\n  {0,-12} : {1}", "test name", testName);
            res += string.Format("\n  {0,12} : {1}", "author", author);
            res += string.Format("\n  {0,12} : {1}", "authorType", authorType);
            res += string.Format("\n  {0,12} : {1}", "priority", priority);
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
}
