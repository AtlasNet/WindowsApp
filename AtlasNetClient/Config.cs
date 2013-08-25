using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.IO;
using System.Collections.ObjectModel;

namespace AtlasNetClient
{
    [DataContract]
    public class Config
    {
        [DataMember]
        public AtlasNodeInfo BootstrapNode { get; set; }

        [DataMember]
        public ObservableCollection<Contact> Contacts = new ObservableCollection<Contact>();

        [DataMember]
        public string PublicKey { get; set; }

        [DataMember]
        public string PrivateKey { get; set; }

        [DataMember]
        public List<Message> Messages { get; set; }

        public static Config Load(string path)
        {
            return JsonConvert.DeserializeObject<Config>(File.ReadAllText(path));
        }

        public void Save(string path)
        {
            File.WriteAllText(path, JsonConvert.SerializeObject(this));
        }
    }
}
