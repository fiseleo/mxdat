using mxdat.NetworkProtocol;
using RestSharp;
using System.Net;

namespace mxdat
{
    public class EliminateRaidOpponentList
    {
        public static void EliminateRaidOpponentListMain(string[] args)
        {
            string jsonFolderPath = Path.Combine(Directory.GetCurrentDirectory(), "EliminateRaidOpponentList");
            if (!Directory.Exists(jsonFolderPath))
            {
                Directory.CreateDirectory(jsonFolderPath);
                Console.WriteLine("EliminateRaidOpponentList folder created");
            }
            else
            {
                Console.WriteLine("EliminateRaidOpponentList folder already exists");
            }
            PacketCryptManager Instance = new PacketCryptManager();
            int rankValue = 1;
            Console.WriteLine("Please enter Hash:");
            long hash = long.Parse(Console.ReadLine());
            Console.WriteLine("Please enter MxToken:");
            string mxtoken = Console.ReadLine();
            Console.WriteLine("Please enter AccountServer:");
            int AccountServerId = int.Parse(Console.ReadLine());
            Console.WriteLine("Please enter AccountId:");
            int AccountId = int.Parse(Console.ReadLine());

            string baseJson = "{{\"Protocol\": 45002, " +
                "\"Rank\": {0}, " +
                "\"Score\": null, " +
                "\"IsUpper\": false, " +
                "\"IsFirstRequest\": true, " +
                "\"SearchType\": 1, " +
                "\"ClientUpTime\": 4, " +
                "\"Resendable\": true, " +
                "\"Hash\": {1}, " +      // input Hash
                "\"IsTest\": false, " +
                "\"SessionKey\":{" +
                    // input SessionKey
                    "{\"AccountServerId\": {3}, " +
                    "\"MxToken\": \"{2}\"}}, " +
                    "\"AccountId\": \"{4}\"}}";
            
            while (true)
            {
                string json = string.Format(baseJson, rankValue, hash, mxtoken, AccountServerId, AccountId);
                Console.WriteLine(json);
                byte[] mx = Instance.RequestToBinary(Protocol.EliminateRaid_OpponentList, json);
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
                        Console.WriteLine("Response is empty or request failed, retrying...");
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

                //if (!response.Content.Contains("OpponentUserDBs"))
                //{
                    //Console.WriteLine(response.Content);
                    //Console.WriteLine("No player information found, terminating...");
                    //break;
                //}
                if (rankValue == 136) //test
                {
                    Console.WriteLine("stop");
                    break;
                }
                string responseFilePath = Path.Combine(jsonFolderPath, $"EliminateRaidOpponentList{rankValue}.json");
                File.WriteAllText(responseFilePath, response.Content);
                if (rankValue == 1)
                {
                    rankValue += 15;
                }
                else
                {
                    rankValue += 30;
                }
                hash++;
                Thread.Sleep(1000);
            }
            EliminateRaidOpponentListjson.EliminateRaidOpponentListjsonMain(args);
        }
    }
}
