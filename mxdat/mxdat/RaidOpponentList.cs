using mxdat.NetworkProtocol;
using RestSharp;
using System.Net;
using Newtonsoft.Json.Linq;
using System.Text;

namespace mxdat
{
    public class RaidOpponentList
    {
        public static bool shouldContinue = false; // New flag variable
        public static int savedRankValue = 1; // Save rank value before pausing

        public static int rankValue = 1;

        public static bool isfinishloop = false;


        public static void RaidOpponentListMain(string[] args, DateTime seasonEndData, DateTime settlementEndDate)
        {
            Console.OutputEncoding = Encoding.UTF8;

            if (shouldContinue)
            {
                shouldContinue = false;
                Console.WriteLine($"Returning from RaidOpponentListjson, continuing to execute RaidOpponentList with rankValue 1");
                ExecuteMainLogic(args, seasonEndData, settlementEndDate, 1); // Resume with saved rank value
            }
            else if (isfinishloop)
            {
                isfinishloop = false;
                
                Console.WriteLine($"Returning from RaidOpponentListjson, continuing to execute RaidOpponentList with rankValue {savedRankValue}");
                ExecuteMainLogic(args, seasonEndData, settlementEndDate, savedRankValue);

            }
            else
            {
                ExecuteMainLogic(args, seasonEndData, settlementEndDate, 1);
            }
        }

        private static void ExecuteMainLogic(string[] args, DateTime seasonEndData, DateTime settlementEndDate, int rankValue)
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

            static string ExtractMxToken(string mxdatjson)
            {
                string jsonData = File.ReadAllText(mxdatjson);
                JObject jsonObject = JObject.Parse(jsonData);
                string mxToken = jsonObject["SessionKey"]["MxToken"].ToString();
                return mxToken;
            }


            static string ExtractAccountId(string mxdatjson)
            {
                string jsonData = File.ReadAllText(mxdatjson);
                JObject jsonObject = JObject.Parse(jsonData);
                string accountId = jsonObject["AccountId"].ToString();
                return accountId;
            }

            static string ExtractAccountServerId(string mxdatjson)
            {
                string jsonData = File.ReadAllText(mxdatjson);
                JObject jsonObject = JObject.Parse(jsonData);
                string accountServerId = jsonObject["SessionKey"]["AccountServerId"].ToString();
                return accountServerId;
            }



            string mxToken = ExtractMxToken(mxdatjson);

            long hash =  73083163508766;
            string accountServerId = ExtractAccountId(mxdatjson);
            string accountId = ExtractAccountServerId(mxdatjson);

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
                if (rankValue == 50056)
                {
                    Console.WriteLine($"Pausing execution at rankValue {rankValue} to run RaidOpponentListjson");
                    savedRankValue = rankValue - 1;
                    isfinishloop = true;
                    RaidOpponentListjson.RaidOpponentListjsonMain(args);
                    return; // Stop the current method execution
                }

                DateTime now = DateTime.Now;
                if (now.Date == seasonEndData.Date && now.Date == settlementEndDate.Date && now.TimeOfDay >= seasonEndData.TimeOfDay && now.TimeOfDay <= settlementEndDate.TimeOfDay)
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

                    if (!finalResponse.Content.Contains("OpponentUserDBs"))
                    {
                        Console.WriteLine(finalResponse.Content);
                        Console.WriteLine("No player information detected");
                        shouldContinue = true; // Set flag variable
                        isfinishloop = false;
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
                     // Forcing garbage collection
                    Thread.Sleep((int)timeToWait.TotalMilliseconds);

                    // Continue loop
                    rankValue = 1;
                    hash++;
                    Thread.Sleep(3600000);// Pause for 15 minutes
                    Decryptmxdat.DecryptMain(args);
                    continue;
                }

                // Normal loop logic
                string json = string.Format(baseJson, rankValue, hash, mxToken, accountServerId, accountId);
                Console.WriteLine($"査排名{rankValue}中...");

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

                if (!response.Content.Contains("OpponentUserDBs"))
                {
                    Console.WriteLine(response.Content);
                    Console.WriteLine("No player information detected");
                    shouldContinue = true; // Set flag variable
                    isfinishloop = false;
                    RaidOpponentListjson.RaidOpponentListjsonMain(args);
                    return; // Stop the current method execution
                }

                string responseFilePath = Path.Combine(jsonFolderPath, $"RaidOpponentList{rankValue}.json");
                File.WriteAllText(responseFilePath, response.Content);

                rankValue = (rankValue == 1) ? rankValue + 15 : rankValue + 30;
                hash++;
                Thread.Sleep(900); // Wait 900ms before the next iteration
                GC.Collect();
                GC.WaitForPendingFinalizers();
            }
        }

        private static TimeSpan CalculateTimeToWait()
        {
            DateTime now = DateTime.Now;
            DateTime today3AM = now.Date.AddHours(3);
            if (now < today3AM)
            {
                today3AM = today3AM.AddDays(1); // 如果現在時間已經超過當天的凌晨3點，則計算到下一天凌晨3點的時間
            }
            return today3AM - now;
        }
    }
}
