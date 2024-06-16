using mxdat.NetworkProtocol;
using RestSharp;
using System.Net;
using Newtonsoft.Json.Linq;

namespace mxdat
{
    public class RaidGetBestTeam
    {
        public static void RaidGetBestTeamMain(string RaidGetBestTeamUserID)
        {
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

            PacketCryptManager Instance = new PacketCryptManager();
            int SearchAccountId = int.Parse(RaidGetBestTeamUserID);
            Console.WriteLine("Please enter Hash:");
            long hash = long.Parse(Console.ReadLine());
            Console.WriteLine("Please enter MxToken:");
            string mxtoken = Console.ReadLine();
            Console.WriteLine("Please enter AccountServer:");
            int AccountServerId = int.Parse(Console.ReadLine());
            Console.WriteLine("Please enter AccountId:");
            int AccountId = int.Parse(Console.ReadLine());

            string baseJson = "{{\"Protocol\": 17020, " +
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

            int maxAttempts = 2; // Specify the number of attempts or a condition
            for (int attempt = 0; attempt < maxAttempts; attempt++)
            {
                string RaidFolderPath = Path.Combine(Directory.GetCurrentDirectory(), "RaidOpponentList");
                string ExtractAccountIdAndNicknamePath = Path.Combine(RaidFolderPath, "RaidOpponentListUserID&Nickname.json");
                try
                {
                    string jsonContent = File.ReadAllText(ExtractAccountIdAndNicknamePath);
                    JArray opponents = JArray.Parse(jsonContent);
                    foreach (JObject opponent in opponents)
                    {
                        long accountId = opponent.Value<long>("AccountId");
                        if (accountId == SearchAccountId)
                        {
                            int rank = opponent.Value<int>("Rank");
                            hash += rank; // Add Rank to hash
                            break;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error reading RaidOpponentListUserID&Nickname.json: {ex.Message}");
                    continue;
                }
                string json = string.Format(baseJson, SearchAccountId, hash, mxtoken, AccountServerId, AccountId);
                byte[] mx = Instance.RequestToBinary(Protocol.Raid_GetBestTeam, json);
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
                        Console.WriteLine("Empty response or request failed, retrying...");
                        Thread.Sleep(2000); // Wait for 2 seconds before retrying
                        continue;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Request failed: {ex.Message}, retrying...");
                    Thread.Sleep(2000); // Wait for 2 seconds before retrying
                    continue;
                }
                string responseFilePath = Path.Combine(jsonFolderPath, $"RaidGetBest{SearchAccountId}.json");
                File.WriteAllText(responseFilePath, response.Content);
                Thread.Sleep(1000);
            }
            string[] emptyArgs = new string[0]; 
            RaidGetBestTeamjson.RaidGetBestTeamjsonMain(emptyArgs);
        }
    }
}
