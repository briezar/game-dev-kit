using System;

namespace GameDevKit.DataPersistence
{
    public class NoEncryptionStrategy : EncryptionStrategy
    {
        public override string Encrypt(string data) => data;
        public override string Decrypt(string data) => data;
    }
}