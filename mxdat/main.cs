namespace mxdat
{
    public class main
    {
        public static void Main(string[] args)
        {
            string rootDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string mxDirectory = Path.Combine(rootDirectory,"mx");
            string filePath = Path.Combine(rootDirectory, mxDirectory, "mx.dat");
            if (!Directory.Exists(mxDirectory))
            {
                Directory.CreateDirectory(mxDirectory);
            }
            else
            {
                Console.WriteLine("Directory already exists");
            }

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
