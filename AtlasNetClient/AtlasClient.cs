using Newtonsoft.Json;
using Org.BouncyCastle.Crypto.Digests;
using Org.BouncyCastle.Crypto.Encodings;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Paddings;
using Org.BouncyCastle.Crypto.Signers;
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

            [DataMember]
            public string sender_key;
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

        public string PrepareMessage(Message msg, bool sign, BackgroundWorker.ProgressMessage OnProgress)
        {
            OnProgress = OnProgress ?? delegate { };

            var payload = new MessagePayload();
            payload.type = "text";
            payload.blob = Convert.ToBase64String(Encoding.UTF8.GetBytes(msg.Text));
            payload.timestamp = (int)(msg.Date - new DateTime(1970, 1, 1, 0, 0, 0, 0)).TotalSeconds;

            if (sign)
            {
                OnProgress(false, "Signing");
                var sig = SignerUtilities.GetSigner("RSASSA-PSS");
                sig.Init(true, Crypto.ReadKey(App.Instance.Config.PrivateKey));
                var bytes = Encoding.UTF8.GetBytes(msg.Text);
                sig.BlockUpdate(bytes, 0, bytes.Length);
                payload.signature = Convert.ToBase64String(sig.GenerateSignature());
                payload.sender_key = Crypto.SanitizeKey(App.Instance.Config.PublicKey);
            }

            var payloadJson = JsonConvert.SerializeObject(payload);

            var iv = new byte[16];
            var key = new byte[32];
            new SecureRandom().NextBytes(iv);
            new SecureRandom().NextBytes(key);

            var aes = new AesManaged();
            aes.Padding = PaddingMode.PKCS7;
            aes.KeySize = 256;
            aes.Mode = CipherMode.CBC;
            aes.Key = key;
            aes.IV = iv;

            OnProgress(false, "Encrypting");
            var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
            var memory = new MemoryStream();
            var cryptoStream = new CryptoStream(memory, encryptor, CryptoStreamMode.Write);
            using (var sw = new StreamWriter(cryptoStream))
            {
                sw.Write(payloadJson);
                sw.Flush();
                cryptoStream.FlushFinalBlock();
            }
            cryptoStream.Close();

            OnProgress(false, "Packing");
            var package = new MessagePackage();
            package.data = Convert.ToBase64String(memory.ToArray());
            package.iv = Convert.ToBase64String(Crypto.Encrypt(iv, Crypto.ReadKey(msg.ContactKey)));
            package.key = Convert.ToBase64String(Crypto.Encrypt(key, Crypto.ReadKey(msg.ContactKey)));
            package.recipient_key = msg.ContactKey;

            return JsonConvert.SerializeObject(package);
        }

        public Message DecryptMessage(AtlasMessage msg)
        {
            var package = JsonConvert.DeserializeObject<MessagePackage>(msg.Data);

            var aes = new AesManaged();
            aes.Padding = PaddingMode.PKCS7;
            aes.KeySize = 256;
            aes.Mode = CipherMode.CBC;
            aes.Key = Crypto.Decrypt(Convert.FromBase64String(package.key), Crypto.ReadKey(App.Instance.Config.PrivateKey));
            aes.IV = Crypto.Decrypt(Convert.FromBase64String(package.iv), Crypto.ReadKey(App.Instance.Config.PrivateKey));


            var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
            var memory = new MemoryStream(Convert.FromBase64String(package.data));
            var cryptoStream = new CryptoStream(memory, decryptor, CryptoStreamMode.Read);

            string payloadJson = "";
            using (var sr = new StreamReader(cryptoStream))
            {
                payloadJson = sr.ReadToEnd();
            }
            cryptoStream.Close();

            var payload = JsonConvert.DeserializeObject<MessagePayload>(payloadJson);
            var result = new Message();
            result.Text = Encoding.UTF8.GetString(Convert.FromBase64String(payload.blob));
            result.Date = new DateTime(1970, 1, 1, 0, 0, 0, 0).AddSeconds(payload.timestamp).ToUniversalTime();
            result.Read = false;
            result.ContactKey = null;

            if (!string.IsNullOrEmpty(payload.sender_key))
            {
                var signedContent = Convert.FromBase64String(payload.blob);
                foreach (var contact in App.Instance.Config.Contacts)
                    if (contact.PublicKey != null && Crypto.SanitizeKey(contact.PublicKey) == Crypto.SanitizeKey(payload.sender_key))
                    {
                        var sig = SignerUtilities.GetSigner("RSASSA-PSS");
                        sig.Init(false, Crypto.ReadKey(contact.PublicKey));
                        sig.BlockUpdate(signedContent, 0, signedContent.Length);
                        if (sig.VerifySignature(Convert.FromBase64String(payload.signature)))
                        {
                            result.ContactKey = contact.PublicKey;
                            result.SignatureVerified = true;
                            break;
                        }
                    }
            }
            else
            {
                result.SignatureVerified = true;
            }

            return result;
        }
    }
}
