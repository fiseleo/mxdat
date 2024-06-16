using System;
using System.Collections;
using System.IO.Compression;
using System.Text;
using mxdat.NetworkProtocol;


namespace mxdat
{
    public class PacketCryptManager
    {
        private static readonly short PROTOCOL_HEAD_RESERVE = 8;
        private readonly XORCryptor _cryptor = new();
        private readonly FastCRC _checke = new();
        private ProtocolConverter _converter = new();
        public static PacketCryptManager Instance = new();

        public PacketCryptManager()
        {

        }
        public byte[] RequestToBinary(Protocol protocol, string json)
        {
            byte[] compressedData = Instance.Compress(json);



            Console.WriteLine("Compress()得到:");
            foreach (byte b in compressedData)
            {
                Console.Write(b.ToString("X2") + " ");
            }
            Console.WriteLine();



            _cryptor.Encrypt(compressedData, 0, compressedData.Length);



            Console.WriteLine("Encypt()得到:");
            foreach (byte b in compressedData)
            {
                Console.Write(b.ToString("X2") + " ");
            }
            Console.WriteLine();



            //int totalLength = compressedData.Length + PROTOCOL_HEAD_RESERVE + 4;
            int totalLength = compressedData.Length + PROTOCOL_HEAD_RESERVE;
            byte[] result = new byte[totalLength];
            using (var memoryStream = new MemoryStream(result))
            {
                using (var binaryWriter = new BinaryWriter(memoryStream))
                {
                    uint crc;
                    _checke.GetCRC(compressedData, 0, compressedData.Length, out crc);



                    Console.WriteLine("crc值为" + crc);
                    foreach (byte b in BitConverter.GetBytes(crc))
                    {
                        Console.Write(b.ToString("X2") + " ");
                    }
                    Console.WriteLine();



                    int protocolConverter = _converter.TypeConversion(crc, protocol);



                    Console.WriteLine("protocolConverter值为" + protocolConverter);
                    foreach (byte b in BitConverter.GetBytes(protocolConverter))
                    {
                        Console.Write(b.ToString("X2") + " ");
                    }
                    Console.WriteLine();



                    byte[] ByteConverter = new byte[8];
                    binaryWriter.Write(crc);
                    binaryWriter.Write(protocolConverter);
                    binaryWriter.Write(compressedData);
                }
            }
            foreach (byte b in result)
            {
                Console.Write(b.ToString("X2") + " ");
            }
            Console.WriteLine();

            return result;

        }
        protected byte[] Compress(string text)
        {
            byte[] inputBytes = Encoding.UTF8.GetBytes(text);
            using (var memoryStream = new MemoryStream())
            {
                using (var gzipStream = new GZipStream(memoryStream, CompressionMode.Compress, true))
                {
                    gzipStream.Write(inputBytes, 0, inputBytes.Length);
                }
                byte[] compressedData = memoryStream.ToArray();
                using (var finalStream = new MemoryStream())
                {
                    byte[] lengthBytes = BitConverter.GetBytes(inputBytes.Length);
                    //Console.WriteLine(lengthBytes.Length);  //length为4
                    //Console.WriteLine(compressedData.Length);
                    finalStream.Write(lengthBytes, 0, lengthBytes.Length);
                    finalStream.Write(compressedData, 0, compressedData.Length);
                    return finalStream.ToArray();
                }
            }
        }


    }
}