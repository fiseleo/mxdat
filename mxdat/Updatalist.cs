using System;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using System.Collections.Generic;

namespace mxdat
{
    class Updatalist
    {
        static readonly string rootDirectory = AppDomain.CurrentDomain.BaseDirectory;
        static readonly string jsonDirectory = Path.Combine(rootDirectory, "Updata"); // Relative path: Updata folder
        static readonly List<string> sourceDirectories = new List<string> { Path.Combine(rootDirectory, "extracted_excels") }; // All source directories
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

        static void ProcessFilesInSourceDirectories()
        {
            foreach (var sourceDirectory in sourceDirectories)
            {
                foreach (var fileName in fileNames)
                {
                    string sourceFilePath = Path.Combine(sourceDirectory, fileName);
                    if (File.Exists(sourceFilePath))
                    {
                        // Read the JSON file
                        string jsonData = File.ReadAllText(sourceFilePath, Encoding.UTF8);
                        JToken jsonObject;

                        try
                        {
                            jsonObject = JToken.Parse(jsonData);
                        }
                        catch (JsonReaderException)
                        {
                            Console.WriteLine($"File {sourceFilePath} is not a valid JSON, skipping.");
                            continue;
                        }

                        // If the JSON contains a DataList property, extract its contents
                        if (jsonObject.Type == JTokenType.Object && jsonObject["DataList"] != null)
                        {
                            jsonObject = jsonObject["DataList"];
                        }

                        // Save processed JSON to source directory
                        File.WriteAllText(sourceFilePath, jsonObject.ToString(), Encoding.UTF8);
                        Console.WriteLine($"Processed {sourceFilePath} and updated original file");
                    }
                }
            }
        }

        static void ProcessAndCopyFilesToUpdata()
        {
            EnsureDirectoryExists(jsonDirectory);

            foreach (var sourceDirectory in sourceDirectories)
            {
                foreach (var fileName in fileNames)
                {
                    string sourceFilePath = Path.Combine(sourceDirectory, fileName);
                    if (File.Exists(sourceFilePath))
                    {
                        // Read the JSON file
                        string jsonData = File.ReadAllText(sourceFilePath, Encoding.UTF8);
                        JToken jsonObject;

                        try
                        {
                            jsonObject = JToken.Parse(jsonData);
                        }
                        catch (JsonReaderException)
                        {
                            Console.WriteLine($"File {sourceFilePath} is not a valid JSON, skipping.");
                            continue;
                        }

                        // If the JSON contains a DataList property, extract its contents
                        if (jsonObject.Type == JTokenType.Object && jsonObject["DataList"] != null)
                        {
                            jsonObject = jsonObject["DataList"];
                        }

                        // Save processed JSON to destination directory
                        string destFilePath = Path.Combine(jsonDirectory, fileName);
                        File.WriteAllText(destFilePath, jsonObject.ToString(), Encoding.UTF8);
                        Console.WriteLine($"Processed and copied {sourceFilePath} to {destFilePath}");
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

                // Add protocol field to each object in the array if JSON is an array
                if (jsonObject.Type == JTokenType.Array)
                {
                    foreach (var item in jsonObject)
                    {
                        if (item.Type == JTokenType.Object)
                        {
                            ((JObject)item)["protocol"] = Path.GetFileName(filePath);
                        }
                    }
                }
                else if (jsonObject.Type == JTokenType.Object)
                {
                    ((JObject)jsonObject)["protocol"] = Path.GetFileName(filePath);
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
            ProcessFilesInSourceDirectories();
            ProcessAndCopyFilesToUpdata();
            UploadFiles();
            Getlist.GetlistMain(args);
        }
    }
}