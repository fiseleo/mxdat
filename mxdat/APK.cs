using mxdat;
using System.Diagnostics;
using System.Text.RegularExpressions;

class APK
{
    public static void APKMain(string[] args)
    {
        Console.WriteLine("Reading the forced version from setversion.txt file.");
        string rootDirectory = AppDomain.CurrentDomain.BaseDirectory;
        // Define the path to the text file
        string filePath = Path.Combine(rootDirectory, "setversion.txt");

        // Check if the file exists
        if (File.Exists(filePath))
        {
            // Read the first line of the file
            Console.WriteLine("Reading the forced version from setversion.txt file.");
            string parameter = File.ReadAllText(filePath).Trim();
            Console.WriteLine($"Forced version: {parameter}");

            // Set the forced version from the file
            GlobalData.IsForcedVersion = true;
            GlobalData.ForcedVersion = parameter;
        }
        else
        {
            Console.WriteLine("setversion.txt file not found. Exiting.");
            return;
        }

        string pythonInterpreter = "python";
        string pythonScriptRelativePath = Path.Combine(rootDirectory,"mxdatpy", "download_apk.py");
        string currentDirectory = Environment.CurrentDirectory;
        string fullPathToPythonScript = Path.GetFullPath(Path.Combine(currentDirectory, pythonScriptRelativePath));
        Console.WriteLine($"Python Interpreter Path: {pythonInterpreter}");
        Console.WriteLine($"Python Script Path: {fullPathToPythonScript}");
        if (!File.Exists(fullPathToPythonScript))
        {
            Console.WriteLine($"Error: Python script not found at {fullPathToPythonScript}");
            return;
        }

        ProcessStartInfo start = new ProcessStartInfo();
        start.FileName = pythonInterpreter;
        start.Arguments = fullPathToPythonScript;
        if (GlobalData.IsForcedVersion)
            start.Arguments += $" -f {GlobalData.ForcedVersion}";
        start.UseShellExecute = false;
        start.RedirectStandardOutput = true;
        start.WorkingDirectory = rootDirectory;
        
        try
        {
            using (Process process = Process.Start(start))
            {
                using (StreamReader reader = process.StandardOutput)
                {
                    string result = reader.ReadToEnd();
                    Regex r = new Regex("APK file: (.*).+?\r?$", RegexOptions.Compiled);
                    Match match = r.Match(result);
                    if (match.Success)
                    {
                        GlobalData.XapkFile = Path.Combine(currentDirectory, "mxdatpy", match.Groups[1].Value);
                    }
                    Console.WriteLine(result);
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
        APKzip.zipMain(args);
    }
}
