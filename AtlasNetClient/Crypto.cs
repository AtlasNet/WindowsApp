using Org.BouncyCastle.Crypto;
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
            return (AsymmetricKeyParameter)pemReader.ReadObject();
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
    }
}
