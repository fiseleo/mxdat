namespace mxdat
{
    public class main
    {
        public static void Main(string[] args)
        {
            while (true)
            {
                Console.WriteLine("Please enter an option:");
                Console.WriteLine("0 - Raid Opponent List");
                Console.WriteLine("1 - Eliminate Raid Opponent List");
                Console.WriteLine("2 - Check Raid Best Team");
                Console.WriteLine("3 - Check Eliminate Raid Best Team");
                Console.WriteLine("4 - Get Nexon Server json");
                Console.WriteLine("A - Get list");
                Console.WriteLine("Enter any other key to exit the program");

                string input = Console.ReadLine();

                switch (input)
                {
                    case "0":
                        RaidOpponentList.RaidOpponentListMain(args);
                        break;
                    case "1":
                        EliminateRaidOpponentList.EliminateRaidOpponentListMain(args);
                        break;
                    case "2":
                        Console.WriteLine("Please enter UserID:");
                        string RaidGetBestTeamUserID = Console.ReadLine();
                        RaidGetBestTeam.RaidGetBestTeamMain(RaidGetBestTeamUserID);
                        break;
                    case "3":
                        Console.WriteLine("Please enter UserID:");
                        string EliminateRaidGetBestTeamUserID = Console.ReadLine();
                        EliminateRaidGetBestTeam.EliminateRaidGetBestTeamMain(EliminateRaidGetBestTeamUserID);
                        break;
                    case "4":
                        GetNexonServerjson.GetNexonServerjsonMain(args);
                        break;
                    case "A":
                        Getlist.GetlistMain(args);
                        break;
                    default:
                        Console.WriteLine("Program terminated");
                        return;
                }
            }
        }
    }
}
