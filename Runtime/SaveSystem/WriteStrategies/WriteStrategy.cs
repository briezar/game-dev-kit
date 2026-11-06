namespace GameDevKit.DataPersistence
{
    public abstract class WriteStrategy
    {
        /// <summary>
        /// Writes the first <paramref name="length"/> bytes of <paramref name="buffer"/> as the whole file and blocks until done.
        /// Runs on a background thread, so it must not use Unity APIs. The buffer is reused afterwards, so do not keep it.
        /// </summary>
        public abstract void Write(string path, byte[] buffer, int length);
    }
}