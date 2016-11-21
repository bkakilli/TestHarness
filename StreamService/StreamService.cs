using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace TestHarness
{
    public class StreamService : IStreamService
    {
        string filename;
        public string FileReceivePath = @"SavedFiles";
        public string FileSendPath = @"ToSend";
        int BlockSize = 1024;
        byte[] block;
        public string fileStreamServerUrl = "http://localhost:8000/StreamService";
        public string appLocation;

        ServiceHost fileHost;

        public StreamService()
        {
            block = new byte[BlockSize];
            appLocation = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            FileReceivePath = Path.GetFullPath(Path.Combine(appLocation, FileReceivePath));
            FileSendPath = Path.GetFullPath(Path.Combine(appLocation, FileSendPath));
        }

        public void Start()
        {
            // File streaming

            fileHost = CreateServiceChannel(fileStreamServerUrl);
            fileHost.Open();
        }

        public void upLoadFile(FileTransferMessage msg)
        {
            // Upload is from client to server
            // Code below is receiving the file from the client endpoint
            int totalBytes = 0;
            //hrt.Start();
            filename = msg.filename;
            string rfilename = Path.Combine(this.FileReceivePath, filename);
            if (!Directory.Exists(FileReceivePath))
                Directory.CreateDirectory(FileReceivePath);
            using (var outputStream = new FileStream(rfilename, FileMode.Create))
            {
                while (true)
                {
                    int bytesRead = msg.transferStream.Read(block, 0, BlockSize);
                    totalBytes += bytesRead;
                    if (bytesRead > 0)
                        outputStream.Write(block, 0, bytesRead);
                    else
                        break;
                }
            }
            //hrt.Stop();
            Console.Write(
              "\n  Received file \"{0}\" of {1} bytes.",
              Path.GetFullPath(rfilename), totalBytes
            );
        }

        public Stream downLoadFile(string filename)
        {
            //hrt.Start();
            string sfilename = Path.Combine(FileSendPath, filename);
            FileStream outStream = null;
            if (File.Exists(sfilename))
            {
                outStream = new FileStream(sfilename, FileMode.Open);
            }
            else
                throw new Exception("open failed for \"" + filename + "\"");
            //hrt.Stop();
            //Console.Write("\n  Sent \"{0}\" in {1} microsec.", filename, hrt.ElapsedMicroseconds);
            return outStream;
        }

        ServiceHost CreateServiceChannel(string url)
        {
            // Can't configure SecurityMode other than none with streaming.
            // This is the default for BasicHttpBinding.
            //   BasicHttpSecurityMode securityMode = BasicHttpSecurityMode.None;
            //   BasicHttpBinding binding = new BasicHttpBinding(securityMode);

            BasicHttpBinding binding = new BasicHttpBinding();
            binding.TransferMode = TransferMode.Streamed;
            binding.MaxReceivedMessageSize = 50000000;
            Uri baseAddress = new Uri(url);
            Type service = typeof(TestHarness.StreamService);
            ServiceHost host = new ServiceHost(service, baseAddress);
            host.AddServiceEndpoint(typeof(IStreamService), binding, baseAddress);
            return host;
        }
    }

    public class StreamClient
    {
      IStreamService fileChannel;
      string ToSendPath = @"UploadFiles";
      string SavePath = @"DownloadFiles";
      int BlockSize = 1024;
      byte[] block;

      public StreamClient()
      {
          string appLocation = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
          ToSendPath = Path.GetFullPath(Path.Combine(appLocation, ToSendPath));
          SavePath = Path.GetFullPath(Path.Combine(appLocation, SavePath));
          block = new byte[BlockSize];
      }

      public void CreateServiceChannel(string url)
      {
          BasicHttpSecurityMode securityMode = BasicHttpSecurityMode.None;

          BasicHttpBinding binding = new BasicHttpBinding(securityMode);
          binding.TransferMode = TransferMode.Streamed;
          binding.MaxReceivedMessageSize = 500000000;
          EndpointAddress address = new EndpointAddress(url);

          ChannelFactory<IStreamService> factory
            = new ChannelFactory<IStreamService>(binding, address);
          fileChannel = factory.CreateChannel();
      }


      public void uploadFile(string filename)
      {
          // Upload is from client to server
          string fqname = Path.Combine(ToSendPath, filename);
          try
          {
              //hrt.Start();
              using (var inputStream = new FileStream(fqname, FileMode.Open))
              {
                  FileTransferMessage msg = new FileTransferMessage();
                  msg.filename = filename;
                  msg.transferStream = inputStream;
                  fileChannel.upLoadFile(msg);
              }
              //hrt.Stop();
              //Console.Write("\n  Uploaded file \"{0}\" in {1} microsec.", filename, hrt.ElapsedMicroseconds);
          }
          catch
          {
              Console.Write("\n  can't find \"{0}\"", fqname);
          }
      }

      public void download(string filename)
      {
          int totalBytes = 0;
          try
          {
              //hrt.Start();
              Stream strm = fileChannel.downLoadFile(filename);
              string rfilename = Path.Combine(SavePath, filename);
              if (!Directory.Exists(SavePath))
                  Directory.CreateDirectory(SavePath);
              using (var outputStream = new FileStream(rfilename, FileMode.Create))
              {
                  while (true)
                  {
                      int bytesRead = strm.Read(block, 0, BlockSize);
                      totalBytes += bytesRead;
                      if (bytesRead > 0)
                          outputStream.Write(block, 0, bytesRead);
                      else
                          break;
                  }
              }
              //hrt.Stop();
              //ulong time = hrt.ElapsedMicroseconds;
              //Console.Write("\n  Received file \"{0}\" of {1} bytes in {2} microsec.", filename, totalBytes, time);
          }
          catch (Exception ex)
          {
              Console.Write("\n  {0}", ex.Message);
          }
      }
    }

}
