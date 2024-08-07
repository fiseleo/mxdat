using mxdat;
using Newtonsoft.Json.Linq;
using RestSharp;
using System;
using System.IO;

class url
{
    public static void urlMain(string[] args)
    {
        string urlFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "mxdatpy", "APK", "url.txt");
        try
        {
            string urlContent = File.ReadAllText(urlFilePath);
            Console.WriteLine("URL Content:");
            Console.WriteLine(urlContent);

            var client = new RestClient(urlContent);
            var request = new RestRequest(Method.GET);

            try
            {
                IRestResponse response = client.Execute(request);
                Console.WriteLine($"Response status code: {response.StatusCode}");

                if (response.IsSuccessful)
                {
                    string jsonContent = response.Content;
                    JObject json = JObject.Parse(jsonContent);
                    JToken overrideGroups = json.SelectToken("ConnectionGroups[0].OverrideConnectionGroups");

                    if (overrideGroups != null && overrideGroups.HasValues)
                    {
                        bool foundSecondRoot = false;
                        foreach (var group in overrideGroups)
                        {
                            string addressablesCatalogUrlRoot = group.Value<string>("AddressablesCatalogUrlRoot");
                            if (!string.IsNullOrEmpty(addressablesCatalogUrlRoot))
                            {
                                if (foundSecondRoot)
                                {
                                    string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "mxdatpy", "APK", "AddressablesCatalogUrlRoot.txt");
                                    File.WriteAllText(filePath, addressablesCatalogUrlRoot);
                                    Console.WriteLine("AddressablesCatalogUrlRoot: " + addressablesCatalogUrlRoot);
                                    break;
                                }
                                else
                                {
                                    foundSecondRoot = true;
                                }
                            }
                        }
                    }
                    else
                    {
                        Console.WriteLine("OverrideConnectionGroups not found in JSON.");
                    }
                }
                else
                {
                    Console.WriteLine($"Error: Failed to get JSON data. Status code: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }
        catch (FileNotFoundException e)
        {
            Console.WriteLine("Error: File not found: " + e.Message);
        }
        catch (UnauthorizedAccessException e)
        {
            Console.WriteLine("Error: Unauthorized access: " + e.Message);
        }
        catch (Exception e)
        {
            Console.WriteLine("Error: " + e.Message);
        }

        try
        {
            GetExcelzip.GetExcelzipMain(args);
        }
        catch (Exception e)
        {
            Console.WriteLine("Error calling GetExcelzipMain: " + e.Message);
        }
    }
}
