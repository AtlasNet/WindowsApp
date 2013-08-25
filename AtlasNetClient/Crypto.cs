using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Digests;
using Org.BouncyCastle.Crypto.Encodings;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.OpenSsl;
using Org.BouncyCastle.Security;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace AtlasNetClient
{
    public static class Crypto
    {
        public static AsymmetricKeyParameter ReadKey(string data)
        {
            var pemReader = new PemReader(new StringReader(data));
            var obj = pemReader.ReadObject();
            if (obj is AsymmetricCipherKeyPair)
                return (obj as AsymmetricCipherKeyPair).Private;
            return (AsymmetricKeyParameter)obj;
        }

        public static string SaveKey(AsymmetricKeyParameter key)
        {
            var writer = new StringWriter();
            new PemWriter(writer).WriteObject(key);
            return writer.ToString();
        }

        public static AsymmetricCipherKeyPair GenerateKey(int bits)
        {
            var gen = new RsaKeyPairGenerator();
            gen.Init(new KeyGenerationParameters(new SecureRandom(), bits));
            return gen.GenerateKeyPair();
        }

        public static byte[] Encrypt(byte[] data, AsymmetricKeyParameter key)
        {
            var cipher = new OaepEncoding(new RsaEngine());
            cipher.Init(true, key);
            return cipher.ProcessBlock(data, 0, data.Length);
        }

        public static byte[] Decrypt(byte[] data, AsymmetricKeyParameter key)
        {
            var cipher = new OaepEncoding(new RsaEngine());
            cipher.Init(false, key);
            return cipher.ProcessBlock(data, 0, data.Length);
        }
    }
}
