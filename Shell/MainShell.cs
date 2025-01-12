using Marar.Core.Linker;

namespace Marar.Shell
{
    internal static class MainShell
    {
        private static MainLinker? Archive = null;
        private static string[] Args = [];

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
            Console.WriteLine("""
                Marar Archivator. Version: 0.0.1 alpha
                """);
        }
        private static void Help()
        {

        }
        private static void Encode()
        {

        }
        private static void Decode()
        {

        }
        private static void PrintFSHeader()
        {

        }
    }
}
