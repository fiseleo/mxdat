using System;
namespace mxdat
{
    public class FastCRC
    {
        // Fields
        public static uint[] crc_table = new uint[256];
        public readonly uint POLYNOMIAL = 79764919;
        public readonly uint MASK = 2147483648;
        public FastCRC()
        {

        }
        private void Generate()
        {
            uint i;
            uint crcValue;  //v8
            int times;      //v9
            for (i = 0; i != 256; ++i)
            {
                crcValue = i << 24;
                times = 8;
                do
                {
                    if ((MASK & crcValue) != 0)
                    {
                        crcValue = POLYNOMIAL ^ (2 * crcValue);
                    }
                    else
                    {
                        crcValue *= 2;
                    }
                    --times;
                } while (times != 0);
                crc_table[i] = crcValue;
            }
        }

        public bool GetCRC(byte[] buffer, int offset, int length, out uint crc)
        {
            crc = 0;    //uVar4

            if (crc_table[0] == 0)
            {
                Generate();
            }
            /*foreach (uint b in crc_table)
            {
                Console.Write(b.ToString() + " ");
            }*/

            for (int i = offset; i < offset + length; i++)
            {
                byte b = buffer[i];
                crc = crc_table[(crc >> 24) ^ b] ^ (crc << 8);
            }
            return true;
        }
    }
}
