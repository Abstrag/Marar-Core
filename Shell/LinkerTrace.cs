using Marar.Core.Linker;
using System.Text;

namespace Marar.Shell
{
    internal class LinkerTrace : ILinkerTrace
    {
        private Stream LogFile;

        public LinkerTrace(Stream logStream)
        {
            LogFile = logStream;
            WriteLine($"Common version: {Launcher.CommonVersion}");
            WriteLine($"Linker version: {Launcher.LinkerVersion}");
        }
        public LinkerTrace(string path) : this(new FileStream(path, FileMode.Open)) { }
        
        private void Write(string message)
        {
            LogFile.Write(Encoding.UTF8.GetBytes(message));
            LogFile.Flush();
        }
        private void WriteLine(string message)
        {
            Write(message + '\n');
        }
        public void Trace(LinkerCondition condition)
        {
            string message = "";

            switch (condition)
            {
                case LinkerCondition.PrimaryHeader:
                    message = "Writing primary header";
                    break;
                case LinkerCondition.FSHeader:
                    message = "Writing file system";
                    break;
                case LinkerCondition.Compress:
                    message = "Data compress";
                    break;
                case LinkerCondition.Crypto:
                    message = "Encrypting files";
                    break;
            }

            Console.WriteLine(message);
            WriteLine(message);
        }
    }
}
