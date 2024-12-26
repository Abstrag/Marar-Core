
namespace MararCore.Compress.Arithmetic
{
    public class IntArithmeticCompressor : FileProcessor
    {
        private const byte CodeLength = 16;
        private const byte Base = 2;
        private const byte BitMove = 1;
        private static readonly ulong MaxCode = (ulong)(MathF.Pow(Base, CodeLength) - 1);
        private static readonly ulong FirstQtr = MaxCode / 4;
        private static readonly ulong Half = MaxCode / 2;
        private static readonly ulong ThirdQtr = MaxCode * 3 / 4;
        private Tuple<double, double>[] FloatDictionary = new Tuple<double, double>[256];

        public IntArithmeticCompressor(Stream input, Stream output) : base(input, output) { }

        private void InitDictionary()
        {

        }
        private ulong Bml(ulong num1, int shift, ulong num2)
        {
            return (num1 << shift) & num2;
        }
        private ulong Bmlt(ulong num1, int shift, ulong num2)
        {
            num1 <<= shift;
            for (int i = 1; i < shift; i++)
            {
                num1 |= (ulong)1 << i;
            }
            return num1 & num2;
        }
        private ulong BitsPlusFollow()
        {

        }

        public override void Encode()
        {
            
        }

    }
}
