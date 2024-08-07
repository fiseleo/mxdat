using System;
using RestSharp;
using System.IO; // 引入 System.IO 命名空間

namespace mxdat
{
    public class GetNexonServerjson
    {
        public static async Task<string> GetNexonServerjsonMain(string[] args)
        {
            string rootDirectory = AppDomain.CurrentDomain.BaseDirectory;
            var client = new RestClient("https://api-pub.nexon.com/patch/v1.1/version-check");
            var request = new RestRequest();
            request.Method = Method.POST;
            string ExeclzipPath = Path.Combine(rootDirectory, "Excel.zip");
            string oldresourcejsonFilePath = Path.Combine(rootDirectory, "resource.json");

            if (File.Exists(oldresourcejsonFilePath))
            {
                File.Delete(oldresourcejsonFilePath);
            }
            if (File.Exists(ExeclzipPath))
            {
                File.Delete(ExeclzipPath);
            }
            request.AddHeader("Connection", "keep-alive");
            request.AddHeader("User-Agent", "Dalvik/2.1.0 (Linux; U; Android 12; SM-A226B Build/V417IR)");
            request.AddHeader("Host", "api-pub.nexon.com");
            request.AddHeader("Accept-Encoding", "gzip");
            request.AddHeader("Content-Type", "application/json; charset=utf-8");

            var body = new
            {
                market_game_id = "com.nexon.bluearchive",
                language = "zh",
                advertising_id = "00000000-0000-0000-0000-000000000000",
                market_code = "playstore",
                sdk_version = "239",
                country = "US",
                curr_build_version = "1.66.291639",
                curr_build_number = "291639",
                curr_patch_version = "1120"
            };
            request.AddJsonBody(body);

            RestResponse response = (RestResponse)client.Execute(request);
            Console.WriteLine(response.Content);

            // 將 response 內容寫入到檔案中
            string resourcejsonFilePath = Path.Combine(rootDirectory, "resource.json");
            File.WriteAllText(resourcejsonFilePath, response.Content);
            

            

            // 呼叫 GetExcelzip.GetExcelzipMain 方法
            GetExcelzip.GetExcelzipMain(args);
            
            return "" ;
        }
    }
}