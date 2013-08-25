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
        public bool Authenticated { get; private set; }
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
            TestConnection();
            NodeInfo = Client.getInfo();
            Console.WriteLine("Connected to {0}", NodeInfo.GetName());
        }

        public void TestConnection()
        {
            Client.ping();
        }

        public void Send(Message message, string packageJson)
        {
            Client.postMessage(new AtlasMessage { Data = packageJson, RecipientKey = message.ContactKey });
        }

        public void AuthenticateIfNeeded()
        {
            if (!Authenticated)
                Authenticate();
        }

        public void Authenticate()
        {
            var challenge = Convert.FromBase64String(Client.getAuthChallenge(App.Instance.Config.PublicKey));
            Authenticated = Client.confirmAuth(Convert.ToBase64String(Crypto.Decrypt(challenge, Crypto.ReadKey(App.Instance.Config.PrivateKey)))) == 1;
        }

        public List<AtlasListing> GetListings()
        {
            AuthenticateIfNeeded();
            return Client.getListings();
        }

        public AtlasMessage RetrieveMessage(long id)
        {
            AuthenticateIfNeeded();
            if (Client.hasMessage(id) == 0)
                return null;
            return Client.retrieveMessage(id);
        }

        public void Disconnect()
        {
            Transport.Close();
        }
    }
}
