namespace MararCore
{
    public class FindDifferences
    {
        public Stream Input { get; set; }
        public Stream Output { get; set; }

        public FindDifferences(Stream input, Stream output)
        {
            Input = input;
            Output = output;
        }

        public void PrintDiffernces()
        {
            if (Input.Length != Output.Length)
            {
                Logging.WriteLine("Different lengths");
            }
            while (Input.Position < Input.Length)
            {
                byte ib = (byte)Input.ReadByte();
                byte ob = (byte)Output.ReadByte();

                if (ib != ob)
                {
                    Logging.WriteLine($"{ib} and {ob} in {Input.Position - 1}");
                }
            }
        }
    }
}
