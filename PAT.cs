using System;
using System.Globalization;

namespace ExtractCamera
{
    public static class Pat
    {
        private const byte ByteLen = 0XD;
        private const byte ProgramId = 0X1;

        private static void AddCrc(ref byte[] data, uint crc)
        {
            var crcArray = BitConverter.GetBytes(crc);

            for (var i = 0; i < 4; i++)
            {
                data[20 - i] = crcArray[i];
            }
        }

        private static void AddEmpty(ref byte[] data)
        {
            for (var i = 21; i < data.Length; i++)
            {
                data[i] = 0Xff;
            }
        }

        public static void RewritePat(ref byte[] data, byte pmt)
        {
            data[7] = ByteLen;
            data[14] = ProgramId;
            data[16] = pmt;
            var changedPart = new byte[12];
            Array.Copy(data, 5, changedPart, 0, 12);

            var crc = Crc32.GetHashCode(changedPart); 
            AddCrc(ref data, crc);
            AddEmpty(ref data);
        }

    }
}
