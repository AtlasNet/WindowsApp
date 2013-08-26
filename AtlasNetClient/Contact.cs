using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace AtlasNetClient
{
    [DataContract]
    public class Contact
    {
        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public string PublicKey { get; set; }

        public bool IsAnonymous
        {
            get { return PublicKey == null; }
        }
    }
}
