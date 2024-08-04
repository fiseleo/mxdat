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
            // Check current time and pause the program if necessary
            CheckAndPauseAt3AM();

            // Step 1: Check and create EliminateRaidOpponentList directory if not exists
            string rootDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string jsonFolderPath = Path.Combine(rootDirectory, "EliminateRaidOpponentList");
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

            JArray combinedOpponents = new JArray();

            foreach (string file in jsonFiles)
            {
                if (Path.GetFileName(file).Equals("EliminateRaidOpponentList10006.json", StringComparison.OrdinalIgnoreCase))
                {
                    Console.WriteLine("Reached EliminateRaidOpponentList10006.json, stopping further processing.");
                    break;
                }

                try
                {
                    string jsonContent = File.ReadAllText(file);
                    JObject jsonObject = JObject.Parse(jsonContent);
                    JArray opponents = (JArray)jsonObject["OpponentUserDBs"];

                    combinedOpponents.Merge(opponents);
                    Console.WriteLine($"Processed contents of {Path.GetFileName(file)}.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error reading or processing {Path.GetFileName(file)}: {ex.Message}");
                }
            }

            ProcessAndOutputData(combinedOpponents);

            if (EliminateRaidOpponentList.isfinishloop)
            {
                EliminateRaidOpponentList.shouldContinue = false;
                EliminateRaidOpponentList.EliminateRaidOpponentListMain(args, DateTime.MinValue, DateTime.MinValue);
            }
            else
            {
                EliminateRaidOpponentList.shouldContinue = true;
                EliminateRaidOpponentList.EliminateRaidOpponentListMain(args, DateTime.MinValue, DateTime.MinValue);
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
                throw new FormatException($"The file name '{fileName}' does not contain a valid number.");
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

                string resultFileName = "EliminateRaidOpponentListUserID&Nickname.json";
                string resultFilePath = Path.Combine(Directory.GetCurrentDirectory(), "EliminateRaidOpponentList", resultFileName);
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
                today3AM = today3AM.AddDays(1); // Calculate time to next 3 AM
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
