using mxdat.NetworkProtocol;
using RestSharp;
using System.Net;
using Newtonsoft.Json.Linq;

namespace mxdat
{
    public class RaidGetBestTeam
    {
        public static void RaidGetBestTeamMain(string[] args)
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

            string RaidFolderPath = Path.Combine(Directory.GetCurrentDirectory(), "RaidOpponentList");
            string ExtractAccountIdAndNicknamePath = Path.Combine(RaidFolderPath, "RaidOpponentListUserID&Nickname.json");

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

                    long adjustedHash = hash + rank; // Adjust hash with rank

                    string json = string.Format(baseJson, SearchAccountId, adjustedHash, mxtoken, AccountServerId, AccountId);
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
                    Thread.Sleep(1000);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading RaidOpponentListUserID&Nickname.json: {ex.Message}");
            }
            
            string[] emptyArgs = new string[0];
            RaidGetBestTeamjson.RaidGetBestTeamjsonMain(emptyArgs);
        }
    }
}
