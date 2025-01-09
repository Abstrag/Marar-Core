namespace MararCore.Linker
{
    internal class LargeModeException(string path, long length) : Exception
    {
        public string Path = path;
        public long Length = length;
    }

    internal class DirectoryFrame(uint address, DateTime creationDate, string name)
    {
        public uint Address = address;
        public DateTime CreationDate = creationDate;
        public string Name = name;
    }
    internal class FileFrame(uint address, DateTime creationDate, string name, long length)
    {
        public uint Address = address;
        public DateTime CreationDate = creationDate;
        public string Name = name;
        public long Length = length;
    }
    internal class FSHeader(string path, bool largeMode)
    {
        private readonly string RootDirectory = path;
        private readonly bool LargeMode = largeMode;
        public List<Stream> FileStreams;
        public List<DirectoryFrame> Directories = new();
        public List<FileFrame> Files = new();
        
        private void AppendAll(string path, uint number)
        {
            string[] paths = Directory.GetFiles(path);
            for (uint i = 0; i < paths.Length; i++)
            {
                FileInfo file = new(paths[i]);
                Files.Add(new(number, file.CreationTime, file.Name, file.Length));
                FileStreams.Add(file.OpenRead());
            }

            paths = Directory.GetDirectories(path);
            for (uint i = 0; i < paths.Length; i++)
            {
                DirectoryInfo directory = new(paths[i]);
                Directories.Add(new(number, directory.CreationTime, directory.Name));
                AppendAll(directory.FullName, (uint)Directories.Count);
            }
        }
        public void InitFS()
        {
            AppendAll(RootDirectory, 0);
            if (!LargeMode)
            {
                foreach (FileFrame file in Files)
                {
                    if (file.Length > 0xFFFFFFFF) throw new LargeModeException(file.Name, file.Length);
                }
            }
        }
    }
}
