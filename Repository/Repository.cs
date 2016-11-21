using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TestHarness
{
    public class Repository
    {
        ReceiverService receiver;
        SenderService sender;
        Thread repoThread;

        public Repository()
        {
            receiver = new ReceiverService();
            receiver.CreateChannel(new Uri("http://localhost:4041/ICommService/WSHttp"));
            sender = new SenderService("http://localhost:4040/ICommService/WSHttp");

            repoThread = new Thread(new ThreadStart(RepositoryProcedure));
        }

        public void RepositoryProcedure()
        {
            bool running = true;
            while (running)
            {
                Message msg = receiver.GetMessage();
                string text = msg.text;
                switch (msg.command)
                {
                    case Message.Command.FileListFromRepo:
                        Thread worker = new Thread(new ThreadStart(() =>
                        {
                            // Search files and send them one by one
                            // Keep track the names of not found
                            // Send FileListFromRepoResponse message with list of not found files
                        }));
                        worker.Start();
                        break;
                    case Message.Command.Shutdown:
                        running = false;
                        break;
                }
            }
        }

        public void Start()
        {
            repoThread.Start();
        }

        public void Stop()
        {
            Message msg = new Message();
            msg.command = Message.Command.Shutdown;
            msg.text = "local";

            receiver.PostMessage(msg);
            sender.PostMessage(msg);

            repoThread.Join();
        }

        public static void Main(string[] args)
        {
            Repository repo = new Repository();
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
