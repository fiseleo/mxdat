using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
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
            CheckAndPauseAt3AM();
            string rootDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string jsonFolderPath = Path.Combine(rootDirectory, "RaidOpponentList");

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
                                          .Where(file => !Path.GetFileName(file).Equals("JP_RaidOpponentList.json", StringComparison.OrdinalIgnoreCase)
                                                      && !Path.GetFileName(file).Equals("JP_RaidOpponentListUserID&Nickname.json", StringComparison.OrdinalIgnoreCase))
                                          .OrderBy(GetFileNumber)
                                          .ToArray();

            JArray combinedOpponents = new JArray();

            foreach (string file in jsonFiles)
            {
                if (Path.GetFileName(file).Equals("JP_RaidOpponentList20026.json", StringComparison.OrdinalIgnoreCase))
                {
                    Console.WriteLine("Reached RaidOpponentList10036.json, stopping further processing.");
                    break;
                }

                try
                {
                    string jsonContent = File.ReadAllText(file);
                    JObject jsonObject = JObject.Parse(jsonContent);
                    // Parse the nested JSON string in the "packet" key
                    JObject packetObject = JObject.Parse(jsonObject["packet"].ToString());
                    JArray opponents = (JArray)packetObject["OpponentUserDBs"];

                    combinedOpponents.Merge(opponents);
                    Console.WriteLine($"Processed contents of {Path.GetFileName(file)}.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error reading or processing {Path.GetFileName(file)}: {ex.Message}");
                }
            }

            ProcessAndOutputData(combinedOpponents);

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

        private static void ProcessAndOutputData(JArray combinedOpponents)
        {
            try
            {
                JArray resultArray = new JArray();
                foreach (JObject opponent in combinedOpponents)
                {
                    try
                    {
                        long accountId = opponent.Value<long>("AccountId");
                        string nickname = opponent.Value<string>("Nickname");
                        int rank = opponent.Value<int>("Rank");
                        JObject resultObject = new JObject
                        {
                            ["AccountId"] = accountId,
                            ["Nickname"] = nickname,
                            ["Rank"] = rank
                        };
                        resultArray.Add(resultObject);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error processing opponent data: {ex.Message}");
                    }
                }

                string resultFileName = "JP_RaidOpponentListUserID&Nickname.json";
                string resultFilePath = Path.Combine(Directory.GetCurrentDirectory(), "RaidOpponentList", resultFileName);
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