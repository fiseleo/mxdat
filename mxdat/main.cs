namespace mxdat
{
    public class main
    {
        public static void Main(string[] args)
        {
            if (!File.Exists(@"C:\ba\mx.dat"))
            {
                Console.WriteLine("C:\\ba沒有mx.dat");
                return;
            }
            else
            {
                Decryptmxdat.DecryptMain(args);
            }
        }
    }
}
