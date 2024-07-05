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
    public class RaidOpponentListjson
    {
        private static string dbPath = "RaidOpponentList.db";

        public static void RaidOpponentListjsonMain(string[] args)
        {
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

            CreateDatabaseAndTable();

            string[] jsonFiles = Directory.GetFiles(jsonFolderPath, "*.json")
                                          .Where(file => !Path.GetFileName(file).Equals("RaidOpponentList.json", StringComparison.OrdinalIgnoreCase)
                                                      && !Path.GetFileName(file).Equals("RaidOpponentListUserID&Nickname.json", StringComparison.OrdinalIgnoreCase))
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

            ProcessRaidOpponentListData();

            if (RaidOpponentList.isfinishloop)
            {
                RaidOpponentList.shouldContinue = false;
                RaidOpponentList.RaidOpponentListMain(args, DateTime.MinValue, DateTime.MinValue);
            }
            else
            {
                RaidOpponentList.shouldContinue = true;
                RaidOpponentList.RaidOpponentListMain(args, DateTime.MinValue, DateTime.MinValue);
            }
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
                throw new FormatException($"File name '{fileName}' does not contain valid numbers.");
            }
        }

        private static void CreateDatabaseAndTable()
        {
            using (SQLiteConnection conn = new SQLiteConnection($"Data Source={dbPath};Version=3;"))
            {
                conn.Open();

                string createTableQuery = @"CREATE TABLE IF NOT EXISTS RaidOpponentList (
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

                string insertQuery = "INSERT INTO RaidOpponentList (JsonData) VALUES (@jsonData)";
                using (SQLiteCommand cmd = new SQLiteCommand(insertQuery, conn))
                {
                    cmd.Parameters.AddWithValue("@jsonData", jsonData);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        private static void ProcessRaidOpponentListData()
        {
            using (SQLiteConnection conn = new SQLiteConnection($"Data Source={dbPath};Version=3;"))
            {
                conn.Open();

                string selectQuery = "SELECT JsonData FROM RaidOpponentList";
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

                    string nestedDataFileName = "RaidOpponentList.json";
                    string nestedDataPath = Path.Combine(Directory.GetCurrentDirectory(), "RaidOpponentList", nestedDataFileName);
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
                        long accountId = opponent.Value<long>("AccountId");
                        string nickname = opponent.Value<string>("Nickname");
                        int rank = opponent.Value<int>("Rank");
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
                today3AM = today3AM.AddDays(1);
            }

            TimeSpan timeTo3AM = today3AM - now;
            if (timeTo3AM.TotalMinutes <= 15)
            {
                Console.WriteLine("Approaching 3 AM, pausing the program for 60 minutes...");
                Thread.Sleep(TimeSpan.FromMinutes(60));
                ExecuteDecryptmxdat();
            }
        }

        private static void ExecuteDecryptmxdat()
        {
            Console.WriteLine("Running Decryptmxdat...");
            string[] emptyArgs = new string[0];
            Decryptmxdat.DecryptMain(emptyArgs);
        }
    }
}
