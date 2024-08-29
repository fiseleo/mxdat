using mxdat.NetworkProtocol;
using RestSharp;
using System.Net;
using Newtonsoft.Json.Linq;
using System.Text;

namespace mxdat
{
    public class FriendSearch
    {
        public static void FriendSearchMain(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;
            CheckAndPauseAt3AM();
            ExecuteMainLogic(args);
            // Execute the next method after the main logic completes
            SaveFriendCodeToFile();
            GetFriendDetailedInfo.GetFriendDetailedInfoMain(args);
        }

        private static void ExecuteMainLogic(string[] args)
        {
            string rootDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string mxdatjson = Path.Combine(rootDirectory, "mxdat.json");
            string jsonFolderPath = Path.Combine(rootDirectory, "FriendSearch");

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
            long hash = 184705068564508;
            string accountServerId = ExtractAccountServerId(mxdatjson);
            string accountId = ExtractAccountId(mxdatjson);

            string baseJson = "{{\"Protocol\": 43005, " +
                              "\"FriendCode\": \"{0}\", " +
                              "\"LevelOption\": 0, " +
                              "\"ClientUpTime\": 1, " +
                              "\"Resendable\": true, " +
                              "\"Hash\": {1}, " +
                              "\"IsTest\": false, " +
                              "\"SessionKey\":{{" +
                              "\"AccountServerId\": {3}, " +
                              "\"MxToken\": \"{2}\"}}, " +
                              "\"AccountId\": \"{4}\"}}";

            // Execute the loop logic just once
            string json = string.Format(baseJson, ReadFriendCodeFromFile(rootDirectory), hash, mxToken, accountServerId, accountId);
            byte[] mx = instance.RequestToBinary(Protocol.Friend_Search, json);
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

            string responseFilePath = Path.Combine(jsonFolderPath, $"FriendSearch.json");
            File.WriteAllText(responseFilePath, response.Content);
            Thread.Sleep(1000); // Wait 2 seconds before the next iteration
            CheckAndPauseAt3AM();
        }
        private static string ReadFriendCodeFromFile(string rootDirectory)
        {
            string FriendCodeFilePath = Path.Combine(rootDirectory, "FriendCode.txt");
            if (!File.Exists(FriendCodeFilePath))
            {
                throw new FileNotFoundException("FriendCode.txt file not found.");
            }
            string FriendCode = File.ReadAllText(FriendCodeFilePath).Trim();
            if (string.IsNullOrEmpty(FriendCode))
            {
                throw new InvalidOperationException("FriendCode.txt is empty or contains invalid data.");
            }
            return FriendCode;
        }

        private static void SaveFriendCodeToFile()
        {
            string rootDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string FCode = ReadFriendCodeFromFile(rootDirectory);
            string saveFriendCodeFilePath = Path.Combine(rootDirectory, "SaveFriendCode.txt");

            try
            {
                File.WriteAllText(saveFriendCodeFilePath, FCode);
                Console.WriteLine($"FriendCode saved to {saveFriendCodeFilePath}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to save FriendCode: {ex.Message}");
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
            Console.WriteLine("Running Decryptmxdat...");
            string[] emptyArgs = new string[0];
            main.Main(emptyArgs);
        }
    }
}
