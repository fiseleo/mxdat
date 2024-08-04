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
                string resourcejsonFilePath = Path.Combine(rootDirectory, "resource.json");
                string ExeclzipPath = Path.Combine(rootDirectory, "Excel.zip");
                string jsonContent = File.ReadAllText(resourcejsonFilePath);
                var jsonObject = JObject.Parse(jsonContent);
                string resourcePath = jsonObject["patch"].Value<string>("resource_path");

                string baseUrl = resourcePath.Substring(0, resourcePath.LastIndexOf("/") + 1);
                string excelZipUrl = $"{baseUrl}Preload/TableBundles/Excel.zip";

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
