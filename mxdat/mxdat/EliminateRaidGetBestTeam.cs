using mxdat.NetworkProtocol;
using RestSharp;
using System.Net;
using Newtonsoft.Json.Linq;

namespace mxdat
{
    public class EliminateRaidGetBestTeam
    {
        public static void EliminateRaidGetBestTeamMain(string[] args)
        {
            string jsonFolderPath = Path.Combine(Directory.GetCurrentDirectory(), "EliminateRaidGetBestTeam");
            string mxdatjson = Path.Combine(Directory.GetCurrentDirectory(), "mxdat.json");

            if (!Directory.Exists(jsonFolderPath))
            {
                Directory.CreateDirectory(jsonFolderPath);
                Console.WriteLine("EliminateRaidGetBestTeam folder created");
            }
            else
            {
                Console.WriteLine("EliminateRaidGetBestTeam folder already exists");
            }

            PacketCryptManager Instance = new PacketCryptManager();

            long hash = 114;
            long AccountServerId = 1;
            long AccountId = 1;

            static string ExtractMxToken(string mxdatjson)
            {
                string jsonData = File.ReadAllText(mxdatjson);
                JObject jsonObject = JObject.Parse(jsonData);
                string mxToken = jsonObject["SessionKey"]["MxToken"].ToString();
                return mxToken;
            }
            string mxToken = ExtractMxToken(mxdatjson);


            string baseJson = "{{\"Protocol\": 45003, " +
                "\"SearchAccountId\": {0}, " +
                "\"ClientUpTime\": 4, " +
                "\"Resendable\": true, " +
                "\"Hash\": {1}, " +      // input Hash
                "\"IsTest\": false, " +
                "\"SessionKey\":{" +
                    // input SessionKey
                    "{\"AccountServerId\": {3}, " +
                    "\"MxToken\": \"{2}\"}}, " +
                    "\"AccountId\": \"{4}\"}}";
            string EliminateRaidFolderPath = Path.Combine(Directory.GetCurrentDirectory(), "EliminateRaidOpponentList");
            string ExtractAccountIdAndNicknamePath = Path.Combine(EliminateRaidFolderPath, "EliminateRaidOpponentListUserID&Nickname.json");

            try
            {
                string jsonContent = File.ReadAllText(ExtractAccountIdAndNicknamePath);
                JArray opponents = JArray.Parse(jsonContent);
                foreach (JObject opponent in opponents)
                {
                    long SearchAccountId = opponent.Value<long>("AccountId");
                    int rank = opponent.Value<int>("Rank");

                    // If rank is 10000, stop processing
                    if (rank == 10000)
                    {
                        Console.WriteLine("Rank is 10000, stopping the process.");
                        break;
                    }

                    long adjustedHash = hash + rank;

                    string json = string.Format(baseJson, SearchAccountId, hash, mxToken, AccountServerId, AccountId);
                    byte[] mx = Instance.RequestToBinary(Protocol.EliminateRaid_GetBestTeam, json);
                    string filePath = "mx.dat";
                    File.WriteAllBytes(filePath, mx);
                    var client = new RestClient("https://nxm-tw-bagl.nexon.com:5000/api/gateway");
                    client.Timeout = -1;
                    var request = new RestRequest(Method.POST);
                    request.AddHeader("mx", "1");
                    request.AddFile("mx", "mx.dat");
                    IRestResponse response = null;

                    try
                    {
                        response = client.Execute(request);
                        if (response.StatusCode != HttpStatusCode.OK || string.IsNullOrWhiteSpace(response.Content))
                        {
                            Console.WriteLine("Empty response or request failed for AccountId: {0}, retrying...", SearchAccountId);
                            Thread.Sleep(2000);
                            continue;

                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Request failed for AccountId: {0}: {1}, retrying...", SearchAccountId, ex.Message);
                        Thread.Sleep(2000);
                        continue;
                    }
                    string responseFilePath = Path.Combine(jsonFolderPath, $"EliminateRaidGetBest{SearchAccountId}.json");
                    File.WriteAllText(responseFilePath, response.Content);
                    Thread.Sleep(1000);
                    
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading EliminateRaidOpponentListUserID&Nickname.json: {ex.Message}");
            }

            string[] emptyArgs = new string[0];
            EliminateRaidGetBestTeamjson.EliminateRaidGetBestTeamjsonMain(emptyArgs);
        }    

    }
}
