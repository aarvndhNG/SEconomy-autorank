using System;
using Terraria;
using TShockAPI;
using TerrariaApi.Server;
using System.Reflection;
using Wolfje.Plugins.SEconomy;
using System.Timers;
using System.Text;
using Wolfje.Plugins.SEconomy.Journal;

namespace Autorank
{
    [ApiVersion(1, 16)]
    public class autorank : TerrariaPlugin
    {
        System.Timers.Timer t = new System.Timers.Timer(300000) { Enabled = true };
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
            t.Elapsed += OnUpdate;
            t.Start();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) { }
            base.Dispose(disposing);
        }

        public autorank(Main game)
            : base(game)
        {
            Order = 1;
        }

        private void OnUpdate(object sender, ElapsedEventArgs args)
        {
            foreach (TSPlayer ts in TShock.Players)
            {
                if (ts.IsLoggedIn)
                {
                    IBankAccount Player = SEconomyPlugin.Instance.GetBankAccount(ts.Index);


                    int i;
                    if (int.TryParse(ts.Group.Name, out i))
                    {
                        if (TShock.Groups.GroupExists((i + 1).ToString()))
                        {
                            int RankCost = GetXp(i);
                            if (Player.Balance >= RankCost)
                            {
                                int r = GiveItem((i + 1), ts);
                                if (r > 0)
                                {
                                    var DbUser = TShock.Users.GetUserByName(ts.UserAccountName);
                                    TShock.Users.SetUserGroup(DbUser, (i + 1).ToString());
                                    ts.SendInfoMessage("Congratulations, you gained a Level!");
                                }
                            }
                        }
                    }
                }
            }
        }

        private void CheckRank(CommandArgs args)
        {
            if (!args.Player.IsLoggedIn)
            {
                args.Player.SendErrorMessage("You need to be logged in to use this command!");
                return;
            }

            IBankAccount Player = SEconomyPlugin.Instance.GetBankAccount(args.Player.Index);

            int i;
            if (int.TryParse(args.Player.Group.Name, out i))
            {
                if (!TShock.Groups.GroupExists((i + 1).ToString()))
                {
                    args.Player.SendErrorMessage("No higher level available!");
                    return;
                }
                int RankCost = GetXp(i);
                if (Player.Balance >= RankCost)
                {
                    int r = GiveItem((i + 1), args.Player);
                    if (r > 0)
                    {
                        args.Player.SendErrorMessage("You don't have enough free inventory spaces to receive items!");
                        args.Player.SendErrorMessage("You need " + r + " more free inventory space(s)!");
                        return;
                    }
                    var DbUser = TShock.Users.GetUserByName(args.Player.UserAccountName);
                    TShock.Users.SetUserGroup(DbUser, (i + 1).ToString());
                    args.Player.SendInfoMessage("Congratulations, you gained a Level!");
                    return;
                }
                else
                {
                    Money m = RankCost - Player.Balance;
                    args.Player.SendMessage(string.Format("You need {0} more XP for Level: {1}", m.ToLongString(), (i + 1).ToString()), Color.Cyan);
                    return;

                }
            }
            else
            {
                args.Player.SendErrorMessage("You can't use this command!");
                return;
            }
        }

        public int GiveItem(int rank, TSPlayer plr)
        {
            switch (rank)
            {
                case 50:
                case 100:
                    return GiveItem(plr, GetItem(1, 5), GetItem(2, 5));
                case 150:
                    return GiveItem(plr, GetItem(6, 2), GetItem(9, 5), GetItem(5, 100));
                default:
                    return 0;
            }
        }

        public Item GetItem(int id, int stack)
        {
            Item it = new Item();
            it.SetDefaults(id);
            it.stack = stack > it.maxStack ? it.maxStack : stack;
            return it;
        }

        public int GiveItem(TSPlayer plr, params Item[] Items)
        {
            int EmptySlots = 0;

            for (int i = 0; i < 50; i++) //51 is trash can, 52-55 is coins, 56-59 is ammo
            {
                if (plr.TPlayer.inventory[i] == null || !plr.TPlayer.inventory[i].active || plr.TPlayer.inventory[i].name == "")
                {
                    EmptySlots++;
                }
            }

            if (EmptySlots < Items.Length)
                return Items.Length - EmptySlots;

            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < Items.Length; i++)
            {
                plr.GiveItemCheck(Items[i].type, Items[i].name, Items[i].width, Items[i].height, Items[i].stack, Items[i].prefix);

                sb.Append(Items[i].name + " (" + Items[i].stack + ")");
                if(i != Items.Length-1)
                    sb.Append(",");
            }

            plr.SendInfoMessage("You have received the following item(s) for ranking up:");
            plr.SendInfoMessage(sb.ToString());
            return 0;
        }

        public static int GetXp(int level)
        {
            return (int)((level * level) / 7.331 * 1337.0f * 1.337f);
        }
    }
}