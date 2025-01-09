namespace MararCore.Linker
{
    internal class ElementFrame(uint address, int creationDate, string name)
    {
        public uint Address = address;
        public int CreationDate = creationDate;
        public string Name = name;
    }
    internal class FSHeader(string path, bool largeMode)
    {
        private bool LargeMode = largeMode;
        private string RootDirectory = path;
        public List<ElementFrame> Directories = new();
        public List<ElementFrame> Files = new();
        
        private void AppendAll(string path, uint number)
        {
            string[] paths = Directory.GetFiles(path);
            for (uint i = 0; i < paths.Length; i++)
            {
                FileInfo file = new(paths[i]);
                Files.Add(new(number, DateTimeConverter.EncodeDateTime(file.CreationTime), file.Name));
            }

            paths = Directory.GetDirectories(path);
            for (uint i = 0; i < paths.Length; i++)
            {
                DirectoryInfo directory = new(paths[i]);
                Directories.Add(new(number, DateTimeConverter.EncodeDateTime(directory.CreationTime), directory.Name));
                AppendAll(directory.FullName, Directories.Count);
            }
        }
        public void InitFS()
        {

        }
    }
}
