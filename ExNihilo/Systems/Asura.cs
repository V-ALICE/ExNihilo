using ExNihilo.Sectors;

namespace ExNihilo.Systems
{
    public static class Asura
    {
        private static bool _cheatyMode;
        private static GameContainer _game;
        private static OuterworldSector _theTown;
        private static UnderworldSector _theVoid;

        private static void PostHelp(ConsoleHandler g, string com)
        {
            switch (com)
            {
                case "help":
                    g.ForceMessage("<Help>",
                        "\n/help           -> Display all available commands" +
                        "\n/help [command] -> Display info about given command");
                    break;
                case "exportmap":
                    g.ForceMessage("<ExportMap>",
                        "\n/exportmap -> Save the current level as a PNG file");
                    break;
                case "kick":
                    g.ForceMessage("<Kick>", 
                        "\n/kick [player] -> Kick given player from game (Host only)");
                    break;
                case "op":
                    g.ForceMessage("<OP>",
                        "\n/op          -> Set self to debug mode" +
                        "\n/op [player] -> Set given player to debug mode");
                    break;
                case "deop":
                    g.ForceMessage("<DeOP>",
                        "\n/deop          -> Set self to default mode" +
                        "\n/deop [player] -> Set given player to default mode");
                    break;
                case "tgm":
                    g.ForceMessage("<TGM>", 
                        "\n/tgm -> Toggle self invincibility");
                    break;
                case "tai":
                    g.ForceMessage("<TAI>",
                        "\n/tai -> Toggle enemy AI" +
                        "\nEnemies will not move and combat will not trigger");
                    break;
                case "tp":
                    g.ForceMessage("<TP>", 
                        "\n/tp [player] -> Teleport self to given player");
                    break;
                case "noclip":
                    g.ForceMessage("<NoClip>",
                        "\n/noclip -> Toggle own collisions" +
                        "\ntoggling noclip off while outside the map area will automatically teleport the player back in");
                    break;
                case "restore":
                    g.ForceMessage("<Restore>", 
                        "\n/restore -> Restore own HP and SP to max");
                    break;
                case "clearinv":
                    g.ForceMessage("<ClearInv>",
                        "\n/clearinv -> Clears own inventory" +
                        "\nThis does not affect player gold");
                    break;
                case "stairs":
                    g.ForceMessage("<Stairs>", 
                        "\n/stairs -> Teleport self to the entrance to the next floor");
                    break;
                case "die":
                    g.ForceMessage("<Die>",
                        "\n/die -> Set self to dead state" +
                        "\nUsing this in single player will end the game");
                    break;
                case "revive":
                    g.ForceMessage("<Revive>", 
                        "\n/revive -> Revive self from dead state");
                    break;
                case "giveitem":
                    g.ForceMessage("<GiveItem>",
                        "\n/giveitem [type] [rank] -> Add item to own inventory" +
                        "\nPossible types: health, poison, fire, freeze, teleport" +
                        "\nrank ranges from 1 to 3");
                    break;
                case "giveequip":
                    g.ForceMessage("<GiveEquip>",
                        "\n/giveequip [type] [level]          -> Add equipment to own inventory" +
                        "\n/giveequip [type] [level] [rarity] -> Add equipment of given rarity to own inventory" +
                        "\nPossible types: weapon, head, chest, hands, legs, feet, trinket" +
                        "\nRarity is 4 letters consisting only of C B A S X" +
                        "\nlevel ranges from 1 to 9999");
                    break;
                case "givegold":
                    g.ForceMessage("<GiveGold>",
                        "\n/givegold [amount] -> Add given amount of gold to own inventory" +
                        "\namount ranges from 1 to 9999");
                    break;
                case "giveskill":
                    g.ForceMessage("<GiveSkill>",
                        "\n/giveskill [slot] [level] -> Add skill to own skills in given slot" +
                        "\nslot ranges from 1 to 4" +
                        "\nlevel ranges from 1 to 9999");
                    break;
                case "levelup":
                    g.ForceMessage("<LevelUp>", 
                        "\n/levelup [amount] -> Increase own level by given amount");
                    break;
                case "giveexp":
                    g.ForceMessage("<GiveExp>", 
                        "\n/giveexp [amount] -> Give self given amount of experience");
                    break;
                case "setspeed":
                    g.ForceMessage("<SetSpeed>",
                        "\n/setspeed [percent] -> Set own character movement speed to given percentage" +
                        "\npercent ranges from 1 to 900");
                    break;
                case "spawnmob":
                    g.ForceMessage("<SpawnMob>",
                        "\n/spawnmob [level] -> Spawn an enemy" +
                        "\nlevel ranges from 1 to 9999");
                    break;
                case "spawnnpc":
                    g.ForceMessage("<SpawnNPC>",
                        "\n/spawnnpc [type] -> Spawn an NPC of given type" +
                        "\nPossible types: talk, shop, bank, forge, relapse");
                    break;
                case "gotofloor":
                    g.ForceMessage("<GoToFloor>",
                        "\n/gotofloor [level] -> Teleport to given floor" +
                        "\nlevel ranges from 1 to 9999");
                    break;
                default:
                    g.ForceMessage("", "<No such command \"" + com + "\". Use /help to see available commands>");
                    break;
            }
        }
        private static void PostHelp(ConsoleHandler g)
        {
            if (_cheatyMode)
            {
                g.ForceMessage("<Possible commands>",
                    "help, kick, op, deop, exportmap, tgm, tai, noclip, restore, clearinv, stairs, die, revive, " +
                    "giveitem, giveequip, givegold, giveexp, giveskill, levelup, setspeed, spawnmob, spawnnpc, gotofloor");
            }
            else
            {
                g.ForceMessage("<Possible commands>", 
                    "help, op, deop, exportmap");
            }
        }

