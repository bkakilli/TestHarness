using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TestHarness
{
    public class Repository
    {
        string RepositoryChannelUrl;
        ReceiverService receiver;
        SenderService TestHarnessSender;
        StreamClient streamClient;
        StreamService RepoStreamServer;
        Thread repoThread;

        string repositoryFolder;
        string RepositoryStreamUrl = "http://localhost:8000/RepositoryStreamService";

        public Repository(string path)
        {
            RepositoryChannelUrl = "http://localhost:4040/RepoChannel";
            repositoryFolder = path;

            receiver = new ReceiverService();
            receiver.CreateChannel(RepositoryChannelUrl);

            RepoStreamServer = new StreamService();
            RepoStreamServer.Start(RepositoryStreamUrl);

            repoThread = new Thread(new ThreadStart(RepositoryProcedure));
        }

        public void RepositoryProcedure()
        {
            bool running = true;
            while (running)
            {
                Message msg = receiver.GetMessage();
                switch (msg.command)
                {
                    case Message.Command.ConnectMe:
                        TestHarnessSender = new SenderService(msg.text);
                        TestHarnessSender.Start();
                        Console.WriteLine("Connected to TestHarness server: {0}", msg.text);
                        break;
                    case Message.Command.ConnectFileServer:
                        streamClient = new StreamClient(msg.text);
                        Console.WriteLine("Connected to TestHarness file server: {0}", msg.text);
                        break;
                    case Message.Command.FileListRequest:
                        FileListRequest(msg);
                        break;
                    case Message.Command.Shutdown:
                        running = false;
                        break;
                    case Message.Command.LogFileRequest:
                        HandleLogFileRequest(msg);
                        break;
                    case Message.Command.LogFileListRequest:
                        HandleLogFileListRequest(msg);
                        break;
                }
            }
        }

        public void Start()
        {
            receiver.Start();
            repoThread.Start();
        }

        private void HandleLogFileRequest(Message msg)
        {
            string[] ss = msg.text.Split(',');

            SenderService client = new SenderService(ss[0]);
            client.Start();

            Message response = new Message();
            response.command = Message.Command.LogFileResponse;

            string filepath = Path.Combine(repositoryFolder, ss[1]);
            try
            {
                response.text = File.ReadAllText(filepath);
            }
            catch (Exception ex)
            {
                response.text = "";
            }

            client.PostMessage(response);
            client.Stop();
        }

        private void HandleLogFileListRequest(Message msg)
        {
            SenderService client = new SenderService(msg.text);
            client.Start();

            Message response = new Message();
            response.text = GetLogFileList();
            response.command = Message.Command.LogFileListResponse;

            client.PostMessage(response);
            client.Stop();
        }

        private string GetLogFileList()
        {
            string[] files = Directory.GetFiles(RepoStreamServer.GetFileReceivePath(), "*.log", SearchOption.AllDirectories);
            string result = "";
            foreach (string file in files)
                result += Path.GetFileName(file) + ",";
            return result;
        }

        private void FileListRequest(Message msg)
        {
            Thread worker = new Thread(new ThreadStart(() =>
            {
                string missingFiles = "";
                char[] delimitter = { ',' };
                string[] filenames = msg.text.Split(delimitter, StringSplitOptions.RemoveEmptyEntries);
                foreach (string file in filenames)
                {
                    // Find and upload files one by one. Append missing files to reponse text
                    string[] filesFound = Directory.GetFiles(repositoryFolder, file, SearchOption.AllDirectories);
                    if (filesFound.Length == 0)
                        missingFiles += file + ",";
                    else
                    {
                        string targetFilePath = Path.GetFullPath(filesFound[0]);
                        streamClient.uploadFile(targetFilePath);
                    }
                }
                Message response = new Message();
                response.command = Message.Command.FileListFromClientResposeFromRepository;
                response.text = missingFiles;
                response.token = msg.token;
                TestHarnessSender.PostMessage(response);
            }));
            worker.Start();
            worker.Join();  // Make it syncronized
        }

        public void Stop()
        {
            Message msg = new Message();
            msg.command = Message.Command.Shutdown;
            msg.text = "local";

            receiver.PostMessage(msg);
            TestHarnessSender.PostMessage(msg);
            
            repoThread.Join();
        }
        

        public static void Main(string[] args)
        {
            Console.Title = "Repository";

            string appLocation = Path.GetFullPath(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location));
            string repoPath = Path.GetFullPath(Path.Combine(appLocation, @"SavedFiles"));
            if (!Directory.Exists(repoPath))
                Directory.CreateDirectory(repoPath);

            Repository repo = new Repository(repoPath);
            repo.Start();

            while (true)
            {
                Console.WriteLine("Enter q to quit repository.");
                string inp = Console.ReadLine();
                if (inp == "q")
                    break;
            }

            repo.Stop();

        }

    }
}
