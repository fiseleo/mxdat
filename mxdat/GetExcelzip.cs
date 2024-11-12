using Newtonsoft.Json.Linq;
using RestSharp;
using System.Text;
using SCHALE.Common.Crypto;
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
                string targetDirectoryPath = Path.Combine(rootDirectory, "extracted");

                // Validate resource.json file
                if (!File.Exists(resourcejsonFilePath))
                {
                    Console.WriteLine("Error: resource.json file does not exist");
                    return;
                }

                // Create target directory if it doesn't exist
                if (!Directory.Exists(targetDirectoryPath))
                {
                    Directory.CreateDirectory(targetDirectoryPath);
                }
                else
                {
                    Console.WriteLine("Directory already exists");
                }

                // Read resource.json and get resource path
                string jsonContent = File.ReadAllText(resourcejsonFilePath);
                var jsonObject = JObject.Parse(jsonContent);
                string? resourcePath = jsonObject["patch"]?.Value<string>("resource_path");

                if (string.IsNullOrEmpty(resourcePath))
                {
                    Console.WriteLine("Error: resource_path is missing or empty in resource.json");
                    return;
                }

                if (resourcePath.LastIndexOf("/") == -1)
                {
                    Console.WriteLine("Error: Invalid resource_path format");
                    return;
                }

                string baseUrl = resourcePath.Substring(0, resourcePath.LastIndexOf("/") + 1);
                string excelZipUrl = $"{baseUrl}Preload/TableBundles/Excel.zip";

                // Download Excel.zip using RestSharp
                var client = new RestClient(excelZipUrl);
                var request = new RestRequest(Method.GET);
                IRestResponse response = client.Execute(request);

                if (response.IsSuccessful && response.RawBytes != null && response.RawBytes.Length > 0)
                {
                    byte[] fileBytes = response.RawBytes;
                    File.WriteAllBytes(ExeclzipPath, fileBytes);
                    Console.WriteLine($"Excel.zip downloaded successfully, size: {fileBytes.Length} bytes");
                }
                else
                {
                    Console.WriteLine("Failed to download Excel.zip or received empty content.");
                    return;
                }

                // Unzip the Excel.zip using 7-Zip with a password
                if (File.Exists(ExeclzipPath))
                {
                    try
                    {
                        // Assuming the password is created by TableService.CreatePassword()
                        string password = Convert.ToBase64String(TableService.CreatePassword(Path.GetFileName(ExeclzipPath)));

                        if (string.IsNullOrEmpty(password))
                        {
                            Console.WriteLine("Error: Zip password is empty");
                            return;
                        }

                        Console.WriteLine($"Using ZIP Password: {password}");

                        // Using 7-Zip to extract the ZIP file with a password
                        System.Diagnostics.Process process = new System.Diagnostics.Process();
                        process.StartInfo.FileName = "7z"; // Assume 7-Zip is installed and available in the system PATH
                        process.StartInfo.Arguments = $"x \"{ExeclzipPath}\" -p\"{password}\" -o\"{targetDirectoryPath}\" -y"; // -p for password, -o for output directory, -y for overwrite
                        process.StartInfo.UseShellExecute = false;
                        process.StartInfo.RedirectStandardOutput = true;
                        process.StartInfo.RedirectStandardError = true;

                        process.Start();
                        string output = process.StandardOutput.ReadToEnd();
                        string error = process.StandardError.ReadToEnd();
                        process.WaitForExit();

                        if (process.ExitCode == 0)
                        {
                            Console.WriteLine("Excel files extracted successfully using 7-Zip.");
                        }
                        else
                        {
                            Console.WriteLine($"Failed to extract ZIP file using 7-Zip. Error: {error}");
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"An exception occurred while extracting using 7-Zip: {ex.Message}");
                    }
                }
                else
                {
                    Console.WriteLine("Error: Excel.zip file does not exist.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An exception occurred: {ex.Message}");
            }

            // Assuming the method pythonScipt.pythonSciptMain(args) is necessary
            pythonScipt.pythonSciptMain(args);
        }
    }
}
