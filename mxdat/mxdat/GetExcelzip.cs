using System;
using System.IO;
using System.Net.Http;
using Newtonsoft.Json.Linq;

namespace mxdat
{
    public class GetExcelzip
    {
        public static async Task GetExcelzipMain(string[] args)
        {
            try
            {
                string jsonContent = File.ReadAllText("resource.json");
                var jsonObject = JObject.Parse(jsonContent);
                string resourcePath = jsonObject["patch"].Value<string>("resource_path");

                string baseUrl = resourcePath.Substring(0, resourcePath.LastIndexOf("/") + 1);
                string excelZipUrl = $"{baseUrl}Preload/TableBundles/Excel.zip";

                using (var httpClient = new HttpClient())
                {
                    var response = await httpClient.GetAsync(excelZipUrl);
                    if (response.IsSuccessStatusCode)
                    {
                        byte[] fileBytes = await response.Content.ReadAsByteArrayAsync();
                        File.WriteAllBytes("Excel.zip", fileBytes);
                        Console.WriteLine("Excel.zip 下載完成");
                    }
                    else
                    {
                        Console.WriteLine("無法下載 Excel.zip 檔案");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"發生異常: {ex.Message}");
            }

            pythonScipt.pythonSciptMain(args);
        }
    }
}