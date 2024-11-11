using Newtonsoft.Json.Linq;
using RestSharp;
using Ionic.Zip;
using SCHALE.Common.Crypto;
using System.Text;

namespace mxdat
{
    public class GetExcelzip
    {
        public static void GetExcelzipMain(string[] args)
        {
            try
            {
                // Register the code page provider to support IBM437 encoding
                Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

                string rootDirectory = AppDomain.CurrentDomain.BaseDirectory;
                string resourcejsonFilePath = Path.Combine(rootDirectory, "resource.json");
                string ExeclzipPath = Path.Combine(rootDirectory, "Excel.zip");
                string targetDirectoryPath = Path.Combine(rootDirectory,"extracted");
                if (!Directory.Exists(targetDirectoryPath))
                {
                    Directory.CreateDirectory(targetDirectoryPath);
                }
                else
                {
                    Console.WriteLine("Directory already exists");
                }


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
                    return;
                }

                // Unzip the Excel.zip using DotNetZip
                using (var zip = new ZipFile(ExeclzipPath))
                {
                    zip.Password = Convert.ToBase64String(TableService.CreatePassword(Path.GetFileName(ExeclzipPath)));
                    zip.ExtractAll(targetDirectoryPath, ExtractExistingFileAction.OverwriteSilently);
                }
                
                Console.WriteLine("Excel files extracted successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An exception occurred: {ex.Message}");
            }

            pythonScipt.pythonSciptMain(args);
        }
    }
}
