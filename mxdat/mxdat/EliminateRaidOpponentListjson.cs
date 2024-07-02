using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;

namespace mxdat
{
    public class EliminateRaidOpponentListjson
    {
        public static void EliminateRaidOpponentListjsonMain(string[] args)
        {
            // 检查当前时间并根据需要暂停程序
            CheckAndPauseAt3AM();

            // Step 1: Check and create EliminateRaidOpponentList directory if not exists
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

            // Step 2: Process all JSON files in EliminateRaidOpponentList directory
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

            // Write the combined data to a new JSON file
            string nestedDataFileName = "EliminateRaidOpponentList.json";
            string nestedDataPath = Path.Combine(jsonFolderPath, nestedDataFileName);
            combinedData["timestamp"] = DateTime.UtcNow.ToString("o");
            File.WriteAllText(nestedDataPath, combinedData.ToString(Formatting.Indented));
            Console.WriteLine($"Successfully merged all JSON file data and wrote to {nestedDataFileName}");

            ProcessEliminateRaidOpponentListData(nestedDataPath);
        }

        private static long GetFileNumber(string filePath)
        {
            string fileName = Path.GetFileNameWithoutExtension(filePath);
            Match match = Regex.Match(fileName, @"\d+");
            if (match.Success)
            {
                return long.Parse(match.Value);
            }
            else
            {
                throw new FormatException($"The file name '{fileName}' does not contain a valid number.");
            }
        }

        private static void ProcessEliminateRaidOpponentListData(string nestedDataPath)
        {
            try
            {
                string jsonContent = File.ReadAllText(nestedDataPath);
                JObject nestedData = JObject.Parse(jsonContent);
                JArray opponents = (JArray)nestedData["OpponentUserDBs"];
                foreach (JObject opponent in opponents)
                {
                    JObject EliminateRaidOpponentListDB = (JObject)opponent["RaidTeamSettingDB"];
                    EliminateRaidOpponentListDB.Remove("MainCharacterDBs");
                    EliminateRaidOpponentListDB.Remove("SupportCharacterDBs");
                    EliminateRaidOpponentListDB.Remove("SkillCardMulliganCharacterIds");
                    EliminateRaidOpponentListDB.Remove("LeaderCharacterUniqueId");
                }
                File.WriteAllText(nestedDataPath, nestedData.ToString(Formatting.Indented));
                Console.WriteLine($"Successfully removed specified JSON data sections from {Path.GetFileName(nestedDataPath)}.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing {Path.GetFileName(nestedDataPath)}: {ex.Message}");
            }

            ExtractAccountIdAndNickname(nestedDataPath);
        }

        private static void ExtractAccountIdAndNickname(string nestedDataPath)
        {
            try
            {
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
                string resultFileName = "EliminateRaidOpponentListUserID&Nickname.json";
                string resultFilePath = Path.Combine(Path.GetDirectoryName(nestedDataPath), resultFileName);
                File.WriteAllText(resultFilePath, resultArray.ToString(Formatting.Indented));
                Console.WriteLine($"Successfully wrote AccountId and Nickname to file: {resultFileName}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during processing: {ex.Message}");
            }
        }

        private static void CheckAndPauseAt3AM()
        {
            DateTime now = DateTime.Now;
            DateTime today3AM = now.Date.AddHours(3);
            if (now > today3AM)
            {
                today3AM = today3AM.AddDays(1); // 计算到下一个凌晨3点的时间
            }

            TimeSpan timeTo3AM = today3AM - now;
            if (timeTo3AM.TotalMinutes <= 15)
            {
                Console.WriteLine("接近凌晨3点，暂停程序15分钟...");
                Thread.Sleep(TimeSpan.FromMinutes(15));
                ExecuteDecryptmxdat();
            }
        }

        private static void ExecuteDecryptmxdat()
        {
            Console.WriteLine("running Decryptmxdat...");
            string[] emptyArgs = new string[0];
            Decryptmxdat.DecryptMain(emptyArgs);
        }
    }
}
