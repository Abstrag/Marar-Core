using Marar.Core.Linker;
using System.Text;

namespace Marar.Shell
{
    internal static class MainShell
    {
        private static MainLinker? Archive = null;
        private static string[] Args = [];
        public static ILinkerTrace LinkerTrace;

        private static int Handler()
        {
            switch (Args[0])
            {
                case "-e":
                    Encode();
                    break;
                case "-v":
                    Version();
                    break;
                case "-d":
                    Decode();
                    break;
                case "-h":
                    Help();
                    break;
                case "-p":
                    PrintPrimaryHeader();
                    break;
                case "-f":
                    PrintFileSystem();
                    break;
                default:
                    Console.WriteLine("Invalid args");
                    break;
            }

            return 0;
        }
        public static int Run(string[] args)
        {
            if (args.Length == 0)
            {
                Help();
                return 0;
            }
            else
            {
                Args = args;
                try
                {
                    return Handler();
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Undefined error: {e.Message}");
                    return -1;
                }
            }
        }
        private static void Version()
        {
            Console.WriteLine($"Marar Archivator. Version: {Launcher.CommonVersion}");
        }
        private static void Help()
        {
            Console.WriteLine(
                """
                Usage: [/arg1] [/args2] [path1] [path2]

                List of arguments 1:
                    -e - encode files from directory (path 1) to archive (path 2). Can request cryptographic key
                        -C - use cryptography to encrypt file system
                        -c - use cryptography to encrypt files
                        -t - write creation date-time of file
                        -l - use 64-bit number to write size of file]
                    -d - decode files from archive (path 1) to directory (path 2). Can request cryptographic key
                        -i - ignore wrong signature
                    -v - print current version of program
                    -h - print this message
                    -p - print primary header of archive (path 1)
                        -i - ignore wrong signature
                    -f - print file system of archive (path 1)
                        -i - ignore wrong signature

                """);
        }
        private static void PrintPrimaryHeader()
        {
            FileStream archive = new(Args[^1], FileMode.Open);
            Archive = new(archive, LinkerTrace);
            Archive.ReadPrimaryHeader(Args[1] == "-i");
            Console.WriteLine($"Version: {Archive.Version}");
            Console.WriteLine($"Creation date-time: {Archive.CreationDateTime.ToString()}");
            Console.WriteLine($"Using time: {Archive.UseTime}");
            Console.WriteLine($"Using large mode: {Archive.LargeMode}");
            Console.WriteLine($"Encription file system: {Archive.UseCryptoFS}");
            Console.WriteLine($"Encription files: {Archive.UseCrypto}");
        }
        private static void PrintFileSystem()
        {
            //Console.WriteLine("Black hole: it is not implemented");
            //return;
            /*void BindPath(int number)
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
            }*/
            FileStream archive = new(Args[^1], FileMode.Open);
            Archive = new(archive, LinkerTrace);
            Archive.ReadFS();

            void printLine(int number, int bufferLength)
            {
                for (int i = 1; i < Archive.FileHeader.Directories.Count; i++)
                {
                    if (Archive.FileHeader.Directories[i].Address == number)
                    {
                        Console.WriteLine('D' + new string(' ', bufferLength) + Archive.FileHeader.Directories[i].Name);
                        printLine(i, bufferLength + 1);
                    }
                }
                for (int i = 0; i < Archive.FileHeader.Files.Count; i++)
                {
                    if (Archive.FileHeader.Directories[i].Address == number) Console.WriteLine('F' + new string(' ', bufferLength) + Archive.FileHeader.Directories[i].Name);
                }
            }

            Console.WriteLine("File system:");
            printLine(0, 0);
        }
        private static void Encode()
        {
            FileStream archive = new(Args[^1], FileMode.Create);
            Archive = new(archive, LinkerTrace);

            for (int i = 1; i < Args.Length - 2; i++)
            {
                switch (Args[i])
                {
                    case "-C":
                        Archive.UseCryptoFS = true;
                        break;
                    case "-c":
                        Archive.UseCrypto = true;
                        break;
                    case "-l":
                        Archive.LargeMode = true;
                        break;
                    case "-t":
                        Archive.UseTime = true;
                        break;
                }
            }

            if (Archive.UseCrypto || Archive.UseCryptoFS)
            {
                Console.Write("Enter cryptographic key: ");
                Archive.SetCrypto(Encoding.UTF8.GetBytes(Console.ReadLine() ?? ""));
                (int a, int b) = Console.GetCursorPosition();
                Console.SetCursorPosition(0, b - 1);
                Console.WriteLine(new string('*', Console.BufferWidth));
            }

            Archive.LinkTo(Args[^2]);
            archive.Close();
            Console.WriteLine("Succesful encoding");
        }
        private static void Decode()
        {
            Console.WriteLine("Start decoding");
            FileStream archive = new(Args[^2], FileMode.Open);
            Archive = new(archive, LinkerTrace);
            Archive.LinkFrom(Args[^1], Args[1] == "-i");
            archive.Close();
            Console.WriteLine("Succesful decoding");
        }
    }
}
