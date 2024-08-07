using mxdat;
using System;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;

class APKzip
{
    public static async void zipMain(string[] args)
    {
        // Read the APK file path from apk_path.txt
        string apkPathFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory,"mxdatpy", "apk_path.txt");

        if (!File.Exists(apkPathFile))
        {
            Console.WriteLine("Error: APK path file not found, the program will be closed");
            return;
        }

        GlobalData.XapkFile = File.ReadAllText(apkPathFile).Trim();

        Console.WriteLine($"Read APK path: {GlobalData.XapkFile}");

        if (string.IsNullOrEmpty(GlobalData.XapkFile))
        {
            Console.WriteLine("Error: APK file path is empty, the program will be closed");
            return;
        }

        if (!File.Exists(GlobalData.XapkFile))
        {
            Console.WriteLine($"Error: APK file not found at the specified path: {GlobalData.XapkFile}");
            return;
        }

        // Ensure the file extension is correct
        if (!GlobalData.XapkFile.EndsWith(".xapk", StringComparison.OrdinalIgnoreCase))
        {
            Console.WriteLine($"Error: The provided file is not an .xapk file. Provided file: {GlobalData.XapkFile}");
            return;
        }

        // Normalize paths to ensure compatibility across OS
        string currentDirectory = Path.GetFullPath(AppDomain.CurrentDomain.BaseDirectory);
        string extractionRelativePath = Path.Combine(currentDirectory, "mxdatpy", "APK", "unzip");
        string xapkFilePath = Path.GetFullPath(GlobalData.XapkFile);

        Console.WriteLine($"Extraction path: {extractionRelativePath}");

        try
        {
            string extractionPath = Path.GetFullPath(extractionRelativePath);
            if (await UnpackZip(xapkFilePath, extractionPath))
            {
                foreach (var apkFile in Directory.GetFiles(extractionPath, "*.apk", SearchOption.TopDirectoryOnly))
                {
                    if (!await UnpackZip(apkFile, extractionPath))
                    {
                        Console.WriteLine($"Error unpacking apk file: {apkFile}");
                        return;
                    }
                }
            }
            else
            {
                Console.WriteLine("Error unpacking xapk file");
                return;
            }

            Console.WriteLine("APK extracted successfully!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
        APKver.APKverMain(args);
    }

    private static async Task<bool> UnpackZip(string zipFile, string extractionPath)
    {
        try
        {
            Directory.CreateDirectory(extractionPath);

            using (ZipArchive archive = ZipFile.OpenRead(zipFile))
            {
                foreach (ZipArchiveEntry entry in archive.Entries)
                {
                    string entryFullName = Path.Combine(extractionPath, entry.FullName);
                    string entryDirectory = Path.GetDirectoryName(entryFullName);

                    if (string.IsNullOrEmpty(entryDirectory))
                        continue; // Skip if the entry is for directory

                    Directory.CreateDirectory(entryDirectory);

                    if (File.Exists(entryFullName))
                    {
                        Console.WriteLine($"Skipped: {entryFullName} already exists.");
                        continue; // Skip if the file already exists
                    }

                    entry.ExtractToFile(entryFullName);
                }
            }
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            return false;
        }
    }
}
