namespace mxdat
{
    public class main
    {
        public static void Main(string[] args)
        {
            if (!File.Exists(@"C:\ba\mx.dat"))
            {
                Console.WriteLine("C:\\ba沒有mx.dat");
                return;
            }
            else
            {
                Decryptmxdat.DecryptMain(args);
            }
            while (true)
            {
                Console.WriteLine("Please enter an option:");
                Console.WriteLine("0 - Raid Opponent List");
                Console.WriteLine("1 - Eliminate Raid Opponent List");
                Console.WriteLine("2 - Check Raid Best Team");
                Console.WriteLine("3 - Check Eliminate Raid Best Team");
                Console.WriteLine("4 - Get Nexon Server json");
                Console.WriteLine("5 - Get list");
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
                        RaidGetBestTeam.RaidGetBestTeamMain(args);
                        break;
                    case "3":
                        EliminateRaidGetBestTeam.EliminateRaidGetBestTeamMain(args);
                        break;
                    case "4":
                        GetNexonServerjson.GetNexonServerjsonMain(args);
                        break;
                    case "5":
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
