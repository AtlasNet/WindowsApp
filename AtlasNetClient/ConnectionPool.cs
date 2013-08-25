using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AtlasNetClient
{
    public class ConnectionPool
    {
        private Dictionary<AtlasNodeDescriptor, NodeClient> pool = new Dictionary<AtlasNodeDescriptor, NodeClient>();

        public NodeClient this[AtlasNodeInfo info]
        {
            get
            {
                var desc = info.GetDescriptor();
                if (!pool.ContainsKey(desc))
                    RecreateConnection(info);
                else
                {
                    try
                    { pool[desc].TestConnection(); }
                    catch
                    {
                        // Retry
                        try
                        { RecreateConnection(info); }
                        catch { }
                    }
                }
                if (pool.ContainsKey(desc))
                    return pool[desc];
                else
                    return null;
            }
        }

        public void RecreateConnection(AtlasNodeInfo info)
        {
            var desc = info.GetDescriptor();
            if (pool.ContainsKey(desc))
            {
                try
                {
                    pool[desc].Disconnect();
                }
                catch { }
                pool.Remove(desc);
            }
            var client = new NodeClient(info);
            client.Connect();
            pool[desc] = client;
        }
    }
}
