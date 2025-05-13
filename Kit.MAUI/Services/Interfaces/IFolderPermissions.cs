namespace Kit.Services.Interfaces
{
    public abstract class IFolderPermissions
    {
        public async Task<bool> CanWrite(DirectoryInfo Path)
        {
            return await CanWrite(Path.FullName);
        }
        public async Task<bool> CanWrite(FileInfo Path)
        {
            return await CanWrite(Path.FullName);
        }
        public abstract Task<bool> CanWrite(string directoryInfo);

        public async Task<bool> CanRead(DirectoryInfo Path)
        {
            return await CanRead(Path.FullName);
        }
        public async Task<bool> CanRead(FileInfo Path)
        {
            return await CanRead(Path.FullName);
        }
        public abstract Task<bool> CanRead(string directoryInfo);

        public async Task<bool> IsWritableReadable(FileInfo Path)
        {
            return await IsWritableReadable(Path.FullName);
        }
        public async Task<bool> IsWritableReadable(DirectoryInfo Path)
        {
            return await IsWritableReadable(Path.FullName);
        }
        public async Task<bool> IsWritableReadable(string Path)
        {
            return await CanRead(Path) && await CanWrite(Path);
        }
        public async Task<bool> TryToUnlock(FileInfo Path)
        {
            return await TryToUnlock(Path.FullName);
        }
        public async Task<bool> TryToUnlock(DirectoryInfo Path)
        {
            return await TryToUnlock(Path.FullName);
        }
        public abstract Task<bool> TryToUnlock(string Path);
    }
}
