using System;
using Terraria;
using TShockAPI;
using TerrariaApi.Server;
using System.Reflection;
using Wolfje.Plugins.SEconomy;
using System.Collections.Generic;
using System.Linq;
using System.Timers;

namespace Autorank
{
    [ApiVersion(1, 16)]
    public class autorank : TerrariaPlugin
    {
        System.Timers.Timer t = new System.Timers.Timer(150000) { Enabled =true};
        public List<Rank> Ranks = new List<Rank>();
        public override Version Version
        {
            get { return Assembly.GetExecutingAssembly().GetName().Version; }
        }
        public override string Author
        {
            get { return "Ancientgods"; }
        }
        public override string Name
        {
            get { return "Autoranking (based on seconomy)"; }
        }

        public override string Description
        {
            get { return "ranks people up automaticly"; }
        }

        public override void Initialize()
        {
            Commands.ChatCommands.Add(new Command(CheckRank, "checkrank"));
            AddRanks();
            t.Start();
        }

        public autorank(Main game)
            : base(game)
        {
            Order = 1;
        }

        private void Onupdate(object sender, ElapsedEventArgs args)
        {
            foreach (TSPlayer ts in TShock.Players)
            {
                List<Rank> rnks = Ranks.Where(r => r.CurrentRank == ts.Group.Name).ToList();
                if (rnks.Count < 1)
                {
                    return;
                }
                Rank rank = rnks[0];
                var DbUser = TShock.Users.GetUserByName(ts.UserAccountName);

                Wolfje.Plugins.SEconomy.Economy.EconomyPlayer EP = SEconomyPlugin.GetEconomyPlayerSafe(ts.Index);
                if (EP.BankAccount.Balance > rank.Cost)
                {
                    TShock.Users.SetUserGroup(DbUser, rank.NextRank);
                    ts.SendInfoMessage("Congratulations, you gained a rank!");
                    return;
                }
            }
        }

        private void CheckRank(CommandArgs args)
        {
            List<Rank> rnks = Ranks.Where(r => r.CurrentRank == args.Player.Group.Name).ToList();
            if (rnks.Count < 1)
            {
                args.Player.SendErrorMessage("No higher rank available!");
                return;
            }

            Rank rank = rnks[0];
            Wolfje.Plugins.SEconomy.Economy.EconomyPlayer EP = SEconomyPlugin.GetEconomyPlayerSafe(args.Player.Index);

            if (EP.BankAccount.Balance < rank.Cost)
            {
                Money m = rank.Cost - EP.BankAccount.Balance;
                args.Player.SendMessage(string.Format("You need {0} more Goldpieces for rank: {1}", m.ToLongString(), rank.NextRank), Color.Cyan);
                return;
            }
            var DbUser = TShock.Users.GetUserByName(args.Player.UserAccountName);
            TShock.Users.SetUserGroup(DbUser, rank.NextRank);
            args.Player.SendInfoMessage("Congratulations, you gained a rank!");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) {}
            base.Dispose(disposing);
        }

        public void AddRanks()
        {
            Ranks.Add(new Rank("1", "2", 1800)); //20min
            Ranks.Add(new Rank("2", "3", 11564)); //40min
            Ranks.Add(new Rank("3", "4", 26453)); //1,3hr
            Ranks.Add(new Rank("4", "5", 46432)); //2,6hr
            Ranks.Add(new Rank("5", "6", 64321)); //5,2hr
            Ranks.Add(new Rank("6", "7", 94354)); //10,4hr
            Ranks.Add(new Rank("7", "8", 153489)); //20,8hr
            Ranks.Add(new Rank("8", "9", 315643));// 1,7 day
            Ranks.Add(new Rank("9", "10", 437832)); //3,4 day
            Ranks.Add(new Rank("10", "11", 584106)); //6,8 day
        }

        public class Rank
        {
            public string CurrentRank = "";
            public string NextRank = "";
            public int Cost = 0;
            public Rank(string currentrank, string nextrank, int cost)
            {
                CurrentRank = currentrank;
                NextRank = nextrank;
                Cost = cost;
            }
        }
    }
}