using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MararCore.Compress.RANS
{
    internal class RANSGPT
    {
        private int _maxValue;
        private int _minValue;
        private int _currentRange;
        private int _currentValue;
        private BitWriter _bitWriter;

        public RANSGPT(BitWriter bitWriter)
        {
            _bitWriter = bitWriter;
            Reset();
        }

        public void Reset()
        {
            _maxValue = int.MaxValue;
            _minValue = int.MinValue;
            _currentRange = _maxValue - _minValue;
            _currentValue = 0;
        }

        public void Encode(int value)
        {
            if (value < _minValue || value > _maxValue)
            {
                throw new ArgumentOutOfRangeException("Value is out of range");
            }

            _currentValue = _minValue + (value - _minValue) * _currentRange / (_maxValue - _minValue);

            while (_currentRange > 1)
            {
                int halfRange = _currentRange >> 1;
                if (_currentValue < _currentValue + halfRange)
                {
                    _bitWriter.WriteBit(0);
                    _currentRange = halfRange;
                }
                else
                {
                    _bitWriter.WriteBit(1);
                    _currentValue -= halfRange;
                    _currentRange -= halfRange;
                }
            }
        }
    }

    public class BitWriter
    {
        private byte[] _buffer;
        private int _bufferIndex;

        public BitWriter(int size)
        {
            _buffer = new byte[size];
            _bufferIndex = 0;
        }

        public void WriteBit(int bit)
        {
            if (_bufferIndex == _buffer.Length)
            {
                throw new InvalidOperationException("Buffer is full");
            }

            _buffer[_bufferIndex++] = (byte)(bit << 7);
        }

        public byte[] GetBuffer()
        {
            return _buffer;
        }

        public int GetBufferIndex()
        {
            return _bufferIndex;
        }
    }
}
