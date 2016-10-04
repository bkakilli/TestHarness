/////////////////////////////////////////////////////////////////////
// XmlTest.cs - Help Session Demonstration of XML Processing       //
//                                                                 //
// Jim Fawcett, CSE681 - Software Modeling and Analysis, F2016     //
/////////////////////////////////////////////////////////////////////
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestHarness
{
    public class Test
    {
        public string testName { get; set; }
        public string author { get; set; }
        public string authorType { get; set; }
        public string priority { get; set; }
        public DateTime timeStamp { get; set; }
        public string testDriver { get; set; }
        public List<string> testCode { get; set; }

        public void show()
        {
            Console.Write("\n  {0,-12} : {1}", "test name", testName);
            Console.Write("\n  {0,12} : {1}", "author", author);
            Console.Write("\n  {0,12} : {1}", "authorType", authorType);
            Console.Write("\n  {0,12} : {1}", "priority", priority);
            Console.Write("\n  {0,12} : {1}", "time stamp", timeStamp);
            Console.Write("\n  {0,12} : {1}", "test driver", testDriver);
            foreach (string library in testCode)
            {
                Console.Write("\n  {0,12} : {1}", "library", library);
            }
            Console.WriteLine();
        }
    }

    public class XMLFactory : ILog
    {
        Logger logger;

        XDocument doc_;
        List<Test> testList_;
        public XMLFactory()
        {
            doc_ = new XDocument();
            testList_ = new List<Test>();
            logger = new Logger("XMLFactory");
        }
        public bool parse(System.IO.Stream xml)
        {
            doc_ = XDocument.Load(xml);
            if (doc_ == null)
            {
                Console.WriteLine("Error: XML document is found but could not been loaded.");
                return false;
            }
            try {
                string author = doc_.Descendants("author").First().Value;
                string authorType = doc_.Descendants("author").First().Attribute("type").Value;
                string priority = doc_.Descendants("priority").First().Value;
                Test test = null;

                XElement[] xtests = doc_.Descendants("test").ToArray();

                if (xtests.Count() < 1)
                {
                    Console.WriteLine("Error: Could not find any test in test request.");
                    return false;
                }

                for (int i = 0; i < xtests.Count(); ++i)
                {
                    test = new Test();
                    test.testCode = new List<string>();
                    test.author = author;
                    test.authorType = author;
                    test.priority = priority;
                    test.timeStamp = DateTime.Now;
                    test.testName = xtests[i].Attribute("name").Value;
                    test.testDriver = xtests[i].Element("testDriver").Value;
                    IEnumerable<XElement> xtestCode = xtests[i].Elements("library");
                    foreach (var xlibrary in xtestCode)
                    {
                        test.testCode.Add(xlibrary.Value);
                    }
                    testList_.Add(test);
                }
            } catch(Exception ex)
            {
                Console.WriteLine("Error: Parse error. Details:\n{0}", ex.Message);
                return false;
            }
            
            return true;
        }

        public List<Test> getTests()
        {
            return testList_;
        }

        public void Log(string log)
        {
            logger.Log(log);
        }

        public string getLog()
        {
            return logger.getLog();
        }

#if (CONSTR_TEST)
        static void Main(string[] args)
        {
            XMLFactory demo = new XMLFactory();
            try
            {
                string path = "../../TestRequest.xml";
                System.IO.FileStream xml = new System.IO.FileStream(path, System.IO.FileMode.Open);
                demo.parse(xml);
                foreach (Test test in demo.testList_)
                {
                    test.show();
                }
            }
            catch (Exception ex)
            {
                Console.Write("\n\n  {0}", ex.Message);
            }
        }
#endif

    }
}
