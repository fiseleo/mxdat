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
            CheckAndPauseAt3AM();
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

            string[] jsonFiles = Directory.GetFiles(jsonFolderPath, "*.json")
                                          .Where(file => !Path.GetFileName(file).Equals("JP_EliminateRaidOpponentList.json", StringComparison.OrdinalIgnoreCase)
                                                      && !Path.GetFileName(file).Equals("JP_EliminateRaidOpponentListUserID&Nickname.json", StringComparison.OrdinalIgnoreCase))
                                          .OrderBy(GetFileNumber)
                                          .ToArray();

            JArray combinedOpponents = new JArray();

            foreach (string file in jsonFiles)
            {
                if (Path.GetFileName(file).Equals("JP_EliminateRaidOpponentList20056.json", StringComparison.OrdinalIgnoreCase))
                {
                    Console.WriteLine("Reached JP_EliminateRaidOpponentList20056.json, stopping further processing.");
                    break;
                }

                try
                {
                    string jsonContent = File.ReadAllText(file);
                    JObject jsonObject = JObject.Parse(jsonContent);

                    // 解析 "packet" 欄位中的嵌套 JSON
                    if (jsonObject.TryGetValue("packet", out JToken packetToken))
                    {
                        JObject packetObject = JObject.Parse(packetToken.ToString());
                        JArray opponents = (JArray)packetObject["OpponentUserDBs"];
                        combinedOpponents.Merge(opponents);
                    }

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
                string rootDirectory = AppDomain.CurrentDomain.BaseDirectory;
                string resultFileName = "JP_EliminateRaidOpponentListUserID&Nickname.json";
                string resultFilePath = Path.Combine(rootDirectory, "EliminateRaidOpponentList", resultFileName);
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
