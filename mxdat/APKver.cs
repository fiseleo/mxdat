using System;
using System.Diagnostics;
using System.IO;

namespace mxdat
{
    public class APKver
    {
        public static void APKverMain(string[] args)
        {
            string rootDirectory = AppDomain.CurrentDomain.BaseDirectory;
            // Use the system's Python interpreter
            string pythonInterpreter = "python3";
            string pythonScriptRelativePath = Path.Combine(rootDirectory,"mxdatpy", "local_info.py");
            string fullPathToPythonScript = Path.GetFullPath(Path.Combine(rootDirectory, pythonScriptRelativePath));
            Console.WriteLine($"Python Interpreter Path: {pythonInterpreter}");
            Console.WriteLine($"Python Script Path: {fullPathToPythonScript}");

            ProcessStartInfo start = new ProcessStartInfo();
            start.FileName = pythonInterpreter;
            start.Arguments = fullPathToPythonScript;
            start.WorkingDirectory = rootDirectory;
            start.UseShellExecute = false;
            start.RedirectStandardOutput = true;
            start.RedirectStandardError = true;

            try
            {
                using (Process process = Process.Start(start))
                {
                    using (StreamReader outputReader = process.StandardOutput)
                    using (StreamReader errorReader = process.StandardError)
                    {
                        string result = outputReader.ReadToEnd();
                        string error = errorReader.ReadToEnd();
                        if (!string.IsNullOrEmpty(error))
                        {
                            Console.WriteLine($"Error: {error}");
                        }
                        else
                        {
                            Console.WriteLine(result);
                        }
                    }
                }
                
                url.urlMain(args);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }
    }
}
