using System.IO.Compression;
using System.Text;


namespace mxdat
{
    public class Decryptmxdat
    {
        public static async void DecryptMain(string[] args)
        {
            string filePath = @"C:\ba\mx.dat";
            byte[] mx = File.ReadAllBytes(filePath);
            byte[] reqBytes = new byte[mx.Length - 12];
            Array.Copy(mx, 12, reqBytes, 0, mx.Length - 12);


            for (int i = 0; i < reqBytes.Length; i++)
            {
                reqBytes[i] ^= 0xD9;
            }


            byte[] decompressedBytes;
            using (MemoryStream input = new MemoryStream(reqBytes))
            using (GZipStream gzip = new GZipStream(input, CompressionMode.Decompress))
            using (MemoryStream output = new MemoryStream())
            {
                gzip.CopyTo(output);
                decompressedBytes = output.ToArray();
            }


            string jsonText = Encoding.UTF8.GetString(decompressedBytes);
            Console.WriteLine(jsonText);
            File.WriteAllText("mxdat.json", jsonText);

            await GetNexonServerjson.GetNexonServerjsonMain(args);
        }

        
     
    }
}