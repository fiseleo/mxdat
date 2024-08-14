using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;


namespace mxdat
{
    class Updatalist
    {
        static readonly string rootDirectory = AppDomain.CurrentDomain.BaseDirectory;
        static readonly string jsonDirectory = Path.Combine(rootDirectory, "Updata"); // Relative path: Updata folder
        static readonly List<string> sourceDirectories = new List<string> { Path.Combine(rootDirectory, "mxdatpy", "extracted", "Excel") }; // All source directories
        static readonly string serverUrl = "http://35.247.55.157:9876/";
        static readonly string token = "]4]88Nft9*wn";
        static readonly List<string> fileNames = new List<string>
        {
            "EliminateRaidSeasonManageExcelTable.json",
            "RaidSeasonManageExcelTable.json"
        };

        public static void UpdatalistMain(string[] args)
        {
            EnsureDirectoryExists(jsonDirectory);

            // Run the job immediately
            Job(args);
        }

        static void EnsureDirectoryExists(string directory)
        {
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
                Console.WriteLine($"Created directory: {directory}");
            }
        }

        static void CopyAndRenameFilesToUpdata()
        {
            EnsureDirectoryExists(jsonDirectory);

            foreach (var sourceDirectory in sourceDirectories)
            {
                foreach (var fileName in fileNames)
                {
                    string sourceFilePath = Path.Combine(sourceDirectory, fileName);
                    if (File.Exists(sourceFilePath))
                    {
                        string destFilePath = Path.Combine(jsonDirectory, fileName);
                        File.Copy(sourceFilePath, destFilePath, true);
                        Console.WriteLine($"Copied and renamed {sourceFilePath} to {destFilePath}");
                    }
                }
            }
        }

        static void UploadFiles()
        {
            // Get all JSON files in the directory
            var jsonFiles = Directory.GetFiles(jsonDirectory, "*.json");

            foreach (var filePath in jsonFiles)
            {
                if (!File.Exists(filePath))
                {
                    Console.WriteLine($"File {filePath} does not exist, skipping.");
                    continue;
                }

                // Read the JSON file
                string jsonData = File.ReadAllText(filePath, Encoding.UTF8);
                JToken jsonObject;

                try
                {
                    jsonObject = JToken.Parse(jsonData);
                }
                catch (JsonReaderException)
                {
                    Console.WriteLine($"File {filePath} is not a valid JSON, skipping.");
                    continue;
                }

                // Check JSON structure and add protocol field
                if (jsonObject.Type == JTokenType.Object)
                {
                    ((JObject)jsonObject)["protocol"] = $"JP_{Path.GetFileName(filePath)}";

                }
                else if (jsonObject.Type == JTokenType.Array)
                {
                    jsonObject = new JObject
                    {
                        ["Data"] = jsonObject,
                        ["protocol"] = $"JP_{Path.GetFileName(filePath)}"
                    };
                }

                // Save modified JSON file
                File.WriteAllText(filePath, jsonObject.ToString(), Encoding.UTF8);

                // Send POST request to the server using RestSharp
                var client = new RestClient(serverUrl);
                var request = new RestRequest(Method.POST);
                request.AddHeader("Content-Type", "application/json");
                request.AddHeader("Token", token);
                request.AddHeader("User-Agent", "PostmanRuntime/7.39.0");
                request.AddHeader("Connection", "keep-alive");
                request.AddHeader("Accept-Encoding", "gzip, deflate, br");
                request.AddJsonBody(jsonObject.ToString());

                try
                {
                    IRestResponse response = client.Execute(request);
                    Console.WriteLine($"Sent {Path.GetFileName(filePath)} to server, response status code: {response.StatusCode}");
                    Console.WriteLine($"Response content: {response.Content}");
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Failed to send {Path.GetFileName(filePath)} to server: {e.Message}");
                }
            }
        }

        static void Job(string[] args)
        {
            CopyAndRenameFilesToUpdata();
            UploadFiles();
            Getlist.GetlistMain(args);
        }
    }
}
