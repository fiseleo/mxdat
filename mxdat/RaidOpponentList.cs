using mxdat.NetworkProtocol; // Ensure this is present
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
            CheckAndPauseAt3AM();

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
            string rootDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string mxdatjson = Path.Combine(rootDirectory, "mxdat.json");
            string jsonFolderPath = Path.Combine(rootDirectory, "RaidOpponentList");

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
                if (rankValue == 20056)
                {
                    Console.WriteLine($"Pausing execution at rankValue {rankValue} to run RaidOpponentListjson");
                    savedRankValue = rankValue - 1;
                    isfinishloop = true;
                    RaidOpponentListjson.RaidOpponentListjsonMain(args);
                    return; // Stop the current method execution
                }

                // Normal loop logic
                string json = string.Format(baseJson, rankValue, hash, mxToken, accountServerId, accountId);
                Console.WriteLine($"査排名{rankValue}中...");
                // CS0117 move dump.cs

                byte[] mx = instance.RequestToBinary(Protocol.Raid_OpponentList, json);
                string filePath = "mx.dat";
                File.WriteAllBytes(filePath, mx);

                var client = new RestClient("https://prod-game.bluearchiveyostar.com:5000/api/gateway");
                client.Timeout = -1;
                var request = new RestRequest(Method.POST);
                request.AddHeader("mx", "1");
                request.AddHeader("bundle-version", "hqvu8nx1gz");
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
                if (rankValue == 20055)
                {
                    rankValue = 120000;
                }
                if (rankValue == 120060);
                {
                    shouldContinue = true; // Set flag variable
                    isfinishloop = false;
                    RaidGetBestTeam.RaidGetBestTeamMain(args);
                    
                }
                string responseFilePath = Path.Combine(jsonFolderPath, $"JP_RaidOpponentList{rankValue}.json");
                File.WriteAllText(responseFilePath, response.Content);
                // Upload the JSON content to the server
                UploadJsonToServer(responseFilePath);
                rankValue = (rankValue == 1) ? rankValue + 15 : rankValue + 30;
                hash++;
                Thread.Sleep(900); // Wait 2 seconds before the next iteration
                GC.Collect();
                GC.WaitForPendingFinalizers();
                CheckAndPauseAt3AM();
                return;
            }
        }

        private static void UploadJsonToServer(string filePath)
        {
            string serverUrl = "http://35.247.55.157:9876";
            string token = "]4]88Nft9*wn";

            try
            {
                string jsonData = File.ReadAllText(filePath);
                var client = new RestClient(serverUrl);
                var request = new RestRequest(Method.POST);
                request.AddParameter("application/json", jsonData, ParameterType.RequestBody);
                request.AddHeader("Token", token);
                request.AddHeader("Content-Type", "application/json");
                request.AddHeader("User-Agent", "PostmanRuntime/7.39.0");
                request.AddHeader("Connection", "keep-alive");
                request.AddHeader("Accept-Encoding", "gzip, deflate, br");

                IRestResponse response = client.Execute(request);
                Console.WriteLine($"Uploaded {filePath} to server, response status code: {response.StatusCode}");
                Console.WriteLine($"Response content: {response.Content}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to upload {filePath} to server: {ex.Message}");
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
