namespace DataManagement
{
    /// <summary>
    /// Path helper, checks if read/write directories exists and create them
    /// </summary>
    public static class PathManager
    {
        public static void CreatePath(string path) => Directory.CreateDirectory(path);
    }
}
