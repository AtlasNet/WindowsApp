using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace AtlasNetClient
{
    [DataContract]
    public class Message
    {
        [DataMember]
        public string Text { get; set; }

        [DataMember]
        public bool Outgoing;

        [DataMember]
        public string ContactKey { get; set; }

        [DataMember]
        public DateTime Date { get; set; }

        [DataMember]
        public bool Read { get; set; }

        public Contact Contact
        {
            get { return App.Instance.Config.Contacts.FirstOrDefault((x) => x.PublicKey == ContactKey); }
        }
    }
}
