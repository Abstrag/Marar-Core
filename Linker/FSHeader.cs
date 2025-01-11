namespace MararCore.Linker
{
    internal class LargeModeException(string path, long length) : Exception
    {
        public string Path = path;
        public long Length = length;
    }

    internal class DirectoryFrame(uint address, DateTime creationDate, string name, string? path = null)
    {
        public uint Address = address;
        public DateTime CreationDate = creationDate;
        public string Name = name;
        public string? ExternalPath = path;
    }
    internal class FileFrame(uint address, DateTime creationDate, long length, string name, string? path = null)
    {
        public uint Address = address;
        public DateTime CreationDate = creationDate;
        public string Name = name;
        public long Length = length;
        public string? ExternalPath = path;
    }
    internal class FSHeader()
    {
        public List<FileFrame> Files = new();
        public List<DirectoryFrame> Directories = new();

        private void AppendAll(string path, uint number)
        {
            string[] paths = Directory.GetFiles(path);
            for (uint i = 0; i < paths.Length; i++)
            {
                FileInfo file = new(paths[i]);
                Files.Add(new(number, file.CreationTime, file.Length, file.Name, file.FullName));
            }

            paths = Directory.GetDirectories(path);
            for (uint i = 0; i < paths.Length; i++)
            {
                DirectoryInfo directory = new(paths[i]);
                Directories.Add(new(number, directory.CreationTime, directory.Name));
                AppendAll(directory.FullName, (uint)Directories.Count);
            }
        }
        public void InitFS(string rootDirectory, bool largeMode)
        {
            AppendAll(rootDirectory, 0);
            if (!largeMode)
            {
                foreach (FileFrame file in Files)
                {
                    if (file.Length > 0xFFFFFFFF) throw new LargeModeException(file.Name, file.Length);
                }
            }
        }

        private void BindPath(int number)
        {          
            for (int i = 0; i < Files.Count; i++)
            {
                if (Files[i].Address == number) Files[i].ExternalPath = Path.Combine(Directories[number].ExternalPath, Files[i].Name);
            }
            for (int i = 1; i < Directories.Count; i++)
            {
                if (Directories[i].Address == number)
                {
                    Directories[i].ExternalPath = Path.Combine(Directories[number].ExternalPath, Directories[i].Name);
                    BindPath(i);
                }
            }
        }
        public void BindFS(string rootDirectory)
        {
            Directories.Insert(0, new(0, Directory.GetCreationTime(rootDirectory), Path.GetFileName(rootDirectory), rootDirectory));
            BindPath(0);
        }
    }
}
