using mxdat.NetworkProtocol;
using RestSharp;
using System.Net;
using Newtonsoft.Json.Linq;

namespace mxdat
{
    public class RaidOpponentList
    {
        public static bool shouldContinue = false; // New flag variable
        public static DateTime nextStartTime; // New variable to store the next start time

        public static void RaidOpponentListMain(string[] args, DateTime seasonEndData, DateTime settlementEndDate)
        {
            if (shouldContinue)
            {
                shouldContinue = false;
                Console.WriteLine("Returning from RaidOpponentListjson, continuing to execute RaidOpponentList");
                Thread.Sleep(900000); // Pause for 15 minutes
                ExecuteMainLogic(args, seasonEndData, settlementEndDate);
            }
            else
            {
                ExecuteMainLogic(args, seasonEndData, settlementEndDate);
            }
        }

        private static void ExecuteMainLogic(string[] args, DateTime seasonEndData, DateTime settlementEndDate)
        {
            string mxdatjson = Path.Combine(Directory.GetCurrentDirectory(), "mxdat.json");
            string jsonFolderPath = Path.Combine(Directory.GetCurrentDirectory(), "RaidOpponentList");

            if (!Directory.Exists(jsonFolderPath))
            {
                Directory.CreateDirectory(jsonFolderPath);
                Console.WriteLine("json folder created");
            }
            else
            {
                Console.WriteLine("json folder already exists");
            }

            PacketCryptManager instance = new PacketCryptManager();
            int rankValue = 1;

            static string ExtractMxToken(string mxdatjson)
            {
                string jsonData = File.ReadAllText(mxdatjson);
                JObject jsonObject = JObject.Parse(jsonData);
                string mxToken = jsonObject["SessionKey"]["MxToken"].ToString();
                return mxToken;
            }

            string mxToken = ExtractMxToken(mxdatjson);

            long hash = 193286413221927;
            long accountServerId = 18152959;
            long accountId = 18152959;

            string baseJson = "{{\"Protocol\": 17016, " +
                              "\"Rank\": {0}, " +
                              "\"Score\": null, " +
                              "\"IsUpper\": false, " +
                              "\"IsFirstRequest\": true, " +
                              "\"SearchType\": 1, " +
                              "\"ClientUpTime\": 4, " +
                              "\"Resendable\": true, " +
                              "\"Hash\": {1}, " +
                              "\"IsTest\": false, " +
                              "\"SessionKey\":{{" +
                              "\"AccountServerId\": {3}, " +
                              "\"MxToken\": \"{2}\"}}, " +
                              "\"AccountId\": \"{4}\"}}";

            while (true)
            {
                DateTime now = DateTime.Now;
                if (now >= seasonEndData && now <= settlementEndDate)
                {
                    // Final loop
                    Console.WriteLine("This is the final loop");
                    string finalJson = string.Format(baseJson, rankValue, hash, mxToken, accountServerId, accountId);
                    byte[] finalMx = instance.RequestToBinary(Protocol.Raid_OpponentList, finalJson);
                    string finalFilePath = "mx.dat";
                    File.WriteAllBytes(finalFilePath, finalMx);

                    var finalClient = new RestClient("https://nxm-tw-bagl.nexon.com:5000/api/gateway");
                    finalClient.Timeout = -1;
                    var finalRequest = new RestRequest(Method.POST);
                    finalRequest.AddHeader("mx", "1");
                    finalRequest.AddFile("mx", finalFilePath);

                    IRestResponse finalResponse = null;
                    try
                    {
                        finalResponse = finalClient.Execute(finalRequest);
                        if (finalResponse.StatusCode != HttpStatusCode.OK || string.IsNullOrWhiteSpace(finalResponse.Content))
                        {
                            Console.WriteLine("Response is empty or request failed, retrying...");
                            Thread.Sleep(2000); // Wait 2 seconds before retrying
                            continue;
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Request failed: {ex.Message}, retrying...");
                        Thread.Sleep(2000); // Wait 2 seconds before retrying
                        continue;
                    }

                    if (rankValue == 50026)
                    {
                        Console.WriteLine(finalResponse.Content);
                        Console.WriteLine("No player information detected");
                        shouldContinue = true; // Set flag variable
                        RaidOpponentListjson.RaidOpponentListjsonMain(args);
                        return; // Stop the current method execution
                    }

                    string finalResponseFilePath = Path.Combine(jsonFolderPath, $"RaidOpponentList{rankValue}.json");
                    File.WriteAllText(finalResponseFilePath, finalResponse.Content);

                    // Execute EliminateRaidOpponentListjson logic
                    RaidOpponentListjson.RaidOpponentListjsonMain(args);

                    // Execute EliminateRaidGetBestTeam logic
                    RaidGetBestTeam.RaidGetBestTeamMain(args);

                    // Pause until 3:00 AM the next day
                    TimeSpan timeToWait = CalculateTimeToWait();
                    Console.WriteLine($"Pausing for {timeToWait.TotalMinutes} minutes");
                    Thread.Sleep((int)timeToWait.TotalMilliseconds);

                    // Continue loop
                    rankValue = 1;
                    hash++;
                    nextStartTime = DateTime.Now.AddMinutes(15); // Set the next start time
                    Console.WriteLine($"Next start time: {nextStartTime}");

                    // Execute Decryptmxdat logic
                    Thread.Sleep(900000); // Pause for 15 minutes
                    Decryptmxdat.DecryptMain(args);
                    continue;
                }

                // Normal loop logic
                string json = string.Format(baseJson, rankValue, hash, mxToken, accountServerId, accountId);
                Console.WriteLine(json);

                byte[] mx = instance.RequestToBinary(Protocol.Raid_OpponentList, json);
                string filePath = "mx.dat";
                File.WriteAllBytes(filePath, mx);

                var client = new RestClient("https://nxm-tw-bagl.nexon.com:5000/api/gateway");
                client.Timeout = -1;
                var request = new RestRequest(Method.POST);
                request.AddHeader("mx", "1");
                request.AddFile("mx", filePath);

                IRestResponse response = null;
                try
                {
                    response = client.Execute(request);
                    if (response.StatusCode != HttpStatusCode.OK || string.IsNullOrWhiteSpace(response.Content))
                    {
                        Console.WriteLine("Response is empty or request failed, retrying...");
                        Thread.Sleep(2000); // Wait 2 seconds before retrying
                        continue;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Request failed: {ex.Message}, retrying...");
                    Thread.Sleep(2000); // Wait 2 seconds before retrying
                    continue;
                }

                if (rankValue == 50026)
                {
                    Console.WriteLine(response.Content);
                    Console.WriteLine("No player information detected");
                    shouldContinue = true; // Set flag variable
                    RaidOpponentListjson.RaidOpponentListjsonMain(args);
                    return; // Stop the current method execution
                }

                string responseFilePath = Path.Combine(jsonFolderPath, $"RaidOpponentList{rankValue}.json");
                File.WriteAllText(responseFilePath, response.Content);

                rankValue = (rankValue == 1) ? rankValue + 15 : rankValue + 30;
                hash++;
                Thread.Sleep(1000); // Wait 1 second before the next iteration
            }
        }

        private static TimeSpan CalculateTimeToWait()
        {
            DateTime now = DateTime.Now;
            DateTime nextDay3AM = now.Date.AddDays(1).AddHours(3);
            return nextDay3AM - now;
        }
    }
}
