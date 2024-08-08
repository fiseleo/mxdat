using mxdat.NetworkProtocol;
using RestSharp;
using System.Net;
using Newtonsoft.Json.Linq;
using System.IO;
using System;

namespace mxdat
{
    public class RaidGetBestTeam
    {
        public static void RaidGetBestTeamMain(string[] args)
        {
            string rootDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string mxdatjson = Path.Combine(rootDirectory, "mxdat.json");
            string jsonFolderPath = Path.Combine(rootDirectory, "RaidGetBestTeam");

            CheckAndPauseAt3AM();

            if (!Directory.Exists(jsonFolderPath))
            {
                Directory.CreateDirectory(jsonFolderPath);
                Console.WriteLine("RaidGetBestTeam folder created");
            }
            else
            {
                Console.WriteLine("RaidGetBestTeam folder already exists");
                foreach (string file in Directory.GetFiles(jsonFolderPath, "*.json"))
                {
                    File.Delete(file);
                }
                Console.WriteLine("Existing JSON files deleted from RaidGetBestTeam folder");
            }

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

            string mxtoken = ExtractMxToken(mxdatjson);

            PacketCryptManager Instance = new PacketCryptManager();

            long hash = 73100343377952;
            string accountServerId = ExtractAccountId(mxdatjson);
            string accountId = ExtractAccountServerId(mxdatjson);

            string baseJson = "{{\"Protocol\": 17020, " +
                "\"SearchAccountId\": {0}, " +
                "\"ClientUpTime\": 4, " +
                "\"Resendable\": true, " +
                "\"Hash\": {1}, " +      // input Hash
                "\"IsTest\": false, " +
                "\"SessionKey\":{{" +
                    "\"AccountServerId\": {3}, " +
                    "\"MxToken\": \"{2}\"}}, " +
                    "\"AccountId\": \"{4}\"}}";

            string RaidFolderPath = Path.Combine(rootDirectory, "RaidOpponentList");
            string ExtractAccountIdAndNicknamePath = Path.Combine(RaidFolderPath, "JP_RaidOpponentListUserID&Nickname.json");

            try
            {
                string jsonContent = File.ReadAllText(ExtractAccountIdAndNicknamePath);
                JArray opponents = JArray.Parse(jsonContent);
                foreach (JObject opponent in opponents)
                {
                    long SearchAccountId = opponent.Value<long>("AccountId");
                    int rank = opponent.Value<int>("Rank");

                    // If rank is 10000, stop processing
                    if (rank == 20001)
                    {
                        Console.WriteLine("Rank is 20000, stopping the process.");
                        break;
                    }

                    long adjustedHash = hash + rank; // Adjust hash with rank

                    string json = string.Format(baseJson, SearchAccountId, adjustedHash, mxtoken, accountServerId, accountId);
                    byte[] mx = Instance.RequestToBinary(Protocol.Raid_GetBestTeam, json);
                    string filePath = "mx.dat";
                    File.WriteAllBytes(filePath, mx);

                    var client = new RestClient("https://prod-game.bluearchiveyostar.com:5000/api/gateway");
                    client.Timeout = -1;
                    var request = new RestRequest(Method.POST);
                    request.AddHeader("mx", "1");
                    request.AddHeader("bundle-version", "hqvu8nx1gz");
                    request.AddFile("mx", "mx.dat");
                    IRestResponse response = null;

                    try
                    {
                        response = client.Execute(request);
                        if (response.StatusCode != HttpStatusCode.OK || string.IsNullOrWhiteSpace(response.Content))
                        {
                            Console.WriteLine("Empty response or request failed for AccountId: {0}, retrying...", SearchAccountId);
                            Thread.Sleep(2000); // Wait for 2 seconds before retrying
                            continue;
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Request failed for AccountId: {0}: {1}, retrying...", SearchAccountId, ex.Message);
                        Thread.Sleep(2000); // Wait for 2 seconds before retrying
                        continue;
                    }
                    string responseFilePath = Path.Combine(jsonFolderPath, $"RaidGetBest{SearchAccountId}.json");
                    File.WriteAllText(responseFilePath, response.Content);
                    Console.WriteLine($"RaidGetBest{SearchAccountId}.json created");

                    // Upload the JSON content to the server
                    //UploadJsonToServer(responseFilePath);
                    CheckAndPauseAt3AM();
                    Thread.Sleep(100);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading JP_RaidOpponentListUserID&Nickname.json: {ex.Message}");
            }

            if (RaidOpponentList.shouldContinue)
            {
                RaidOpponentList.isfinishloop = false;
                RaidOpponentList.RaidOpponentListMain(args, DateTime.MinValue, DateTime.MinValue);
            }
            else
            {
                RaidOpponentList.isfinishloop = true;
                RaidOpponentList.RaidOpponentListMain(args, DateTime.MinValue, DateTime.MinValue);
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
