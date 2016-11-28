using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TestHarness
{
    public delegate void RequestHandler(TestRequest t);

    public class WaitResponse
    {
        string clientID;
        string key;
        string text;
    }

    public class ComServer
    {
        string TestHarnessUrl;
        string TestHarnessStreamUrl;
        Dictionary<string, WaitResponse> MonitorDict;
        //object responseWait;

        RequestHandler enQRequest;

        ReceiverService ReceiveChannel;
        StreamService FileService;
        StreamClient RepositoryFileSender;

        Thread thread;

        Dictionary<string, SenderService> SenderDictionary;

        public ComServer(RequestHandler handler)
        {
            TestHarnessUrl = "http://localhost:4040/TestHarnessChannel";
            TestHarnessStreamUrl = "http://localhost:8000/TestHarnessStreamService";
            enQRequest = handler;
            ReceiveChannel = new ReceiverService();
            ReceiveChannel.CreateChannel(TestHarnessUrl);

            RepositoryFileSender = new StreamClient("http://localhost:8000/RepositoryStreamService");

            FileService = new StreamService();

            SenderDictionary = new Dictionary<string, SenderService>();
            MonitorDict = new Dictionary<string, WaitResponse>();
            //responseWait = new object();
        }

        public void Start()
        {
            ReceiveChannel.Start();
            FileService.Start(TestHarnessStreamUrl);
            Thread.Sleep(200);
            thread = new Thread(new ThreadStart(() =>
            {
                bool running = true;
                while (running)
                {
                    Message msg = ReceiveChannel.GetMessage();
                    switch (msg.command)
                    {
                        case Message.Command.TestRequest:
                            TestRequest testRequest = new TestRequest();
                            testRequest.clientID = msg.address + ":" + msg.port;
                            testRequest.xmlFileName = msg.text;
                            enQRequest(testRequest);
                            break;
                        case Message.Command.FileListRespose:
                            lock (MonitorDict[msg.token])
                                Monitor.Pulse(MonitorDict[msg.token]);
                            break;
                        case Message.Command.FileListFromClientResposeFromRepository:
                            lock (MonitorDict[msg.token])
                                Monitor.Pulse(MonitorDict[msg.token]);
                            break;
                        case Message.Command.ConnectMe:
                            string url = msg.text;
                            string clientID = msg.address + ":" + msg.port;
                            SenderService newClientChannel = new SenderService(url);
                            newClientChannel.Start();

                            SenderDictionary.Add(clientID, newClientChannel);
                            Console.WriteLine("Client {0} connected.", clientID);
                            break;
                        case Message.Command.Shutdown:
                            TestRequest t = new TestRequest();
                            t.xmlFileName = "quit";
                            enQRequest(t);
                            running = false;
                            break;
                    }
                }
            }));
            thread.Start();
        }

        public void Stop()
        {
            Console.WriteLine("Server closed.");
            thread.Join();
        }

        public bool connectToRepository(string repoUrl)
        {
            SenderService RepositorySender = new SenderService(repoUrl);
            RepositorySender.Start();

            Message msg1 = new Message();
            msg1.command = Message.Command.ConnectMe;
            msg1.text = TestHarnessUrl;
            Message msg2 = new Message();
            msg2.command = Message.Command.ConnectFileServer;
            msg2.text = TestHarnessStreamUrl;

            RepositorySender.PostMessage(msg1);
            RepositorySender.PostMessage(msg2);

            SenderDictionary.Add("repository", RepositorySender);

            return true;
        }

        public void UploadToRepository(string path)
        {
            RepositoryFileSender.uploadFile(path);
        }

        public void DisconnectFromSender(string source)
        {
            SenderDictionary[source].Stop();
        }

        public string getUploadDir()
        {
            return FileService.GetFileReceivePath();
        }

        public string FetchFilesFromSource(string fileList, string libDirectory, string source="repository")
        {
            string testRequestID = Path.GetFileName(
                Path.GetFullPath(libDirectory).TrimEnd(Path.DirectorySeparatorChar));
            string token = testRequestID + source;

            Message msg = new Message();
            msg.text = fileList;
            msg.command = Message.Command.FileListRequest;
            msg.token = token;
            SenderDictionary[source].PostMessage(msg);
            

            if (!MonitorDict.ContainsKey(token))
                MonitorDict.Add(token, new WaitResponse());

            // Wait for repository to respond
            lock (MonitorDict[token])
                Monitor.Wait(MonitorDict[token]);

            string missingFiles = "";
            char[] delimitter = { ',' };
            foreach (string file in fileList.Split(delimitter, StringSplitOptions.RemoveEmptyEntries))
            {
                string sourceFilePath = Path.Combine(FileService.GetFileReceivePath(), Path.GetFileName(file));
                string targetFilePath = Path.Combine(libDirectory, Path.GetFileName(file));
                if (File.Exists(sourceFilePath))
                    FileManager<string>.copyFile(sourceFilePath, targetFilePath);
                else
                    missingFiles += file + ",";
            }

            return missingFiles;
        }

        public void NotifyClient(string clientID, Message msg)
        {
            SenderDictionary[clientID].PostMessage(msg);
        }

        public string GetCache()
        {
            return FileService.GetFileReceivePath();
        }



#if (Server_TEST)
        public static void sampleEnq(TestRequest tr)
        {
            Console.WriteLine("Server_TEST");
        }
        public static void Main(string[] args)
        {
            
            ComServer cs = new ComServer(new RequestHandler(sampleEnq));
            cs.Start();

            cs.Stop();

        }
#endif
    }

}
