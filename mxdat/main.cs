namespace mxdat
{
    public class main
    {
        public static void Main(string[] args)
        {
            string rootDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string filePath = Path.Combine(rootDirectory,"mx", "mx.dat");
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
