using Marar.Core.Linker;

namespace Marar.Shell
{
    internal class LinkerTrace : ILinkerTrace
    {
        private Stream LogFile;

        public LinkerTrace(Stream logFile)
        {
            LogFile = logFile;
        }

        public void Trace(LinkerCondition condition)
        {
            throw new NotImplementedException();
        }
    }
}
