using System;
using System.IO;

namespace mxdat
{
    public class main
    {
        public static void Main(string[] args)
        {
            string rootDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string mxDirectory = Path.Combine(rootDirectory, "mx");
            
            // Create mx directory if it doesn't exist
            if (!Directory.Exists(mxDirectory))
            {
                Directory.CreateDirectory(mxDirectory);
                Console.WriteLine("Mx folder created");
            }
            else
            {
                Console.WriteLine("mx資料夾已存在");
            }
            
            string filePath = Path.Combine(mxDirectory, "mx.dat");
            
            // Check if mx.dat file exists
            if (!File.Exists(filePath))
            {
                Console.WriteLine("沒有mx.dat");
                return;
            }
            else
            {
                // Process FriendCode
                ProcessFriendCode(rootDirectory, args);
            }
        }

        private static void ProcessFriendCode(string rootDirectory, string[] args)
        {
            string friendCodeFilePath = Path.Combine(rootDirectory, "FriendCode.txt");
            string saveFriendCodeFilePath = Path.Combine(rootDirectory, "SaveFriendCode.txt");

            if (!File.Exists(friendCodeFilePath))
            {
                Console.WriteLine("FriendCode.txt file not found.");
                return;
            }

            string friendCode = File.ReadAllText(friendCodeFilePath).Trim();

            if (string.IsNullOrEmpty(friendCode))
            {
                Console.WriteLine("FriendCode.txt is empty or contains invalid data.");
                return;
            }

            bool codesMatch = false;

            while (!codesMatch)
            {
                if (File.Exists(saveFriendCodeFilePath))
                {
                    string savedFriendCode = File.ReadAllText(saveFriendCodeFilePath).Trim();

                    if (friendCode == savedFriendCode)
                    {
                        Console.WriteLine("FriendCode.txt matches SaveFriendCode.txt, re-reading FriendCode.txt.");
                        friendCode = File.ReadAllText(friendCodeFilePath).Trim();
                        Console.WriteLine($"FriendCode: {friendCode}");
                    }
                    else
                    {
                        codesMatch = true; // The codes are different, exit loop
                    }
                }
                else
                {
                    codesMatch = true; // SaveFriendCode.txt does not exist, exit loop
                }

                // Optionally, add a delay to prevent excessive CPU usage
                System.Threading.Thread.Sleep(1000); // Wait 1 second before re-checking
            }

            // Save the current FriendCode to SaveFriendCode.txt
            File.WriteAllText(saveFriendCodeFilePath, friendCode);

            // Execute Decryptmxdat since codes are different or SaveFriendCode.txt doesn't exist
            Console.WriteLine("Executing Decryptmxdat...");
            Decryptmxdat.DecryptMain(args);
        }
    }
}
