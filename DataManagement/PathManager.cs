namespace DataManagement
{
    /// <summary>
    /// Path helper, checks if read/write directories exists and create them
    /// </summary>
    public static class PathManager
    {
        private static void CreatePath(string path) => Directory.CreateDirectory(path);
        public static void CreateReadFolderPath() => CreatePath(Constants.FolderReadPath);
        public static void CreateWriteFolderPath() => CreatePath(Constants.FolderWritePath);
    }
}
