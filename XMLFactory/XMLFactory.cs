//////////////////////////////////////////////////////////////////////////////
//  XMLFactory.cs - Parses XML files and holds Test enties described in XML //
//  ver 0.5                                                                 //
//  Language:     C#, VS 2015, .NET Framework 4.5.2                         //
//  Platform:     Windows 10                                                //
//  Application:  Test Harness, CSE681 - Project 2                          //
//  Author:       Jim Fawcett and Burak Kakillioglu, Syracuse University    //
//                jfawcett@twcny.rr.com, bkakilli@syr.edu                   //
//////////////////////////////////////////////////////////////////////////////
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.IO;

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

    public class XMLFactory : ILog
    {
        Logger logger;
        string TAG = "XMLFactory";

        XDocument doc_;
        List<Test> testList_;
        public XMLFactory(Logger logger_)
        {
            doc_ = new XDocument();
            testList_ = new List<Test>();
            logger = logger_;

        }
        public bool parse(System.IO.Stream xml)
        {
            doc_ = XDocument.Load(xml);
            if (doc_ == null)
            {
                Log(TAG, "Error: XML document is found but could not been loaded.\n");
                return false;
            }
            Log(TAG, string.Format("XML document is loaded."));
            Log(TAG, string.Format("XML is parsing..."));
            try {
                string author = doc_.Descendants("author").First().Value;
                string authorType = doc_.Descendants("author").First().Attribute("type").Value;
                string priority = doc_.Descendants("priority").First().Value;
                Test test = null;

                XElement[] xtests = doc_.Descendants("test").ToArray();

                if (xtests.Count() < 1)
                {
                    Log(TAG, "Error: Could not find any test in test request.\n");
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
                Log(TAG, string.Format("Error: Parse error. Details:\n{0}\n", ex.Message));
                return false;
            }
            
            return true;
        }

        public List<Test> getTests()
        {
            return testList_;
        }

        public void Log(string tag, string log)
        {
            logger.Log(tag, log);
        }

        public string getLog()
        {
            return logger.getLog();
        }

#if (XMLFactory_TEST)
        static void Main(string[] args)
        {
            Logger logger = new Logger();
            XMLFactory demo = new XMLFactory(logger);
            try
            {
                string appLocation = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
                string xmlFile = args[0];
                string path = Path.GetFullPath(Path.Combine(appLocation, xmlFile));

                Console.WriteLine("XML file is parsing...");
                System.IO.FileStream xml = new System.IO.FileStream(path, System.IO.FileMode.Open);
                demo.parse(xml);
                int count = 0;
                foreach (Test test in demo.testList_)
                {
                    count++;
                    Console.WriteLine("Test #{0}:\n{1}", count, test.ToString());
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
