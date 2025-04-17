namespace GameDevKit.DataPersistence
{
    public class NoObfuscationStrategy : SavePathObfuscationStrategy
    {
        public override string Obfuscate(string filePath) => filePath;
        public override string Deobfuscate(string filePath) => filePath;
    }
}