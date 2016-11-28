/////////////////////////////////////////////////////////////////////////
// ICommService.cs - ICommService contract                             //
//                                                                     //
// Jim Fawcett, CSE681 - Software Modeling and Analysis, Summer 2008   //
/////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

namespace TestHarness
{
    [ServiceContract]
    public interface ICommService
    {

        [OperationContract(IsOneWay = true)]
        void PostMessage(Message msg);

        // Not a service operation so only server can call

        Message GetMessage();
    }

    [DataContract]
    public class Message
    {
        [DataMember]
        Command cmd = Command.TestRequestSetupError;
        [DataMember]
        string body = "default message text";
        [DataMember]
        string addr = "";
        [DataMember]
        int prt = 0;
        [DataMember]
        string tkn = "nan";

        public enum Command
        {
            [EnumMember]
            TestRequestSetupError,
            [EnumMember]
            ConnectFileServer,
            [EnumMember]
            ConnectMe,
            [EnumMember]
            TestRequest,
            [EnumMember]
            Shutdown,
            [EnumMember]
            FileStreamSocketInfo,
            [EnumMember]
            FileListRequest,
            [EnumMember]
            FileListRespose,
            [EnumMember]
            FileListFromClient,
            [EnumMember]
            FileListFromClientRespose,
            [EnumMember]
            FileListFromClientResposeFromRepository,
            [EnumMember]
            LogFileListRequest,
            [EnumMember]
            LogFileListResponse,
            [EnumMember]
            LogFileRequest,
            [EnumMember]
            LogFileResponse
        }

        [DataMember]
        public Command command
        {
            get { return cmd; }
            set { cmd = value; }
        }

        [DataMember]
        public string text
        {
            get { return body; }
            set { body = value; }
        }

        [DataMember]
        public string address
        {
            get { return addr; }
            set { addr = value; }
        }

        [DataMember]
        public int port
        {
            get { return prt; }
            set { prt = value; }
        }

        [DataMember]
        public string token
        {
            get { return tkn; }
            set { tkn = value; }
        }

    }
}
