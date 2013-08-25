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

        [DataMember]
        public States State { get; set; }

        public Contact Contact
        {
            get { return App.Instance.Config.Contacts.FirstOrDefault((x) => x.PublicKey == ContactKey); }
        }

        public string DisplaySender
        {
            get
            {
                if (ContactKey == null)
                    return "Anonymous";
                if (Contact == null)
                    return "Unknown";
                if (Outgoing)
                    return "Me";
                return Contact.Name;
            }
        }

        public enum States
        {
            Sending, Normal
        }
    }
}
