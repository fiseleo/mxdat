using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using System.Threading;

namespace mxdat
{
    public class SeasonData
    {
        public int SeasonId { get; set; }
        public int SeasonDisplay { get; set; }
        public DateTime SeasonStartData { get; set; }
        public DateTime SeasonEndData { get; set; }
        public DateTime SettlementEndDate { get; set; }
        public List<string> OpenRaidBossGroup { get; set; }
        public string OpenRaidBossGroup01 { get; set; }
        public string OpenRaidBossGroup02 { get; set; }
        public string OpenRaidBossGroup03 { get; set; }
        public string SourceFile { get; set; }
    }

    public class Getlist
    {
        public static SeasonData GetClosestSeason()
        {
            string rootPath = AppDomain.CurrentDomain.BaseDirectory;
            string sourcePath = Path.Combine(rootPath, "mxdatpy", "extracted", "Excel");
            string eliminatorFileName = "EliminateRaidSeasonManageExcelTable.json";
            string raidSeasonFileName = "RaidSeasonManageExcelTable.json";

            string sourceEliminatorFilePath = Path.Combine(sourcePath, eliminatorFileName);
            string sourceRaidSeasonFilePath = Path.Combine(sourcePath, raidSeasonFileName);

            // Read JSON files as strings
            string eliminateRaidSeasonJson = File.ReadAllText(sourceEliminatorFilePath);
            string raidSeasonJson = File.ReadAllText(sourceRaidSeasonFilePath);

            // Deserialize JSON data
            var eliminateRaidSeasons = JsonConvert.DeserializeObject<List<SeasonData>>(eliminateRaidSeasonJson);
            var raidSeasons = JsonConvert.DeserializeObject<List<SeasonData>>(raidSeasonJson);

            // Assign SourceFile property
            foreach (var season in eliminateRaidSeasons)
            {
                season.SourceFile = eliminatorFileName;
            }

            foreach (var season in raidSeasons)
            {
                season.SourceFile = raidSeasonFileName;
            }

            // Combine both lists
            var combinedSeasons = new List<SeasonData>();
            combinedSeasons.AddRange(eliminateRaidSeasons);
            combinedSeasons.AddRange(raidSeasons);

            // Handle OpenRaidBossGroup differences
            foreach (var season in combinedSeasons)
            {
                if (season.OpenRaidBossGroup == null)
                {
                    season.OpenRaidBossGroup = new List<string>();
                }
                if (!string.IsNullOrEmpty(season.OpenRaidBossGroup01))
                {
                    season.OpenRaidBossGroup.Add(season.OpenRaidBossGroup01);
                }
                if (!string.IsNullOrEmpty(season.OpenRaidBossGroup02))
                {
                    season.OpenRaidBossGroup.Add(season.OpenRaidBossGroup02);
                }
                if (!string.IsNullOrEmpty(season.OpenRaidBossGroup03))
                {
                    season.OpenRaidBossGroup.Add(season.OpenRaidBossGroup03);
                }
            }

            // Find the closest season to the current time
            var now = DateTime.Now;
            SeasonData closestSeason = null;
            TimeSpan minTimeSpan = TimeSpan.MaxValue;

            foreach (var season in combinedSeasons)
            {
                var timeSpan = (season.SeasonStartData - now).Duration();
                if (timeSpan < minTimeSpan)
                {
                    minTimeSpan = timeSpan;
                    closestSeason = season;
                }
            }

            return closestSeason;
        }

        public static void GetlistMain(string[] args)
        {
            while (true)
            {
                var closestSeason = GetClosestSeason();
                var now = DateTime.Now;

                // Output the closest OpenRaidBossGroup to the Console if the start date and time is now
                if (closestSeason != null && now >= closestSeason.SeasonStartData && now < closestSeason.SeasonEndData)
                {
                    if (closestSeason.OpenRaidBossGroup != null && closestSeason.OpenRaidBossGroup.Count > 0)
                    {
                        Console.WriteLine("現在開放的是:");
                        foreach (var bossGroup in closestSeason.OpenRaidBossGroup)
                        {
                            Console.WriteLine(bossGroup);
                        }

                        if (closestSeason.SourceFile == "EliminateRaidSeasonManageExcelTable.json")
                        {
                            Console.WriteLine("Executing EliminateRaidOpponentList...");
                            EliminateRaidOpponentList.EliminateRaidOpponentListMain(args, closestSeason.SeasonEndData, closestSeason.SettlementEndDate);
                        }
                        else if (closestSeason.SourceFile == "RaidSeasonManageExcelTable.json")
                        {
                            Console.WriteLine("Executing RaidOpponentList...");
                            //RaidOpponentListjson.RaidOpponentListjsonMain(args);
                            
                            RaidOpponentList.RaidOpponentListMain(args, closestSeason.SeasonEndData, closestSeason.SettlementEndDate);
                            //RaidGetBestTeam.RaidGetBestTeamMain(args);
                        }
                        break;
                    }
                    else
                    {
                        Console.WriteLine("沒有開放。");
                    }
                }
                else
                {
                    Console.WriteLine("沒有開放。");
                    Thread.Sleep(60000); // Sleep for 1 minute
                    Decryptmxdat.DecryptMain(args);
                }
            }
        }
    }
}
