using System;

namespace GameDevKit.DataPersistence
{
    /// <summary> The stored hash does not match the file content: the file was edited or is corrupted. </summary>
    public class SaveTamperedException : Exception
    {
        public SaveTamperedException() : base("Save hash mismatch.") { }
        public SaveTamperedException(string message) : base(message) { }
    }
}