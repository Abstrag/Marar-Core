namespace MararCore
{
    public static class Extensions
    {
        public static double? IsSafeDivide(this double value, double divider)
        {
            double a = value / divider;
            if (Math.Abs(a * divider - value) < 0.001)
            {
                return a;
            }
            else return null;
        }
        public static double GetLength(this Tuple<double, double> tuple)
        {
            return tuple.Item2 - tuple.Item1;
        }
        public static bool IsLastTrue(this Tuple<double, double> tuple)
        {
            double length = tuple.GetLength();
            if(length > 5e-13) return false;
            else return true;
        }
        public static bool IsLast(this Tuple<double, double> tuple)
        {
            double length = tuple.GetLength();
            if (length > 0) return false;
            else return true;
        }
        public static bool ContainsIt(this Tuple<double, double> tuple, double num)
        {
            if(num >= tuple.Item1 && num < tuple.Item2) return true;
            else return false;
        }
        public static long GetLength(this Dictionary<byte, long> dictionary)
        {
            long result = 0;

            foreach (var key in dictionary.Keys)
            {
                result += dictionary[key];
            }

            return result;
        }
        public static byte GetByteKey(this Dictionary<byte, Tuple<double, double>> dictionary, Tuple<double, double> range)
        {
            foreach (var key in dictionary.Keys)
            {
                if (dictionary[key].Item1 == range.Item1 && dictionary[key].Item2 == range.Item2) return key;
            }
            throw new Exception($"This don't contain Range: {range}");
        }
        public static string ToString(this Tuple<double, double> tuple)
        {
            return $"{tuple.Item1}:{tuple.Item2}";
        }
        public static byte[] ReadBytes(this Stream stream, long bufferSize)
        {
            byte[] buffer = new byte[bufferSize];
            stream.ReadExactly(buffer);
            return buffer;
        }
    }
}
