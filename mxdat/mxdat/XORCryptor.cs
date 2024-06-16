using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mxdat
{
    internal class XORCryptor
    {
        private readonly uint ENCRYPTION_KEY = 2948064217;
        //byte[] key = [0xD9];

        public bool Encrypt(byte[] data, int offset, int length)
        {
            for (int i = offset; i < offset + length; i++)
            {
                data[i] ^= (byte)ENCRYPTION_KEY;
                //data[i] ^= key[i % key.Length];
            }
            return true;
        }

        public XORCryptor()
        {

        }
    }
}
