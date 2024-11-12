using System.Reflection;
using SCHALE.Common.Crypto;
using System.IO;

namespace mxdat
{
    public class pythonScipt
    {
        public static void pythonSciptMain(string[] args)
        {
            string rootDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string sourceFilePath = Path.Combine(rootDirectory, "Excel.zip");
            string targetDirectoryPath = Path.Combine(rootDirectory, "extracted");

            if (!Directory.Exists(targetDirectoryPath))
            {
                Directory.CreateDirectory(targetDirectoryPath);
            }
            else
            {
                Console.WriteLine("Directory already exists");
            }

            if (!File.Exists(sourceFilePath))
            {
                Console.WriteLine("File does not exist");
                return;
            }

            try
            {
                // Use DumpExcels method to extract Excel.zip
                string bytesDir = targetDirectoryPath;
                string destDir = Path.Combine(rootDirectory, "extracted_excels");

                // Check if the directories are valid
                if (string.IsNullOrEmpty(bytesDir) || string.IsNullOrEmpty(destDir))
                {
                    Console.WriteLine("Error: Directory path cannot be null or empty.");
                    return;
                }

                if (!Directory.Exists(destDir))
                {
                    Directory.CreateDirectory(destDir);
                }

                Console.WriteLine($"Bytes Directory: {bytesDir}");
                Console.WriteLine($"Destination Directory: {destDir}");
                
                // Dump the excels
                TableService.DumpExcels(bytesDir, destDir);
                Console.WriteLine("Excel files extracted successfully");

                // Call Updatalist without modifying JSON
                Updatalist.UpdatalistMain(args);
            }
            catch (TargetInvocationException ex)
            {
                Console.WriteLine($"Error: {ex.InnerException?.Message}");
                Console.WriteLine($"Stack Trace: {ex.InnerException?.StackTrace}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }
    }
}
