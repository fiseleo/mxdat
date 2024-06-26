using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text.RegularExpressions;

namespace mxdat
{
    public class RaidOpponentListjson
    {
        public static void RaidOpponentListjsonMain(string[] args)
        {
            string jsonFolderPath = Path.Combine(Directory.GetCurrentDirectory(), "RaidOpponentList");
            if (!Directory.Exists(jsonFolderPath))
            {
                Directory.CreateDirectory(jsonFolderPath);
                Console.WriteLine("RaidOpponentList folder created");
            }
            else
            {
                Console.WriteLine("RaidOpponentList folder already exists");
            }

            string[] jsonFiles = Directory.GetFiles(jsonFolderPath, "*.json")
                                          .Where(file => !Path.GetFileName(file).Equals("RaidOpponentList.json", StringComparison.OrdinalIgnoreCase)
                                                      && !Path.GetFileName(file).Equals("RaidOpponentListUserID&Nickname.json", StringComparison.OrdinalIgnoreCase))
                                          .OrderBy(GetFileNumber)
                                          .ToArray();

            JObject combinedData = new JObject();

            foreach (string file in jsonFiles)
            {
                try
                {
                    string jsonContent = File.ReadAllText(file);
                    JObject jsonData = JObject.Parse(jsonContent);

                    string nestedJsonStr = jsonData["packet"].ToString();
                    JObject nestedData = JObject.Parse(nestedJsonStr);

                    combinedData.Merge(nestedData, new JsonMergeSettings
                    {
                        MergeArrayHandling = MergeArrayHandling.Union
                    });

                    Console.WriteLine($"Added contents of {Path.GetFileName(file)} to combinedData.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error reading or parsing {Path.GetFileName(file)}: {ex.Message}");
                }
            }

            string dateTimeFormat = DateTime.Now.ToString("yyyyMMddHHmmss");
            string nestedDataFileName = $"RaidOpponentList{dateTimeFormat}.json";
            string nestedDataPath = Path.Combine(jsonFolderPath, nestedDataFileName);
            combinedData["timestamp"] = DateTime.UtcNow.ToString("o");
            File.WriteAllText(nestedDataPath, combinedData.ToString(Formatting.Indented));
            Console.WriteLine($"Successfully merged all JSON file data and wrote to {nestedDataFileName}");

            ProcessRaidOpponentListData();

            // After completion, return to RaidOpponentListMain method
            RaidOpponentList.shouldContinue = true;
            RaidOpponentList.RaidOpponentListMain(args, DateTime.MinValue, DateTime.MinValue); // Actual seasonEndData and settlementEndDate should be passed when calling
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
                throw new FormatException($"File name '{fileName}' does not contain valid numbers.");
            }
        }

        private static void ProcessRaidOpponentListData()
        {
            string jsonFolderPath = Path.Combine(Directory.GetCurrentDirectory(), "RaidOpponentList");
            string dateTimeFormat = DateTime.Now.ToString("yyyyMMddHHmmss");
            string nestedDataFileName = $"RaidOpponentList{dateTimeFormat}.json";
            string nestedDataPath = Path.Combine(jsonFolderPath, nestedDataFileName);

            try
            {
                string jsonContent = File.ReadAllText(nestedDataPath);
                JObject nestedData = JObject.Parse(jsonContent);
                JArray opponents = (JArray)nestedData["OpponentUserDBs"];
                foreach (JObject opponent in opponents)
                {
                    JObject RaidOpponentListDB = (JObject)opponent["RaidTeamSettingDB"];
                    RaidOpponentListDB.Remove("MainCharacterDBs");
                    RaidOpponentListDB.Remove("SupportCharacterDBs");
                    RaidOpponentListDB.Remove("SkillCardMulliganCharacterIds");
                    RaidOpponentListDB.Remove("LeaderCharacterUniqueId");
                }
                File.WriteAllText(nestedDataPath, nestedData.ToString(Formatting.Indented));
                Console.WriteLine($"Successfully removed specified JSON data sections from {nestedDataFileName}.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing RaidOpponentList.json: {ex.Message}");
            }

            ExtractAccountIdAndNickname(jsonFolderPath, dateTimeFormat);
        }

        private static void ExtractAccountIdAndNickname(string jsonFolderPath, string dateTimeFormat)
        {
            try
            {
                string nestedDataFileName = $"RaidOpponentList{dateTimeFormat}.json";
                string nestedDataPath = Path.Combine(jsonFolderPath, nestedDataFileName);
                string jsonContent = File.ReadAllText(nestedDataPath);
                JObject nestedData = JObject.Parse(jsonContent);
                JArray opponents = (JArray)nestedData["OpponentUserDBs"];
                JArray accountIdNicknameList = new JArray();
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
                string resultFileName = $"RaidOpponentListUserID&Nickname.json";
                string resultFilePath = Path.Combine(jsonFolderPath, resultFileName);
                File.WriteAllText(resultFilePath, resultArray.ToString());
                Console.WriteLine($"Successfully wrote AccountId and Nickname to file: {resultFileName}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error occurred during processing: {ex.Message}");
            }
        }
    }
}
