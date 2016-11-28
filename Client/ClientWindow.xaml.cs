using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace TestHarness
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>

    public delegate void LogListCallBack(string logList);
    public delegate void LogFileCallBack(string logList);
    

    public class TestFile
    {
        public string FileName { get; set; }
        public string FileType { get; set; }
    }

    public partial class ClientWindow : Window
    {

        List<string> testList = new List<string>();
        List<TestFile> fileList = new List<TestFile>();
        string appLocation;
        string clientLibFolder;
        Client client;

        TestRequest currentTestRequest;
        public ClientWindow()
        {
            InitializeComponent();

            Console.Title = "Client";

            currentTestRequest = new TestRequest();
            TestListBox.Items.Clear();
            FilesDataGrid.ItemsSource = fileList;
            TestListBox.ItemsSource = testList;

            appLocation = System.IO.Path.GetFullPath(
                System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location
                ));

            clientLibFolder = System.IO.Path.Combine(appLocation, @"ClientLibs");
            if (!Directory.Exists(clientLibFolder))
                Directory.CreateDirectory(clientLibFolder);
            
            // Automated demo thread start
            Thread demo = new Thread(new ThreadStart(AutomatedDemo));
            demo.Start();

        }

        private void AutomatedPart1()
        {
            Console.WriteLine("Automated demo started.");
            Thread.Sleep(3000);
            Dispatcher.Invoke(() =>
            {
                TestHarnessCommunicationUrlTextBox.Text = "";
                TestHarnessStreamUrlTextBox.Text = "";
                RepositoryStreamUrlTextBox.Text = "";
            });

            Console.WriteLine("Switch to connection tab");
            Thread.Sleep(2000);
            Dispatcher.Invoke(() => tabControl1.SelectedIndex = 1);
            Console.WriteLine("Entering url information...");

            Thread.Sleep(2000);
            Dispatcher.Invoke(() => TestHarnessCommunicationUrlTextBox.Text = "http://localhost:4040/TestHarnessChannel");
            Thread.Sleep(2000);
            Dispatcher.Invoke(() => TestHarnessStreamUrlTextBox.Text = "http://localhost:8000/TestHarnessStreamService");
            Thread.Sleep(2000);
            Dispatcher.Invoke(() => RepositoryStreamUrlTextBox.Text = "http://localhost:8000/RepositoryStreamService");

            Console.WriteLine("Connecting to servers");
            Thread.Sleep(2000);
            Dispatcher.Invoke(() => ConnectButton.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent)));

            Console.WriteLine("Switch to testing tab");
            Thread.Sleep(2000);
            Dispatcher.Invoke(() => tabControl1.SelectedIndex = 0);

            Console.WriteLine("Enter test request name");
            Thread.Sleep(2000);
            Dispatcher.Invoke(() => XMLFileNameTextBox.Text = "DemoTestRequest1");

            Console.WriteLine("Enter author name");
            Thread.Sleep(2000);
            Dispatcher.Invoke(() => AuthorNameTextBox.Text = "Burak Kakillioglu");
        }

        private void AutomatedPart2()
        {
            Thread.Sleep(500*client.clientCount);

            Console.WriteLine("Add some tests");
            Thread.Sleep(2000);
            Dispatcher.Invoke(() => AddTestTextBox.Text = "Demo Test 1");
            Thread.Sleep(1000);
            Dispatcher.Invoke(() => AddTestButton.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent)));
            Console.WriteLine("Add test files");
            fileList.Clear();

            // Add driver
            Thread.Sleep(1000);
            currentTestRequest.tests[0].testDriver = "Driver.dll";
            fileList.Add(new TestFile()
            {
                FileName = currentTestRequest.tests[0].testDriver,
                FileType = "Test Driver"
            });
            Dispatcher.Invoke(() => {
                TestListBox.SelectedIndex = 0;
                UpdateFileList();
            });

            // Add source
            Thread.Sleep(1000);
            currentTestRequest.tests[0].testCode.Add("SourceCode.dll");
            fileList.Add(new TestFile()
            {
                FileName = currentTestRequest.tests[0].testCode[0],
                FileType = "Source File"
            });
            Dispatcher.Invoke(() => {
                TestListBox.SelectedIndex = 0;
                UpdateFileList();
            });

            // Add source
            Thread.Sleep(1000);
            currentTestRequest.tests[0].testCode.Add("SourceCode1_2.dll");
            fileList.Add(new TestFile()
            {
                FileName = currentTestRequest.tests[0].testCode[1],
                FileType = "Source File"
            });
            Dispatcher.Invoke(() => {
                TestListBox.SelectedIndex = 0;
                UpdateFileList();
            });
        }

        private void AutomatedPart3()
        {
            Thread.Sleep(2000);
            Console.WriteLine("Add another test");
            Thread.Sleep(2000);
            Dispatcher.Invoke(() => AddTestTextBox.Text = "Demo Test 2");
            Thread.Sleep(1000);
            Dispatcher.Invoke(() => AddTestButton.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent)));
            fileList.Clear();

            // Add driver
            Thread.Sleep(1000);
            currentTestRequest.tests[1].testDriver = "Driver_2.dll";
            fileList.Add(new TestFile()
            {
                FileName = currentTestRequest.tests[1].testDriver,
                FileType = "Test Driver"
            });
            Dispatcher.Invoke(() => {
                TestListBox.SelectedIndex = 1;
                UpdateFileList();
            });

            // Add source
            Thread.Sleep(1000);
            currentTestRequest.tests[1].testCode.Add("SourceCode_2_1.dll");
            fileList.Add(new TestFile()
            {
                FileName = currentTestRequest.tests[1].testCode[0],
                FileType = "Source File"
            });
            Dispatcher.Invoke(() => {
                TestListBox.SelectedIndex = 1;
                UpdateFileList();
            });
        }

        private void AutomatedPart4()
        {
            Thread.Sleep(2000);
            Console.WriteLine("Add last test");
            Thread.Sleep(2000);
            Dispatcher.Invoke(() => AddTestTextBox.Text = "Demo Test 3");
            Thread.Sleep(1000);
            Dispatcher.Invoke(() => AddTestButton.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent)));
            fileList.Clear();

            // Add driver
            Thread.Sleep(1000);
            currentTestRequest.tests[2].testDriver = "Driver3.dll";
            fileList.Add(new TestFile()
            {
                FileName = currentTestRequest.tests[2].testDriver,
                FileType = "Test Driver"
            });
            Dispatcher.Invoke(() => {
                TestListBox.SelectedIndex = 2;
                UpdateFileList();
            });

            // Add source
            Thread.Sleep(1000);
            currentTestRequest.tests[2].testCode.Add("SourceCode_3_1.dll");
            fileList.Add(new TestFile()
            {
                FileName = currentTestRequest.tests[2].testCode[0],
                FileType = "Source File"
            });
            Dispatcher.Invoke(() => {
                TestListBox.SelectedIndex = 2;
                UpdateFileList();
            });
        }

        private void AutomatedDemo()
        {
            AutomatedPart1();
            AutomatedPart2();
            AutomatedPart3();
            AutomatedPart4();
            
            // Send Test Request
            Thread.Sleep(1000);
            Console.WriteLine("Sending test request to test harness");
            Thread.Sleep(1000);
            Dispatcher.Invoke(() => SendTestRequestButton.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent)));

            Thread.Sleep(2000);
            Console.WriteLine("Switch to Test Results tab");
            Thread.Sleep(2000);
            Dispatcher.Invoke(() => tabControl1.SelectedIndex = 2);

            Thread.Sleep(2000);
            Console.WriteLine("List log files");
            Thread.Sleep(1000);
            Dispatcher.Invoke(() => ListLogButton.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent)));

            Thread.Sleep(1000);
            Console.WriteLine("Display the first log file");
            Thread.Sleep(2000);
            Dispatcher.Invoke(() => LogFileNameTextBox.Text = LogResultsTextBox.Text.Split('\n')[0]);
            Dispatcher.Invoke(() => DisplayLogButton.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent)));


            Thread.Sleep(1000);
            Console.WriteLine("You can now take the control.");
        }

        private void SendTestRequestButtonHandler(object sender, RoutedEventArgs e)
        {
            if (client == null)
                return;
            // Add author name to test request
            currentTestRequest.author = AuthorNameTextBox.Text;

            // Create XML file from form-generated test request
            string xmlFileName = xmlFileName= client.clientCount + XMLFileNameTextBox.Text.Replace(" ", "_") + ".xml";
            string xmlPath = System.IO.Path.Combine(clientLibFolder, xmlFileName);
            currentTestRequest.xmlPath = xmlPath;
            //FileManager<string>.WriteToFile(xmlPath, xml);

            if(File.Exists(xmlPath))
                File.Delete(xmlPath);
            XMLFactory.Serialize(currentTestRequest);

            // Send all files described in each test to repository
            foreach (Test test in currentTestRequest.tests)
            {
                string filePath = System.IO.Path.Combine(clientLibFolder, test.testDriver);
                client.UploadToRepository(filePath);
                foreach (string filename in test.testCode)
                {
                    filePath = System.IO.Path.Combine(clientLibFolder, filename);
                    client.UploadToRepository(filePath);
                }
            }

            // Send the XML file to the Test Harness Server
            client.UploadToTestHarness(currentTestRequest.xmlPath);
            Thread.Sleep(100);

            // Send Test Request Message to Test Harness
            Message msg = new Message();
            msg.command = Message.Command.TestRequest;
            msg.text = xmlFileName;
            client.SendMessageToTestHarness(msg);

            Console.WriteLine("Test request {0} sent", xmlFileName);

        }

        private void TestListSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (TestListBox.Items.Count == 0)
                return;

            if(e.AddedItems.Count == 0)
                TestListBox.SelectedIndex = 0;

            UpdateFileList();
        }

        private void AddTestButtonHandler(object sender, RoutedEventArgs e)
        {
            if (AuthorNameTextBox.Text == "")
                return;

            if (AddTestTextBox.Text == "")
                return;

            if (testList.Contains(AddTestTextBox.Text))
                return;

            if (currentTestRequest.tests == null)
                currentTestRequest.tests = new List<Test>();

            Test test = new Test();
            test.testName = AddTestTextBox.Text;
            test.testDriver = "";
            test.testCode = new List<string>();

            currentTestRequest.tests.Add(test);

            testList.Add(test.testName);
            TestListBox.Items.Refresh();
            AddTestTextBox.Text = "";
        }

        private void UpdateTestList()
        {
            foreach (Test test in currentTestRequest.tests)
                testList.Add(test.testName);
        }

        private void ClearTestsButtonHandler(object sender, RoutedEventArgs e)
        {
            currentTestRequest.tests.Clear();
            testList.Clear();
            TestListBox.Items.Refresh();

            fileList.Clear();
            FilesDataGrid.Items.Refresh();
        }

        private void RemoveTestButtonHandler(object sender, RoutedEventArgs e)
        {
            int i = TestListBox.SelectedIndex;
            currentTestRequest.tests.RemoveAt(i);
            testList.RemoveAt(i);
            TestListBox.Items.Refresh();
        }

        private void AddTestDriverButtonHandler(object sender, RoutedEventArgs e)
        {
            if (TestListBox.SelectedIndex < 0)
                return;

            Test test = currentTestRequest.tests[TestListBox.SelectedIndex];

            // Create OpenFileDialog 
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.DefaultExt = ".dll";
            dlg.InitialDirectory = appLocation;
            Nullable<bool> result = dlg.ShowDialog();

            // Get the selected file name and display in a TextBox 
            if (result == true)
            {
                test.testDriver = System.IO.Path.GetFileName(dlg.FileName);
                File.Copy(
                    dlg.FileName,
                    System.IO.Path.Combine(clientLibFolder, test.testDriver),
                    true);
            }

            UpdateFileList();
        }

        private void AddSourceFileButtonHandler(object sender, RoutedEventArgs e)
        {
            if (TestListBox.SelectedIndex < 0)
                return;

            Test test = currentTestRequest.tests[TestListBox.SelectedIndex];

            // Create OpenFileDialog 
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.DefaultExt = ".dll";
            dlg.InitialDirectory = appLocation;
            Nullable<bool> result = dlg.ShowDialog();

            // Get the selected file name and display in a TextBox 
            if (result == true)
            {
                string sourceFileName = System.IO.Path.GetFileName(dlg.FileName);
                test.testCode.Add(sourceFileName);
                File.Copy(
                    dlg.FileName,
                    System.IO.Path.Combine(clientLibFolder, sourceFileName),
                    true);
            }

            UpdateFileList();
        }

        private void UpdateFileList()
        {

            fileList.Clear();
            Test test = currentTestRequest.tests[TestListBox.SelectedIndex];
            if (test.testDriver != "")
            {
                fileList.Add(new TestFile()
                {
                    FileName = test.testDriver,
                    FileType = "Test Driver"
                });
            }
            foreach (string source in test.testCode)
            {
                fileList.Add(new TestFile()
                {
                    FileName = source,
                    FileType = "Source Code"
                });
            }

            FilesDataGrid.Items.Refresh();
        }

        private void RemoveSelectedFileButtonHandler(object sender, RoutedEventArgs e)
        {
            int i = FilesDataGrid.SelectedIndex;
            Test test = currentTestRequest.tests[TestListBox.SelectedIndex];

            if (i == 0)
                test.testDriver = "";
            else
                test.testCode.RemoveAt(i - 1);

            UpdateFileList();
        }

        private void SaveCurrentTestRequest(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
            dlg.FileName = "default_test_request"; // Default file name
            if (XMLFileNameTextBox.Text != "")
                dlg.FileName = XMLFileNameTextBox.Text;
            dlg.DefaultExt = ".xml"; // Default file extension
            dlg.Filter = "XML Files (.xml)|*.xml"; // Filter files by extension

            // Show save file dialog box
            Nullable<bool> result = dlg.ShowDialog();

            // Process save file dialog box results
            if (result == true)
            {
                // Save document
                string filename = dlg.FileName;
                currentTestRequest.xmlPath = filename;
                currentTestRequest.author = AuthorNameTextBox.Text;
                XMLFactory.Serialize(currentTestRequest);
            }

        }

        private void LoadTestRequestFromFile(object sender, RoutedEventArgs e)
        {
            // Create OpenFileDialog 
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.DefaultExt = ".xml";
            dlg.InitialDirectory = appLocation;
            Nullable<bool> result = dlg.ShowDialog();

            // Get the selected file name and display in a TextBox 
            if (result == true)
            {
                string filename = dlg.FileName;

                XMLFileNameTextBox.Text = System.IO.Path.GetFileNameWithoutExtension(filename);

                List<Test> parsed = ParseAndGetTests(filename);
                if (parsed == null)
                    return;

                testList.Clear();
                fileList.Clear();
                currentTestRequest = new TestRequest();
                currentTestRequest.tests = new List<Test>();

                foreach (Test test in parsed)
                {
                    testList.Add(test.testName);
                    currentTestRequest.tests.Add(test);
                }

                AuthorNameTextBox.Text = parsed[0].author;
                FilesDataGrid.Items.Refresh();
                TestListBox.Items.Refresh();
            }
        }

        private List<Test> ParseAndGetTests(string filename)
        {
            XMLFactory xf = new XMLFactory(new Logger());
            FileStream xml;
            try
            {
                xml = new FileStream(filename, FileMode.Open);
            }
            catch (FileNotFoundException)
            {
                //Log(TAG, string.Format("Test request file is not found in provided path: {0}\n", xmlPath));
                return null;
            }
            catch (Exception)
            {
                //Log(TAG, string.Format("Test request file could not be opened.\n", xmlPath));
                return null;
            }

            if (!xf.parse(xml))
            {
                //Log(TAG, string.Format("Parse error! Skiping test request.\n"));
                return null;
            }

            if (xf.getTests().Count < 1)
            {
                //Log(TAG, string.Format("No tests found in test request! Skiping test request.\n"));
                return null;
            }

            return xf.getTests();
        }

        private void InitAndConnect(object sender, RoutedEventArgs e)
        {
            if (client != null)
            {
                Console.WriteLine("You are already connected!");
                return;
            }
            Console.WriteLine("Connecting to Test Harness and Repository...");
            string url1 = TestHarnessCommunicationUrlTextBox.Text;
            string url2 = TestHarnessStreamUrlTextBox.Text;
            string url3 = RepositoryStreamUrlTextBox.Text;
            string url4 = RepositoryCommunicationUrlTextBox.Text;
            Thread t = new Thread(new ThreadStart(() =>
            {
                client = new Client();
                client.logListCallBack = new LogListCallBack(ListLogs);
                client.logFileCallBack = new LogFileCallBack(DisplayLog);
                client.ConnectTestHarness(url1, url2);
                client.ConnectRepository(url4, url3);
            }));
            t.Start();
        }

        private void DisplayLogButtonHandler(object sender, RoutedEventArgs e)
        {
            client.SendLogFileRequest(LogFileNameTextBox.Text);
        }

        private void GetLogListButtonHandler(object sender, RoutedEventArgs e)
        {
            client.SendLogFileListRequest();
        }

        public void DisplayLog(string logContent)
        {
            Dispatcher.Invoke(() => {
                LogResultsTextBox.Text = logContent;
            });
        }

        public void ListLogs(string logList)
        {
            char[] delimitter = { ',' };
            string[] files = logList.Split(delimitter, StringSplitOptions.RemoveEmptyEntries);
            Dispatcher.Invoke(()=> {
                LogResultsTextBox.Text = "";
                foreach (string file in files)
                {
                    LogResultsTextBox.Text += file + "\n";
                }
            });
        }
        
    }


    public class Client
    {
        string appLocation;
        string clientFilesDir;

        StreamClient THStreamClient;
        StreamClient RepoStreamClient;
        SenderService TestHarnessSender;
        SenderService RepositorySender;
        string TestHarnessUrl = "http://localhost:4040/TestHarnessChannel";
        string TestHarnessStreamUrl = "http://localhost:8000/TestHarnessStreamService";

        string RepositoryUrl = "http://localhost:4040/RepoChannel";
        string RepositoryStreamUrl = "http://localhost:8000/RepositoryStreamService";
        
        string ClientChannelUrl = "http://localhost:4040/ClientChannel";
        ReceiverService clientReceiver;

        Thread clientProcedure;

        public LogListCallBack logListCallBack { get; set; }
        public LogFileCallBack logFileCallBack { get; set; }

        public int clientCount { get; set; }
        

        public Client()
        {
            Thread.Sleep(2000);

            clientCount = 0;
            bool again = false;
            clientReceiver = new ReceiverService();
            do
            {
                try
                {
                    ClientChannelUrl = ClientChannelUrl + "/client" + clientCount;
                    clientReceiver.CreateChannel(ClientChannelUrl);
                    clientReceiver.Start();
                    again = false;
                }
                catch
                {
                    clientCount++;
                    again = true;
                }
            } while (again);

            appLocation = System.IO.Path.GetFullPath(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location));
            clientFilesDir = System.IO.Path.Combine(appLocation, @"ClientLibs");

            clientProcedure = new Thread(new ThreadStart(ClientReceiverProc));
            clientProcedure.Start();

        }

        private void ClientReceiverProc()
        {
            bool running = true;
            while (running)
            {
                Message msg = clientReceiver.GetMessage();
                string text = msg.text;
                switch (msg.command)
                {
                    case Message.Command.TestRequestSetupError:
                        Console.WriteLine(msg.text);
                        break;
                    case Message.Command.Shutdown:
                        running = false;
                        break;
                    case Message.Command.LogFileResponse:
                        logFileCallBack(msg.text);
                        break;
                    case Message.Command.LogFileListResponse:
                        logListCallBack(msg.text);
                        break;
                }
            }
        }

        public void SendLogFileRequest(string logFileName)
        {
            Message msg = new Message();
            msg.command = Message.Command.LogFileRequest;
            msg.text = ClientChannelUrl + "," + logFileName;
            RepositorySender.PostMessage(msg);
        }

        public void SendLogFileListRequest()
        {
            Message msg = new Message();
            msg.command = Message.Command.LogFileListRequest;
            msg.text = ClientChannelUrl;
            RepositorySender.PostMessage(msg);
        }

        public void UploadToRepository(string filePath)
        {
            RepoStreamClient.uploadFile(filePath);
        }

        public void UploadToTestHarness(string filePath)
        {
            THStreamClient.uploadFile(filePath);
        }

        public void SendMessageToTestHarness(Message msg)
        {
            TestHarnessSender.PostMessage(msg);
        }

        public void Stop()
        {
            TestHarnessSender.Stop();
        }

        public void SetTestHarnessCommunicationUrl(string url)
        {
            TestHarnessUrl = url;
        }

        public void SetTestHarnessStreamUrl(string url)
        {
            TestHarnessStreamUrl = url;
        }

        public void SetRepositoryStreamUrl(string url)
        {
            RepositoryStreamUrl = url;
        }

        public bool ConnectTestHarness(string url, string streamUrl)
        {
            THStreamClient = new StreamClient(streamUrl);
            TestHarnessSender = new SenderService(url);
            TestHarnessSender.Start();

            Message msg = new Message();
            msg.command = Message.Command.ConnectMe;
            msg.text = ClientChannelUrl;
            TestHarnessSender.PostMessage(msg);

            return true;
        }

        public bool ConnectRepository(string url, string streamUrl)
        {
            RepoStreamClient = new StreamClient(streamUrl);
            RepositorySender = new SenderService(url);
            RepositorySender.Start();

            return true;
        }
    }
}
