using mxdat.NetworkProtocol;
using RestSharp;
using System.Net;
using Newtonsoft.Json.Linq;
using System.Text;

namespace mxdat
{
    public class GetFriendDetailedInfo
    {
        public static void GetFriendDetailedInfoMain(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;
            CheckAndPauseAt3AM();
            ExecuteMainLogic(args);
            // Execute the next method after the main logic completes
            main.Main(args);
        }

        private static void ExecuteMainLogic(string[] args)
        {
            string rootDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string mxdatjson = Path.Combine(rootDirectory, "mxdat.json");
            string FriendSearchFolderPath = Path.Combine(rootDirectory, "FriendSearch");
            string Friendjson = Path.Combine(FriendSearchFolderPath, "FriendSearch.json");
            string jsonFolderPath = Path.Combine(rootDirectory, "GetFriendDetailedInfo");

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

            static string ExtractFriendAccountId(string Friendjson)
            {
                // Read the content of the Friendjson file
                string jsonData = File.ReadAllText(Friendjson);
                JObject jsonObject = JObject.Parse(jsonData);

                // Check for error in protocol
                if (jsonObject["protocol"]?.ToString() == "Error")
                {
                    Console.WriteLine("好友碼錯誤");
                    main.Main(new string[0]);  // Call main.Main(args) to restart
                    return null;
                }

                string packetString = jsonObject["packet"].ToString();
                JObject packetObject = JObject.Parse(packetString);
                JArray searchResultArray = (JArray)packetObject["SearchResult"];
                string FriendAccountId = (string)searchResultArray[0]["AccountId"];
                return FriendAccountId;
            }

            string mxToken = ExtractMxToken(mxdatjson);
            long hash = 184692183662621;
            string accountServerId = ExtractAccountServerId(mxdatjson);
            string accountId = ExtractAccountId(mxdatjson);
            string FriendAccountId = ExtractFriendAccountId(Friendjson);

            // If there's an error and we returned null, stop further processing
            if (FriendAccountId == null)
            {
                return;
            }

            string baseJson = "{{\"Protocol\": 43002, " +
                              "\"FriendAccountId\": \"{0}\", " +
                              "\"ClientUpTime\": 2019, " +
                              "\"Resendable\": true, " +
                              "\"Hash\": {1}, " +
                              "\"IsTest\": false, " +
                              "\"SessionKey\":{{" +
                              "\"AccountServerId\": {3}, " +
                              "\"MxToken\": \"{2}\"}}, " +
                              "\"AccountId\": \"{4}\"}}";

            // Execute the loop logic just once
            string json = string.Format(baseJson, FriendAccountId, hash, mxToken, accountServerId, accountId);
            byte[] mx = instance.RequestToBinary(Protocol.Friend_GetFriendDetailedInfo, json);
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
                    Thread.Sleep(900); // Wait 2 seconds before retrying
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Request failed: {ex.Message}, retrying...");
                Thread.Sleep(2000); // Wait 2 seconds before retrying
            }

            string responseFilePath = Path.Combine(jsonFolderPath, $"GetFriendDetailedInfo.json");
            File.WriteAllText(responseFilePath, response.Content);

            // Upload the JSON content to the server
            UploadJsonToServer(responseFilePath);
            // Update rankValue and hash for next run if needed
            Thread.Sleep(1000); // Wait 2 seconds before the next iteration
            CheckAndPauseAt3AM();
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
                ExecuteMain();
            }
        }

        private static void ExecuteMain()
        {
            Console.WriteLine("Running Main...");
            string[] emptyArgs = new string[0];
            main.Main(emptyArgs);
        }
    }
}
