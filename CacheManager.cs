namespace Marar.Core
{
    public static class CacheManager
    {
        private static Random NameGenerator = new();
        private static string RootDirectory;
        public static string CurrentDirectory { get; private set; }

        public static void GlobalFlush()
        {
            Directory.Delete(RootDirectory, true);
            Directory.CreateDirectory(RootDirectory);
        }
        public static void InitManager(string rootDirectory)
        {
            RootDirectory = rootDirectory;
            CurrentDirectory = Path.Combine(RootDirectory, DateTime.Now.GetHashCode().ToString());
            Directory.CreateDirectory(CurrentDirectory);
        }
        public static string GetNewFile()
        {
            int id = NameGenerator.Next();
            string fullPath = Path.Combine(CurrentDirectory, id.ToString());
            return fullPath;
        }
        public static void Flush()
        {
            Directory.Delete(CurrentDirectory, true);
        }
    }
}
