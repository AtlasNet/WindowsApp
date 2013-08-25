using Newtonsoft.Json;
using Org.BouncyCastle.Security;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Cryptography;
using System.Text;

namespace AtlasNetClient
{
    public class AtlasClient
    {
        [DataContract]
        private class MessagePayload
        {
            [DataMember]
            public string type;

            [DataMember]
            public string blob;

            [DataMember]
            public int timestamp;

            [DataMember]
            public string signature;
        }

        [DataContract]
        private class MessagePackage
        {
            [DataMember]
            public string data;

            [DataMember]
            public string key;

            [DataMember]
            public string iv;

            [DataMember]
            public string recipient_key;
        }

        public string PrepareMessage(Message msg, bool sign)
        {
            var payload = new MessagePayload();
            payload.type = "text";
            payload.blob = Convert.ToBase64String(Encoding.UTF8.GetBytes(msg.Text));
            payload.timestamp = (int)(msg.Date - new DateTime(1970, 1, 1, 0, 0, 0, 0)).TotalSeconds;

            if (sign)
            {
                var sig = SignerUtilities.GetSigner("SHA1withRSA");
                sig.Init(true, Crypto.ReadKey(App.Instance.Config.PrivateKey));
                var bytes = Encoding.UTF8.GetBytes(msg.Text);
                sig.BlockUpdate(bytes, 0, bytes.Length);
                payload.signature = Convert.ToBase64String(sig.GenerateSignature());
            }

            var payloadJson = JsonConvert.SerializeObject(payload);

            var iv = new byte[32];
            var key = new byte[32];
            new SecureRandom().NextBytes(iv);
            new SecureRandom().NextBytes(key);

            var aes = new RijndaelManaged();
            aes.BlockSize = 256;
            aes.Key = key;
            aes.IV = iv;

            var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
            var memory = new MemoryStream();
            var cryptoStream = new CryptoStream(memory, encryptor, CryptoStreamMode.Write);
            using (var sw = new StreamWriter(cryptoStream))
                sw.Write(payloadJson);
            cryptoStream.Close();

            var package = new MessagePackage();
            package.data = Convert.ToBase64String(memory.ToArray());
            package.iv = Convert.ToBase64String(Crypto.Encrypt(iv, Crypto.ReadKey(msg.ContactKey)));
            package.key = Convert.ToBase64String(Crypto.Encrypt(key, Crypto.ReadKey(msg.ContactKey)));
            package.recipient_key = msg.ContactKey;

            return JsonConvert.SerializeObject(package);
        }
    }
}