        private static void HandleNoArgCommands(ConsoleHandler g, string command)
        {
            if (command.Equals("help"))
            {
                PostHelp(g);
            }
            else if (command.Equals("op"))
            {
                _cheatyMode = true;
            }
            else if (command.Equals("deop"))
            {
                _cheatyMode = false;
            }
            else if (command.Equals("exportmap"))
            {
                //ExportMap(g);
            }
            else
            {
                switch (command)
                {
                    case "tgm":
                        //GodMode(g);
                        break;
                    case "restore":
                        //Restore(g);
                        break;
                    case "clearinv":
                        //ClearInventory(g);
                        break;
                    case "tai":
                        //ToggleAI(g);
                        break;
                    case "stairs":
                        //GoToStairs(g);
                        break;
                    case "die":
                        //Die(g);
                        break;
                    case "revive":
                        //Revive(g);
                        break;
                    case "noclip":
                        //NoClip(g);
                        break;
                    default:
                        PostHelp(g, command);
                        break;
                }
            }
        }
        private static void HandleArgCommands(ConsoleHandler g, string command)
        {
            if (command.StartsWith("help "))
            {
                PostHelp(g, command.Substring(5));
            }
            else if (command.StartsWith("giveitem "))
            {
                var extra = command.Substring(9);
                //GiveItem(g, extra);
            }
            else if (command.StartsWith("giveequip "))
            {
                var extra = command.Substring(10);
                //GiveEquip(g, extra);
            }
            else if (command.StartsWith("givegold "))
            {
                var extra = command.Substring(9);
                //GiveGold(g, extra);
            }
            else if (command.StartsWith("setspeed "))
            {
                var extra = command.Substring(9);
                //SetSpeed(g, extra);
            }
            else if (command.StartsWith("giveskill "))
            {
                var extra = command.Substring(10);
                //GiveSkill(g, extra);
            }
            else if (command.StartsWith("gotofloor "))
            {
                var extra = command.Substring(10);
                //GoToFloor(g, extra);
            }
            else if (command.StartsWith("spawnnpc "))
            {
                var extra = command.Substring(9);
                //SpawnNPC(g, extra);
            }
            else if (command.StartsWith("spawnmob "))
            {
                var extra = command.Substring(9);
                //SpawnMob(g, extra);
            }
            else if (command.StartsWith("levelup "))
            {
                var extra = command.Substring(8);
                //LevelUp(g, extra);
            }
            else if (command.StartsWith("giveexp "))
            {
                var extra = command.Substring(8);
                //GainExp(g, extra);
            }
            else if (command.StartsWith("tp "))
            {
                var extra = command.Substring(3);
                //TeleportToFriend(g, extra);
            }
            else
            {
                PostHelp(g, command.Substring(0, command.IndexOf(' ')));
            }
        }

        public static void Handle(ConsoleHandler g, string com)
        {
            var command = com.Substring(1).ToLower();
            if (command.Length < 1) return;

            if (command.Contains(" ")) HandleArgCommands(g, command);
            else HandleNoArgCommands(g, command);
        }

        public static void Ascend(GameContainer g, UnderworldSector u, OuterworldSector o)
        {
            _game = g;
            _theTown = o;
            _theVoid = u;
        }
    }
}
