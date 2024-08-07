using Newtonsoft.Json.Linq;
using RestSharp;

namespace mxdat
{
    public class GetExcelzip
    {
        public static void GetExcelzipMain(string[] args)
        {
            try
            {
                string rootDirectory = AppDomain.CurrentDomain.BaseDirectory;
                string resourcejsonFilePath = Path.Combine(rootDirectory, "mxdatpy", "APK", "AddressablesCatalogUrlRoot.txt");
                string ExeclzipPath = Path.Combine(rootDirectory, "Excel.zip");
                string txtContent = File.ReadAllText(resourcejsonFilePath);
                string excelZipUrl = $"{txtContent}/TableBundles/Excel.zip";

                var client = new RestClient(excelZipUrl);
                var request = new RestRequest(Method.GET);
                IRestResponse response = client.Execute(request);

                if (response.IsSuccessful)
                {
                    byte[] fileBytes = response.RawBytes;
                    File.WriteAllBytes(ExeclzipPath, fileBytes);
                    Console.WriteLine("Excel.zip downloaded successfully");
                }
                else
                {
                    Console.WriteLine("Failed to download Excel.zip");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An exception occurred: {ex.Message}");
            }

            pythonScipt.pythonSciptMain(args);
        }
    }
}
