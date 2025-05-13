namespace Kit.Services.Interfaces
{
    public interface IDataShare
    {
        Task<bool> CheckStoragePermission();
        void ShareFile(string absolutePath);
        void ShowFile(string AttachmentName, byte[] AttachmentBytes);

        void ShowFile(string title, string message, string filePath);
        void ShareFile(string title, string message, string FileName, MemoryStream fileData);
        void ShareFiles(string title, string message, List<Tuple<string, byte[]>> Files);
        void ShowFiles(string title, string message, List<string> archivos);
    }
}
