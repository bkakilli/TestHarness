using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TestHarness
{
    public delegate void RequestHandler(string s);

    public class ComServer
    {
        RequestHandler enQRequest;
        ReceiverService ReceiveChannel;
        StreamService FileService;
        Thread thread;
        public string appLocation;

        public ComServer(RequestHandler handler)
        {
            enQRequest = handler;
            ReceiveChannel = new ReceiverService();
            ReceiveChannel.CreateChannel(new Uri("http://localhost:4040/ICommService/WSHttp"));

            FileService = new StreamService();
        }

        public void Start()
        {
            ReceiveChannel.Start();
            FileService.Start();
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
                            enQRequest(msg.text);
                            break;
                        case Message.Command.Shutdown:
                            enQRequest("quit");
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

        public string getUploadDir()
        {
            return FileService.FileReceivePath;
        }

        public List<string> fetchFilesFromRepository(List<string> fileList, string libDirectory)
        {
            List<string> missingFiles = new List<string>();

            //Log(TAG, string.Format("Copying files from repository...\n"));
            string repo = Path.GetFullPath(Path.Combine(appLocation, @"../../../Repository_folder"));
            foreach (string file in fileList)
            {
                try
                {
                    string[] filesFound = Directory.GetFiles(
                        repo, file, SearchOption.AllDirectories);
                    if (filesFound.Length == 0)
                    {
                        missingFiles.Add(file);
                        Console.WriteLine("File not found: {0}", file);
                    }
                    else
                    {
                        string targetFilePath = Path.GetFullPath(Path.Combine(libDirectory, Path.GetFileName(filesFound[0])));
                        File.Copy(filesFound[0], targetFilePath, true);
                        //Console.WriteLine("File copied: {0}", targetFilePath);
                    }

                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }

            //return fileList;    // temporary

            return missingFiles;
        }



#if (Server_TEST)
        public static void Main(string[] args)
        {

        }
#endif
    }

}
