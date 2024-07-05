using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace mxdat
{
    public class RaidGetBestTeamjson
    {
        private static string dbPath = "RaidGetBestTeam.db";

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

            CreateDatabaseAndTable();

            // Step 2: Process all JSON files in RaidGetBestTeam directory
            string[] jsonFiles = Directory.GetFiles(jsonFolderPath, "*.json")
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

            ProcessRaidGetBestTeamData();

            Getlist.GetClosestSeason();
            GetNexonServerjson.GetNexonServerjsonMain(args);
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

        private static void CreateDatabaseAndTable()
        {
            using (SQLiteConnection conn = new SQLiteConnection($"Data Source={dbPath};Version=3;"))
            {
                conn.Open();

                string createTableQuery = @"CREATE TABLE IF NOT EXISTS RaidGetBestTeam (
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

                string insertQuery = "INSERT INTO RaidGetBestTeam (JsonData) VALUES (@jsonData)";
                using (SQLiteCommand cmd = new SQLiteCommand(insertQuery, conn))
                {
                    cmd.Parameters.AddWithValue("@jsonData", jsonData);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        private static void ProcessRaidGetBestTeamData()
        {
            using (SQLiteConnection conn = new SQLiteConnection($"Data Source={dbPath};Version=3;"))
            {
                conn.Open();

                string selectQuery = "SELECT JsonData FROM RaidGetBestTeam";
                using (SQLiteCommand cmd = new SQLiteCommand(selectQuery, conn))
                using (SQLiteDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string jsonData = reader.GetString(0);
                        JObject jsonObject = JObject.Parse(jsonData);

                        if (jsonObject.TryGetValue("packet", out JToken nestedJsonToken))
                        {
                            string nestedJsonStr = nestedJsonToken.ToString();
                            JObject nestedData = JObject.Parse(nestedJsonStr);
                            jsonObject["packet"] = nestedData;
                            jsonObject["timestamp"] = DateTime.UtcNow.ToString("o");

                            // Write the updated JSON back to the database
                            UpdateJsonDataInDatabase(jsonObject.ToString(Formatting.Indented));
                            Console.WriteLine($"Updated content in database for {jsonObject["Id"]}.");
                        }
                        else
                        {
                            Console.WriteLine("Record in database is missing the \"packet\" field.");
                        }
                    }
                }
            }
        }

        private static void UpdateJsonDataInDatabase(string jsonData)
        {
            using (SQLiteConnection conn = new SQLiteConnection($"Data Source={dbPath};Version=3;"))
            {
                conn.Open();

                string updateQuery = "UPDATE RaidGetBestTeam SET JsonData = @jsonData WHERE Id = (SELECT Id FROM RaidGetBestTeam ORDER BY Id DESC LIMIT 1)";
                using (SQLiteCommand cmd = new SQLiteCommand(updateQuery, conn))
                {
                    cmd.Parameters.AddWithValue("@jsonData", jsonData);
                    cmd.ExecuteNonQuery();
                }
            }
        }
    }
}
