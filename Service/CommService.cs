/////////////////////////////////////////////////////////////////////////
// CommService.svc.cs - Implementation of ICommService contract        //
//                                                                     //
// Jim Fawcett, CSE775 - Distributed Objects, Spring 2009              //
/////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Text;
using System.Threading;
using System.Runtime.InteropServices;

namespace TestHarness
{
    // PerSession activation creates an instance of the service for each
    // client.  That instance lives for a pre-determined lease time.
    // - If the creating client calls back within the lease time, then
    //   the lease is renewed and the object stays alive.  Otherwise it
    //   is invalidated for garbage collection.
    // - This behavior is a reasonable compromise between the resources
    //   spent to create new objects and the memory allocated to persistant
    //   objects.
    //

    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerSession)]

    public class ReceiverService : ICommService
    {

        // We want the queue to be shared by all clients and the server,
        // so make it static.

        static BlockingQueue<Message> BlockingQ = null;
        ServiceHost host = null;

        public ReceiverService()
        {
            // Only one service, the first, should create the queue

            if (BlockingQ == null)
                BlockingQ = new BlockingQueue<Message>();

        }

        public void Start()
        {
            host.Open();
        }

        public void PostMessage(Message msg)
        {
            AddClientInfo(msg);
            BlockingQ.enQ(msg);
        }

        public void AddClientInfo(Message msg)
        {
            OperationContext context = OperationContext.Current;
            MessageProperties messageProperties = context.IncomingMessageProperties;
            RemoteEndpointMessageProperty endpointProperty =
                messageProperties[RemoteEndpointMessageProperty.Name]
                as RemoteEndpointMessageProperty;

            msg.address = endpointProperty.Address;
            msg.port = endpointProperty.Port;
        }

        // Since this is not a service operation only server can call

        public Message GetMessage()
        {
            return BlockingQ.deQ();
        }

        public void CreateChannel(Uri uri)
        {
            WSHttpBinding binding = new WSHttpBinding();
            Uri address = uri;
            host = new ServiceHost(typeof(ReceiverService), address);
            host.AddServiceEndpoint(typeof(ICommService), binding, address);
        }

    }

    public class SenderService
    {
        ICommService channel;
        BlockingQueue<Message> senderQueue;
        Thread senderThread;

        public SenderService(string url)
        {
            senderQueue = new BlockingQueue<Message>();
            EndpointAddress address = new EndpointAddress(url);
            WSHttpBinding binding = new WSHttpBinding();
            channel = ChannelFactory<ICommService>.CreateChannel(binding, address);

            senderThread = new Thread(new ThreadStart(ThreadProc));
        }
        private void ThreadProc()
        {
            while (true)
            {
                Message msg = senderQueue.deQ();
                channel.PostMessage(msg);
                if (msg.command == Message.Command.Shutdown && msg.text == "local")
                    break;
            }
        }
        public void Start()
        {
            senderThread.Start();
        }
        public void PostMessage(Message msg)
        {
            senderQueue.enQ(msg);
        }
        public void Stop()
        {
            Message msg = new Message();
            msg.command = Message.Command.Shutdown;
            msg.text = "local";
            senderQueue.enQ(msg);

            senderThread.Join();
        }
    }
}
