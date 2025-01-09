namespace MararCore.Linker
{
    internal class LinkTo
    {
        private const uint Signature = 0x4DA072AF;
        private const byte GrandVersion = 0;
        private const byte Version = 0;
        private Stream Output;
        
        public LinkTo(Stream output)
        {
            Output = output;
        }
        

    }
}
