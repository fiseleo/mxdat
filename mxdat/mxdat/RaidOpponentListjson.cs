using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Data;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;

namespace mxdat
{
    public class RaidOpponentListjson
    {
        public static void RaidOpponentListjsonMain(string[] args)
        {
            // 检查当前时间并根据需要暂停程序
            CheckAndPauseAt3AM();

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

            string nestedDataFileName = "RaidOpponentList.json";
            string nestedDataPath = Path.Combine(jsonFolderPath, nestedDataFileName);
            combinedData["timestamp"] = DateTime.UtcNow.ToString("o");
            File.WriteAllText(nestedDataPath, combinedData.ToString(Formatting.Indented));
            Console.WriteLine($"Successfully merged all JSON file data and wrote to {nestedDataFileName}");

            ProcessRaidOpponentListData(nestedDataPath);

            // 完成后返回RaidOpponentListMain方法
            RaidOpponentList.shouldContinue = true;
            RaidOpponentList.RaidOpponentListMain(args, DateTime.MinValue, DateTime.MinValue); // 实际seasonEndData和settlementEndDate应在调用时传递
        }

        private static long GetFileNumber(string filePath)
        {
            string fileName = Path.GetFileNameWithoutExtension(filePath);
            Match match = Regex.Match(fileName, @"\d+");
            if (match.Success)
            {
                return long.Parse(match.Value); // 从int改为long
            }
            else
            {
                throw new FormatException($"File name '{fileName}' does not contain valid numbers.");
            }
        }

        private static void ProcessRaidOpponentListData(string nestedDataPath)
        {
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
                    try
                    {
                        long accountId = opponent.Value<long>("AccountId"); // 确保AccountId为long类型
                        string nickname = opponent.Value<string>("Nickname");
                        int rank = opponent.Value<int>("Rank"); // 确保rank在Int32范围内
                        JObject resultObject = new JObject();
                        resultObject["AccountId"] = accountId;
                        resultObject["Nickname"] = nickname;
                        resultObject["Rank"] = rank;
                        resultArray.Add(resultObject);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error processing opponent data: {ex.Message}");
                    }
                }
                string resultFileName = "RaidOpponentListUserID&Nickname.json";
                string resultFilePath = Path.Combine(Path.GetDirectoryName(nestedDataPath), resultFileName);
                File.WriteAllText(resultFilePath, resultArray.ToString(Formatting.Indented));
                Console.WriteLine($"Successfully wrote AccountId and Nickname to file: {resultFileName}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error occurred during processing: {ex.Message}");
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
