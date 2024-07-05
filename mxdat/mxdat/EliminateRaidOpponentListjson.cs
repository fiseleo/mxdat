using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;

namespace mxdat
{
    public class EliminateRaidOpponentListjson
    {
        private static string dbPath = "EliminateRaidOpponentList.db";

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

            CreateDatabaseAndTable();

            // Step 2: Process all JSON files in EliminateRaidOpponentList directory
            string[] jsonFiles = Directory.GetFiles(jsonFolderPath, "*.json")
                                          .Where(file => !Path.GetFileName(file).Equals("EliminateRaidOpponentList.json", StringComparison.OrdinalIgnoreCase)
                                                      && !Path.GetFileName(file).Equals("EliminateRaidOpponentListUserID&Nickname.json", StringComparison.OrdinalIgnoreCase))
                                          .OrderBy(GetFileNumber)
                                          .ToArray();

            foreach (string file in jsonFiles)
            {
                try
                {
                    string jsonContent = File.ReadAllText(file);
                    InsertJsonDataIntoDatabase(jsonContent);
                    Console.WriteLine($"Inserted contents of {Path.GetFileName(file)} into SQLite database.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error reading or inserting {Path.GetFileName(file)}: {ex.Message}");
                }
            }

            ProcessEliminateRaidOpponentListData();
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

        private static void CreateDatabaseAndTable()
        {
            using (SQLiteConnection conn = new SQLiteConnection($"Data Source={dbPath};Version=3;"))
            {
                conn.Open();

                string createTableQuery = @"CREATE TABLE IF NOT EXISTS EliminateRaidOpponentList (
                                                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                                                JsonData TEXT
                                            )";
                using (SQLiteCommand cmd = new SQLiteCommand(createTableQuery, conn))
                {
                    cmd.ExecuteNonQuery();
                }
            }
        }

        private static void InsertJsonDataIntoDatabase(string jsonData)
        {
            using (SQLiteConnection conn = new SQLiteConnection($"Data Source={dbPath};Version=3;"))
            {
                conn.Open();

                string insertQuery = "INSERT INTO EliminateRaidOpponentList (JsonData) VALUES (@jsonData)";
                using (SQLiteCommand cmd = new SQLiteCommand(insertQuery, conn))
                {
                    cmd.Parameters.AddWithValue("@jsonData", jsonData);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        private static void ProcessEliminateRaidOpponentListData()
        {
            using (SQLiteConnection conn = new SQLiteConnection($"Data Source={dbPath};Version=3;"))
            {
                conn.Open();

                string selectQuery = "SELECT JsonData FROM EliminateRaidOpponentList";
                using (SQLiteCommand cmd = new SQLiteCommand(selectQuery, conn))
                using (SQLiteDataReader reader = cmd.ExecuteReader())
                {
                    JObject combinedData = new JObject();

                    while (reader.Read())
                    {
                        string jsonData = reader.GetString(0);
                        JObject jsonObject = JObject.Parse(jsonData);
                        string nestedJsonStr = jsonObject["packet"].ToString();
                        JObject nestedData = JObject.Parse(nestedJsonStr);

                        combinedData.Merge(nestedData, new JsonMergeSettings
                        {
                            MergeArrayHandling = MergeArrayHandling.Union
                        });
                    }

                    string nestedDataFileName = "EliminateRaidOpponentList.json";
                    string nestedDataPath = Path.Combine(Directory.GetCurrentDirectory(), "EliminateRaidOpponentList", nestedDataFileName);
                    combinedData["timestamp"] = DateTime.UtcNow.ToString("o");
                    File.WriteAllText(nestedDataPath, combinedData.ToString(Formatting.Indented));
                    Console.WriteLine($"Successfully merged all JSON data and wrote to {nestedDataFileName}");

                    ProcessJsonData(nestedDataPath);
                }
            }
        }

        private static void ProcessJsonData(string nestedDataPath)
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
                Console.WriteLine("接近凌晨3点，暂停程序60分钟...");
                Thread.Sleep(TimeSpan.FromMinutes(60));
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
