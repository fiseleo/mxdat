using System;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace mxdat
{
    class Updatalist
    {
        static readonly string rootDirectory = AppDomain.CurrentDomain.BaseDirectory;
        static readonly string jsonDirectory = Path.Combine(rootDirectory, "Updata"); // Relative path: Updata folder
        static readonly List<string> sourceDirectories = new List<string> { Path.Combine(rootDirectory, "extracted_excels") }; // All source directories
        static readonly string serverUrl = "http://34.145.96.130:9876/";
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

        static void ProcessAndCopyFilesToUpdata()
        {
            EnsureDirectoryExists(jsonDirectory);

            Parallel.ForEach(sourceDirectories, sourceDirectory =>
            {
                foreach (var fileName in fileNames)
                {
                    string sourceFilePath = Path.Combine(sourceDirectory, fileName);
                    if (File.Exists(sourceFilePath))
                    {
                        try
                        {
                            using (var fileStream = File.OpenRead(sourceFilePath))
                            using (var streamReader = new StreamReader(fileStream, Encoding.UTF8))
                            using (var jsonReader = new JsonTextReader(streamReader))
                            {
                                var jsonObject = JToken.ReadFrom(jsonReader);

                                // If the JSON contains a DataList property, extract its contents
                                if (jsonObject.Type == JTokenType.Object && jsonObject["DataList"] != null)
                                {
                                    jsonObject = jsonObject["DataList"];
                                }

                                // Remove existing "protocol" fields if any
                                if (jsonObject.Type == JTokenType.Object && ((JObject)jsonObject).ContainsKey("protocol"))
                                {
                                    ((JObject)jsonObject).Remove("protocol");
                                }

                                // Add protocol field to the JSON object at the end
                                var jsonWithProtocol = new JObject
                                {
                                    ["Data"] = jsonObject,
                                    ["protocol"] = fileName
                                };

                                // Save final JSON to the destination directory
                                string destFilePath = Path.Combine(jsonDirectory, fileName);
                                File.WriteAllText(destFilePath, jsonWithProtocol.ToString(Formatting.Indented), Encoding.UTF8);
                                Console.WriteLine($"Processed and copied {sourceFilePath} to {destFilePath}");
                            }
                        }
                        catch (JsonReaderException)
                        {
                            Console.WriteLine($"File {sourceFilePath} is not a valid JSON, skipping.");
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error processing {sourceFilePath}: {ex.Message}");
                        }
                    }
                }
            });
        }

        static void UploadFiles()
        {
            var jsonFiles = Directory.GetFiles(jsonDirectory, "*.json");

            Parallel.ForEach(jsonFiles, filePath =>
            {
                if (!File.Exists(filePath))
                {
                    Console.WriteLine($"File {filePath} does not exist, skipping.");
                    return;
                }

                try
                {
                    using (var fileStream = File.OpenRead(filePath))
                    using (var streamReader = new StreamReader(fileStream, Encoding.UTF8))
                    using (var jsonReader = new JsonTextReader(streamReader))
                    {
                        var jsonObject = JToken.ReadFrom(jsonReader);

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
                catch (JsonReaderException)
                {
                    Console.WriteLine($"File {filePath} is not a valid JSON, skipping.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error processing {filePath}: {ex.Message}");
                }
            });
        }

        static void Job(string[] args)
        {
            ProcessAndCopyFilesToUpdata();
            UploadFiles();
            Getlist.GetlistMain(args);
        }
    }
}
