using System;
using System.IO;
using System.Text.Json;
using RestSharp;

namespace mxdat
{
    public class GetNexonServerjson
    {
        public class RequestBody
        {
            public string market_game_id { get; set; }
            public string language { get; set; }
            public string advertising_id { get; set; }
            public string market_code { get; set; }
            public string sdk_version { get; set; }
            public string country { get; set; }
            public string curr_build_version { get; set; }
            public string curr_build_number { get; set; }
            public string curr_patch_version { get; set; }
        }

        public static string GetNexonServerjsonMain(string[] args)
        {
            var client = new RestClient("https://api-pub.nexon.com/patch/v1.1/version-check");
            var request = new RestRequest();
            request.Method = Method.POST;

            if (File.Exists("resource.json"))
            {
                File.Delete("resource.json");
            }
            if (File.Exists("Excel.zip"))
            {
                File.Delete("Excel.zip");
            }

            request.AddHeader("Connection", "keep-alive");
            request.AddHeader("User-Agent", "Dalvik/2.1.0 (Linux; U; Android 12; SM-A226B Build/V417IR)");
            request.AddHeader("Host", "api-pub.nexon.com");
            request.AddHeader("Accept-Encoding", "gzip");
            request.AddHeader("Content-Type", "application/json; charset=utf-8");

            string rootDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string filePath = Path.Combine(rootDirectory, "body.json");
            var jsonContent = File.ReadAllText(filePath);
            var body = JsonSerializer.Deserialize<RequestBody>(jsonContent);
            request.AddJsonBody(body);

            var response = client.Execute(request);
            Console.WriteLine(response.Content);
            string resourcejsonPath = Path.Combine(rootDirectory ,"resource.json");

            File.WriteAllText(resourcejsonPath, response.Content);

            GetExcelzip.GetExcelzipMain(args);

            return "";
        }
    }
}
