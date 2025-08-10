using System;

namespace GameDevKit.DataPersistence
{
    public class XOR_EncryptionStrategy : EncryptionStrategy
    {
        public static readonly string DefaultKey = "SaveSystem";
        private readonly string _key;
        public XOR_EncryptionStrategy(string key = null)
        {
            _key = key ?? DefaultKey;
        }

        public override string Encrypt(string data) => Encryption.XOR(data, _key);
        public override string Decrypt(string data) => Encryption.XOR(data, _key);
    }
}