using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text.RegularExpressions;

namespace mxdat
{
    public class RaidGetBestTeamjson
    {
        public static void RaidGetBestTeamjsonMain(string[] args)
        {
            // Step 1: Check and create RaidGetBestTeam directory if not exists
            string jsonFolderPath = Path.Combine(Directory.GetCurrentDirectory(), "RaidGetBestTeam");
            if (!Directory.Exists(jsonFolderPath))
            {
                Directory.CreateDirectory(jsonFolderPath);
                Console.WriteLine("RaidGetBestTeam folder created");
            }
            else
            {
                Console.WriteLine("RaidGetBestTeam folder already exists");
            }

            // Step 2: Process all JSON files in RaidGetBestTeam directory
            string[] jsonFiles = Directory.GetFiles(jsonFolderPath, "*.json")
                                          .OrderBy(GetFileNumber)
                                          .ToArray();

            // Initialize a JObject to hold the combined nested JSON data
            JObject combinedData = new JObject();

            foreach (string file in jsonFiles)
            {
                try
                {
                    string jsonContent = File.ReadAllText(file);
                    JObject jsonData = JObject.Parse(jsonContent);

                    if (jsonData.TryGetValue("packet", out JToken nestedJsonToken))
                    {
                        string nestedJsonStr = nestedJsonToken.ToString();
                        JObject nestedData = JObject.Parse(nestedJsonStr);
                        jsonData["packet"] = nestedData;
                        File.WriteAllText(file, jsonData.ToString(Formatting.Indented));
                        Console.WriteLine($"Content of file {Path.GetFileName(file)} has been updated.");
                    }
                    else
                    {
                        Console.WriteLine($"File {Path.GetFileName(file)} is missing the \"packet\" field.");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error reading or parsing {Path.GetFileName(file)}: {ex.Message}");
                }
            }
        }

        private static int GetFileNumber(string filePath)
        {
            string fileName = Path.GetFileNameWithoutExtension(filePath);
            Match match = Regex.Match(fileName, @"\d+");
            if (match.Success)
            {
                return int.Parse(match.Value);
            }
            else
            {
                throw new FormatException($"File name '{fileName}' does not contain a valid number.");
            }
        }
    }
}
