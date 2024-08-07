namespace mxdat
{
    public class main
    {
        public static void Main(string[] args)
        {
            string rootDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string MxDirectory = Path.Combine(rootDirectory, "mx");
            if (!Directory.Exists(MxDirectory))
            {
                Directory.CreateDirectory(MxDirectory);
                Console.WriteLine("Mx folder created");
            }
            else
            {
                Console.WriteLine("mx資料夾已存在");
            }
            string filePath = Path.Combine(MxDirectory, "mx.dat");
            if (!File.Exists(filePath))
            {
                Console.WriteLine("沒有mx.dat");
                return;
            }
            else
            {
                Decryptmxdat.DecryptMain(args);
            }
        }
    }
}
