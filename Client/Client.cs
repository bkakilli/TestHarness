////////////////////////////////////////////////////////////////////////////////
//  TestCore.cs - Schedules test requests and run them in seperate AppDomains //
//  ver 0.5                                                                   //
//  Language:     C#, VS 2015, .NET Framework 4.5.2                           //
//  Platform:     Windows 10                                                  //
//  Application:  Test Harness, CSE681 - Project 2                            //
//  Author:       Burak Kakillioglu, Syracuse University                      //
//                bkakilli@syr.edu                                            //
////////////////////////////////////////////////////////////////////////////////


using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.ServiceModel;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TestHarness
{
    class Client
    {

      //        ICommService commChannel;
      //        IStreamService fileChannel;
      //        string ToSendPath = @"TestRequests";
      //        string SavePath = @"DownloadedFiles";
      //        int BlockSize = 1024;
      //        byte[] block;
      //
      //        Client()
      //        {
      //            string appLocation = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
      //            ToSendPath = Path.GetFullPath(Path.Combine(appLocation, ToSendPath));
      //            SavePath = Path.GetFullPath(Path.Combine(appLocation, SavePath));
      //            block = new byte[BlockSize];
      //        }
      //
      //        void CreateServiceChannel(string url)
      //        {
      //            BasicHttpSecurityMode securityMode = BasicHttpSecurityMode.None;
      //
      //            BasicHttpBinding binding = new BasicHttpBinding(securityMode);
      //            binding.TransferMode = TransferMode.Streamed;
      //            binding.MaxReceivedMessageSize = 500000000;
      //            EndpointAddress address = new EndpointAddress(url);
      //
      //            ChannelFactory<IStreamService> factory
      //              = new ChannelFactory<IStreamService>(binding, address);
      //            fileChannel = factory.CreateChannel();
      //        }
      //
      //
      //        void uploadFile(string filename)
      //        {
      //            // Upload is from client to server
      //            string fqname = Path.Combine(ToSendPath, filename);
      //            try
      //            {
      //                //hrt.Start();
      //                using (var inputStream = new FileStream(fqname, FileMode.Open))
      //                {
      //                    FileTransferMessage msg = new FileTransferMessage();
      //                    msg.filename = filename;
      //                    msg.transferStream = inputStream;
      //                    fileChannel.upLoadFile(msg);
      //                }
      //                //hrt.Stop();
      //                //Console.Write("\n  Uploaded file \"{0}\" in {1} microsec.", filename, hrt.ElapsedMicroseconds);
      //            }
      //            catch
      //            {
      //                Console.Write("\n  can't find \"{0}\"", fqname);
      //            }
      //        }
      //
      //        void download(string filename)
      //        {
      //            int totalBytes = 0;
      //            try
      //            {
      //                //hrt.Start();
      //                Stream strm = fileChannel.downLoadFile(filename);
      //                string rfilename = Path.Combine(SavePath, filename);
      //                if (!Directory.Exists(SavePath))
      //                    Directory.CreateDirectory(SavePath);
      //                using (var outputStream = new FileStream(rfilename, FileMode.Create))
      //                {
      //                    while (true)
      //                    {
      //                        int bytesRead = strm.Read(block, 0, BlockSize);
      //                        totalBytes += bytesRead;
      //                        if (bytesRead > 0)
      //                            outputStream.Write(block, 0, bytesRead);
      //                        else
      //                            break;
      //                    }
      //                }
      //                //hrt.Stop();
      //                //ulong time = hrt.ElapsedMicroseconds;
      //                //Console.Write("\n  Received file \"{0}\" of {1} bytes in {2} microsec.", filename, totalBytes, time);
      //            }
      //            catch (Exception ex)
      //            {
      //                Console.Write("\n  {0}", ex.Message);
      //            }
      //        }
      //
      //        void CreateWSHttpChannel(string url)
      //        {
      //            EndpointAddress address = new EndpointAddress(url);
      //            WSHttpBinding binding = new WSHttpBinding();
      //            commChannel = ChannelFactory<ICommService>.CreateChannel(binding, address);
      //        }

        static void Main(string[] args)
        {
            Console.Title = "Client";
            //string logFolderName = "Logs";
            string usage =
                "Usage information: \n" +
                "Perform an automated demonstration  a\n" +
                "Show this dialog:                   h or help\n" +
                "List available test requests:       lt\n" +
                "Run a test request:                 <testRequestfileName>\n" +
                "List available log files:           ll\n" +
                "Get log of a specific test request: gl\n" +
                "Toggle verbose:                     v\n" +
                "Show test queue:                    s\n" +
                "Quit program:                       q\n" +
                "Force quit program:                 fq\n";

            string repository;

            if (args.Length < 1)
            {
                Console.Write("Repository path is not provided.\nPlease provide a repository path: ");
                repository = Console.ReadLine();
            }
            else
                repository = args[0];

            Console.WriteLine(usage);

            Thread.Sleep(2000);

            StreamClient streamClient = new StreamClient();
            streamClient.CreateServiceChannel("http://" + args[0] + ":8000/StreamService");

            string url = "http://" + args[0] + ":4040/ICommService/WSHttp";
            SenderService client = new SenderService(url);
            client.Start();

            string appLocation = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            string TestFolder = Path.GetFullPath(Path.Combine(
                appLocation, "TestRequests"
                ));

            //client.uploadFile("TestRequest.xml");
            streamClient.uploadFile("TestRequest1.xml");
            //client.uploadFile("TestRequest2.xml");

            {
                Message msg = new Message();
                msg.command = Message.Command.TestRequest;
                msg.text = "TestRequest1.xml";
                client.PostMessage(msg);
            }

            //msg.text = "TestRequest1.xml";
            //client.commChannel.PostMessage(msg);
            //msg.text = "TestRequest2.xml";
            //client.commChannel.PostMessage(msg);
            //msg.text = "TestRequest3.xml";
            //client.commChannel.PostMessage(msg);
            //msg.command = Message.Command.Shutdown;
            //client.PostMessage(msg);

            Thread.Sleep(1000);
            client.Stop();

            //while (true)
            //{
            //    Console.Write("Choose wisely: ");
            //    string line = Console.ReadLine();
            //    if (line == "q")
            //    {
            //        Console.WriteLine("Quiting");
            //        //core.Stop();
            //        break;
            //    }
            //    else if (line == "fq")
            //    {
            //        Console.WriteLine("Force quit.");
            //        //core.Stop(true);
            //        break;
            //    }
            //    else if (line == "lt")
            //    {
            //        string testRequestFolder = Path.GetFullPath(Path.Combine(
            //            Path.GetDirectoryName(Assembly.GetEntryAssembly().Location),
            //            repository, testRequestFolderName
            //            ));
            //        try
            //        {
            //            string[] filesFound = Directory.GetFiles(
            //            testRequestFolder, "*.xml", SearchOption.TopDirectoryOnly);
            //            foreach (string file in filesFound)
            //            {
            //                Console.WriteLine(Path.GetFileNameWithoutExtension(file));
            //            }
            //        }
            //        catch (Exception ex)
            //        {
            //            Console.WriteLine("Test directory folder could not be read:\n{0}\n", ex.Message);
            //        }
            //        Console.WriteLine();
            //    }
            //    else if (line == "ll")
            //    {
            //        string logFolder = Path.GetFullPath(Path.Combine(
            //            Path.GetDirectoryName(Assembly.GetEntryAssembly().Location),
            //            repository, logFolderName
            //            ));
            //        try
            //        {
            //            string[] filesFound = Directory.GetFiles(
            //            logFolder, "*.log", SearchOption.TopDirectoryOnly);
            //            foreach (string file in filesFound)
            //            {
            //                Console.WriteLine(Path.GetFileNameWithoutExtension(file));
            //            }
            //        }
            //        catch (Exception ex)
            //        {
            //            Console.WriteLine("Log directory folder could not be read:\n{0}\n", ex.Message);
            //        }
            //        Console.WriteLine();
            //    }
            //    else if (line == "gl")
            //    {
            //        Console.WriteLine("Enter the log file name: ");
            //        string logName = Console.ReadLine();
            //        //Console.WriteLine(core.getLog(logName));

            //    }
            //    else if (line == "v")
            //    {
            //        //core.setVerbose(!core.logger.verbose);
            //    }
            //    else if (line == "s")
            //    {
            //        //Console.WriteLine("Test requests waiting in the queue:\n{0}", core.getQueueElements());
            //    }
            //    else if (line == "a")
            //    {
            //        //core.enQRequest("TestRequest");
            //        //core.enQRequest("TestRequest1");
            //        //core.enQRequest("TestRequest2");
            //        //core.enQRequest("TestRequest3");
            //        //core.enQRequest("someNonExistingTestRequest");
            //    }
            //    else if (line == "h" || line == "help")
            //    {
            //        Console.WriteLine(usage);
            //    }
            //}
        }
    }
}
