using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Thrift.Protocol;
using Thrift.Transport;

namespace AtlasNetClient
{
    public class NodeClient
    {
        private AtlasNode.Client Client;
        private TTransport Transport;
        public AtlasNodeInfo NodeInfo;

        public NodeClient(string host, int port)
        {
            Transport = new TBufferedTransport(new TSocket(host, port));
            TProtocol protocol = new TBinaryProtocol(Transport);
            Client = new AtlasNode.Client(protocol);
        }

        public NodeClient(AtlasNodeInfo info)
        {
            Transport = new TBufferedTransport(new TSocket(info.Host, (int)info.Port));
            TProtocol protocol = new TBinaryProtocol(Transport);
            Client = new AtlasNode.Client(protocol);
        }

        public void Connect()
        {
            Transport.Open();
            Client.ping();
            NodeInfo = Client.getInfo();
            Console.WriteLine("Connected to {0}", NodeInfo.GetName());
        }

        public void Disconnect()
        {
            Transport.Close();
        }
    }
}
