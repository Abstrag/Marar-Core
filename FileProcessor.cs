namespace Marar.Core
{
    public abstract class FileProcessor
    {
        public Stream Input;
        public Stream Output;

        public FileProcessor(Stream input, Stream output)
        {
            Input = input;
            Output = output;
        }

        public abstract void Encode();
    }
}
