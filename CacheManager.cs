namespace MararCore
{
    public static class CacheManager
    {
        private static Random NameGenerator = new();
        public static string RootDirectory;
        public static string CurrentDirectory;

        public static void GlobalClear()
        {
            Directory.Delete(RootDirectory, true);
            Directory.CreateDirectory(RootDirectory);
        }
        public static void InitManager()
        {
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
