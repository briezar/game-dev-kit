using System;

namespace GameDevKit.DataPersistence
{
    public abstract class EncryptionStrategy
    {
        public abstract string Encrypt(string data);
        public abstract string Decrypt(string data);
    }
}