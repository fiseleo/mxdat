using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text.RegularExpressions;

namespace mxdat
{
    public class EliminateRaidOpponentListjson
    {
        public static void EliminateRaidOpponentListjsonMain(string[] args)
        {

            // Step 1: Check and create RaidOpponentList directory if not exists
            string jsonFolderPath = Path.Combine(Directory.GetCurrentDirectory(), "EliminateRaidOpponentList");
            if (!Directory.Exists(jsonFolderPath))
            {
                Directory.CreateDirectory(jsonFolderPath);
                Console.WriteLine("EliminateRaidOpponentList folder created");
            }
            else
            {
                Console.WriteLine("EliminateRaidOpponentList folder already exists");
            }

            // Step 2: Process all JSON files in RaidOpponentList directory
            string[] jsonFiles = Directory.GetFiles(jsonFolderPath, "*.json")
                                          .Where(file => !Path.GetFileName(file).Equals("EliminateRaidOpponentList.json", StringComparison.OrdinalIgnoreCase)
                                                      && !Path.GetFileName(file).Equals("EliminateRaidOpponentListUserID&Nickname.json", StringComparison.OrdinalIgnoreCase))
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

                    // Assuming each JSON file has a "packet" field containing nested JSON
                    string nestedJsonStr = jsonData["packet"].ToString();
                    JObject nestedData = JObject.Parse(nestedJsonStr);

                    // Combine nestedData into combinedData
                    combinedData.Merge(nestedData, new JsonMergeSettings
                    {
                        MergeArrayHandling = MergeArrayHandling.Union
                    });

                    Console.WriteLine($"The content of file {Path.GetFileName(file)} has been added to combinedData.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error reading or parsing {Path.GetFileName(file)}: {ex.Message}");
                }
            }

            // Step 3: Write combinedData to nested_data.json with indented format
            string dateTimeFormat = DateTime.Now.ToString("yyyyMMddHHmmss");
            string nestedDataFileName = $"EliminateRaidOpponentList{dateTimeFormat}.json";
            string nestedDataPath = Path.Combine(jsonFolderPath, nestedDataFileName);
            combinedData["timestamp"] = DateTime.UtcNow.ToString("o");
            File.WriteAllText(nestedDataPath, combinedData.ToString(Formatting.Indented));

            Console.WriteLine($"Successfully combined all JSON file data and wrote to {nestedDataFileName}.json");
            ExtractAccountIdAndNickname(jsonFolderPath, dateTimeFormat);
            EliminateRaidOpponentList.shouldContinue = true;
            EliminateRaidOpponentList.EliminateRaidOpponentListMain(args, DateTime.MinValue, DateTime.MinValue);
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
                throw new FormatException($"The file name '{fileName}' does not contain a valid number.");
            }
        }

        private static void ExtractAccountIdAndNickname(string jsonFolderPath, string dateTimeFormat)
        {
            try
            {
                string nestedDataFileName = $"EliminateRaidOpponentList{dateTimeFormat}.json";
                string nestedDataPath = Path.Combine(jsonFolderPath, nestedDataFileName);
                string jsonContent = File.ReadAllText(nestedDataPath);
                JObject nestedData = JObject.Parse(jsonContent);
                JArray opponents = (JArray)nestedData["OpponentUserDBs"];
                JArray resultArray = new JArray();
                foreach (JObject opponent in opponents)
                {
                    long accountId = opponent.Value<long>("AccountId");
                    string nickname = opponent.Value<string>("Nickname");
                    int rank = opponent.Value<int>("Rank");
                    JObject resultObject = new JObject();
                    resultObject["AccountId"] = accountId;
                    resultObject["Nickname"] = nickname;
                    resultObject["Rank"] = rank;
                    resultArray.Add(resultObject);
                }
                string ExtractAccountIdAndNicknamePath = Path.Combine(jsonFolderPath, "EliminateRaidOpponentListUserID&Nickname.json");
                File.WriteAllText(ExtractAccountIdAndNicknamePath, resultArray.ToString());
                Console.WriteLine($"Successfully wrote AccountId and Nickname to file: {ExtractAccountIdAndNicknamePath}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during processing: {ex.Message}");
            }
        }
    }    
}
