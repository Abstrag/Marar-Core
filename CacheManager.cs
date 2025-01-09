namespace MararCore
{
    internal static class CacheManager
    {
        private static Random NameGenerator = new();
        public static string RootDirectory;
        public static string CurrentDirectory;

        public static void InitManager()
        {
            int id = DateTime.Now.GetHashCode();
            CurrentDirectory = Path.Combine(RootDirectory, id.ToString());
            Directory.CreateDirectory(CurrentDirectory);
        }
        public static string GetNewFile()
        {
            int id = NameGenerator.Next();
            string fullPath = Path.Combine(CurrentDirectory, id.ToString());
            File.Create(fullPath);
            return fullPath;
        }
        public static void Flush()
        {
            Directory.Delete(CurrentDirectory, true);
        }
    }
}
