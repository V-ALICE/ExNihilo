using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ExNihilo.Sectors;
using ExNihilo.Systems.Backend;
using ExNihilo.Systems.Game;
using ExNihilo.Systems.Game.Items;
using ExNihilo.Util;
using Microsoft.Xna.Framework;

namespace ExNihilo.Systems
{
    public static class Asura
    {
        private static bool _elevatedMode=true;
        private static GameContainer _game;
        private static OuterworldSector _theTown;
        private static VoidSector _theVoid;
        private static ConsoleHandler Log => GameContainer.Console;

        private static Dictionary<string, Action<string>> _basicCommands;
        private static Dictionary<string, Action<string>> _elevatedCommands;
        private static Dictionary<string, Action<string>> _paramSet;
        private static string _basicHelpString, _elevatedHelpString;
        private static Dictionary<string, string> _helpInfo;

        //Called whenever a command-form message is issued
        public static void Handle(string com)
        {
            var command = com.Substring(1).ToLower(); //remove command starter /
            if (command.Length < 1) return; //if the command is blank don't do anything

            var sepIndex = command.IndexOf(' ');
            var baseCommand = sepIndex >= 0 ? command.Substring(0, sepIndex) : command;
            var extra = sepIndex >= 0 ? command.Substring(sepIndex+1) : "";
            if (_basicCommands.TryGetValue(baseCommand, out var basic))
            {
                basic.Invoke(extra);
            }
            else if (_elevatedCommands.TryGetValue(baseCommand, out var elevated))
            {
                if (_elevatedMode) elevated.Invoke(extra);
                else Log.ForceMessage("<error>", "You do not have permission to use \"" + baseCommand + "\"", Color.DarkRed, Color.White);
            }
            else
            {
                Log.ForceMessage("<error>", "No such command \"" + baseCommand + "\". Use /help to see available commands", Color.DarkRed, Color.White);
            }
        }

        //Initial setup
        private static void SetupParams()
        {
            //Sets player collisions on or off
            void SetCollisions(string args)
            {
                if (args == "on")
                {
                    Log.ForceMessage("<Asura>", "Enabling collisions", Color.Purple, Color.White);
                    _theVoid.ToggleCollisions(true);
                    _theTown.ToggleCollisions(true);
                }
                else if (args == "off")
                {
                    Log.ForceMessage("<Asura>", "Disabling collisions", Color.Purple, Color.White);
                    _theVoid.ToggleCollisions(false);
                    _theTown.ToggleCollisions(false);
                }
                else
                {
                    Log.ForceMessage("<error>", "\"" + args + "\" is not valid. Value must be \"on\" or \"off\"", Color.DarkRed, Color.White);
                }
            }
            _paramSet.Add("collision", SetCollisions);

            //Changes to the given floor
            void SetFloor(string args)
            {
                if (GameContainer.ActiveSectorID != GameContainer.SectorID.Void)
                {
                    Log.ForceMessage("<error>", "Can only change floors from within the void", Color.DarkRed, Color.White);
                    return;
                }

                if (int.TryParse(args, out int num) && num > 0)
                {
                    Log.ForceMessage("<Asura>", "Swapping to floor " + args, Color.Purple, Color.White);
                    _theVoid.SetFloor(num);
                }
                else Log.ForceMessage("<error>", "\"" + args + "\" is not a valid floor value", Color.DarkRed, Color.White);
            }
            _paramSet.Add("floor", SetFloor);

            //Sets level generation type
            void SetGenType(string args)
            {
                switch (args)
                {
                    case "standard1":
                        Log.ForceMessage("<Asura>", "Changing generation algorithm to standard1", Color.Purple, Color.White);
                        _theVoid.SetGenType(MapGenerator.Type.Standard1);
                        if (GameContainer.ActiveSectorID == GameContainer.SectorID.Void) _theVoid.SetFloor();
                        break;
                    case "standard2":
                        Log.ForceMessage("<Asura>", "Changing generation algorithm to standard2", Color.Purple, Color.White);
                        _theVoid.SetGenType(MapGenerator.Type.Standard2);
                        if (GameContainer.ActiveSectorID == GameContainer.SectorID.Void) _theVoid.SetFloor();
                        break;
                    case "messy":
                        Log.ForceMessage("<Asura>", "Changing generation algorithm to messy", Color.Purple, Color.White);
                        _theVoid.SetGenType(MapGenerator.Type.MessyBoxes);
                        if (GameContainer.ActiveSectorID == GameContainer.SectorID.Void) _theVoid.SetFloor();
                        break;
                    default:
                        Log.ForceMessage("<error>", "\"" + args + "\" is not a valid gentype value", Color.DarkRed, Color.White);
                        break;
                }
            }
            _paramSet.Add("gentype", SetGenType);

            //Sets HP to given value
            void SetHP(string args)
            {
                if (GameContainer.ActiveSectorID == GameContainer.SectorID.MainMenu)
                {
                    Log.ForceMessage("<error>", "Cannot change player values on title screen", Color.DarkRed, Color.White);
                    return;
                }

                if (int.TryParse(args, out int num) && num >= 0)
                {
                    Log.ForceMessage("<Asura>", "Setting current player HP to " + args, Color.Purple, Color.White);
                    _game.Player.Inventory.SetHPMP(num);
                }
                else Log.ForceMessage("<error>", "\"" + args + "\" is not a valid HP value", Color.DarkRed, Color.White);
            }
            _paramSet.Add("hp", SetHP);

            //Sets MP to given value
            void SetMP(string args)
            {
                if (GameContainer.ActiveSectorID == GameContainer.SectorID.MainMenu)
                {
                    Log.ForceMessage("<error>", "Cannot change player values on title screen", Color.DarkRed, Color.White);
                    return;
                }

                if (int.TryParse(args, out int num) && num >= 0)
                {
                    Log.ForceMessage("<Asura>", "Setting current player MP to " + args, Color.Purple, Color.White);
                    _game.Player.Inventory.SetHPMP(-1, num);
                }
                else Log.ForceMessage("<error>", "\"" + args + "\" is not a valid MP value", Color.DarkRed, Color.White);
            }
            _paramSet.Add("mp", SetMP);

            //Sets parallax level
            void SetParallax(string args)
            {
                if (int.TryParse(args, out int num) && num >= 0)
                {
                    Log.ForceMessage("<Asura>", "Changing parallax to level " + args, Color.Purple, Color.White);
                    _theVoid.SetParallax(num);
                    if (GameContainer.ActiveSectorID == GameContainer.SectorID.Void) _theVoid.SetFloor();
                }
                else Log.ForceMessage("<error>", "\"" + args + "\" is not a valid parallax value", Color.DarkRed, Color.White);
            }
            _paramSet.Add("parallax", SetParallax);

            //Set currently active seed for map generation
            void SetSeed(string args)
            {
                _theVoid.SetSeed(Utilities.GetAbsoluteSeed(MathD.urand, args));
                if (GameContainer.ActiveSectorID == GameContainer.SectorID.Void) _theVoid.SetFloor();
                Log.ForceMessage("<Asura>", "Setting active seed to " + args, Color.Purple, Color.White);
            }
            _paramSet.Add("seed", SetSeed);

            //Set speed multiplier for player movement
            void SetSpeed(string args)
            {
                if (float.TryParse(args, out float num) && num > 0)
                {
                    _theTown.SetSpeedMultiplier(num);
                    _theVoid.SetSpeedMultiplier(num);
                    Log.ForceMessage("<Asura>", "Setting speed multiplier to " + args, Color.Purple, Color.White);
                }
                else Log.ForceMessage("<error>", "\"" + args + "\" is not a valid speed value", Color.DarkRed, Color.White);
            }
            _paramSet.Add("speed", SetSpeed);

            //Set texture pack
            void SetTexturePack(string args)
            {
                var set = args.Split(' ');
                if (set.Length > 0 && set.Length < 4)
                {
                    foreach (var file in set)
                    {
                        if (!File.Exists(file))
                        {
                            Log.ForceMessage("<error>", "\"" + file + "\" is not a valid file", Color.DarkRed, Color.White);
                            return;
                        }
                    }
                    Log.ForceMessage("<Asura>", "Setting texture pack to " + args, Color.Purple, Color.White);
                    _theVoid.SetTexturePack(set);
                    if (GameContainer.ActiveSectorID == GameContainer.SectorID.Void) _theVoid.SetFloor();
                }
                else Log.ForceMessage("<error>", "Can only set texture pack using 1 to 3 files", Color.DarkRed, Color.White);
            }
            _paramSet.Add("textures", SetTexturePack);
        }
        private static void SetupHelpInfo()
        {
            _basicHelpString = _basicCommands.Aggregate("   Basic commands: ", (current, com) => current + (com.Key + " "));
            _elevatedHelpString = _elevatedCommands.Aggregate(_basicHelpString + "\nElevated Commands: ", (current, com) => current + (com.Key + " "));
            _basicHelpString += "\nUse \"/help <command>\" to see how a certain command works";
            _elevatedHelpString += "\nUse \"/help <command>\" to see how a certain command works";

            _helpInfo.Add("help",
                "\n/help           -> Display all available commands" +
                "\n/help [command] -> Display info about given command");

            _helpInfo.Add("exit",
                "\n/exit -> Exits the game. This operates the same as the title menu exit button");

            _helpInfo.Add("exportmap",
                "\n/exportall -> Save the current level set as PNG files to the maps directory");

            _helpInfo.Add("ascend",
                "\n/ascend          -> Set self to privileged mode" +
                "\n/ascend [player] -> Set given player to privileged mode");

            _helpInfo.Add("descend",
                "\n/descend          -> Set self to default mode" +
                "\n/descend [player] -> Set given player to default mode");

            _helpInfo.Add("return", 
                "\nreturn -> Exit the Void and return to the Outerworld" +
                "\nAny unsaved progress will be lost");

            _helpInfo.Add("save", 
                "\n/save -> Forces the current game to save");

            _helpInfo.Add("give", 
                "\n/give [type] [level] [quality] -> Give the current player an item of the given type, level, and quality" +
                "\n/give [type] [level] -> Give the current player an item of the given type and level" +
                "\n/give gold [amount] -> Give current player the given amount of gold" +
                "\n/give exp [amount] -> Give current player the given amount of exp" +
                "\nTypes are: weap, head, chest, hands, legs, feet, acc, potion" + 
                "\nQuality is 0 - 10");

            //Set-related help section

            _helpInfo.Add("set",
                "\n/set <param> [value] -> set an environment parameter." +
                _paramSet.Aggregate("\nPossible parameters: ", (current, com) => current + (com.Key + " ")) +
                "\nUse \"/help set <param>\" to see how a certain parameter works");

            _helpInfo.Add("set collision",
                "\n/set collision [value] -> Set own collisions on or off" +
                "\nValue must be \"on\" or \"off\"");

            _helpInfo.Add("set floor",
                "\n/set floor [value] -> Set current floor to given value" +
                "\nValue must be greater than zero. This will trigger a loading sequence if used in the Void.");

            _helpInfo.Add("set gentype",
                "\n/set gentype [value] -> Set active generation algorithm. Default is Standard2" +
                "\nValue must be standard1, standard2, messy. This will trigger a loading sequence if used in the Void.");

            _helpInfo.Add("set hp",
                "\n/set hp [value] -> Sets current player's HP to given value");

            _helpInfo.Add("set mp",
                "\n/set mp [value] -> Sets current player's MP to given value");

            _helpInfo.Add("set parallax",
                "\n/set parallax [value] -> Set parallax level" +
                "\nValue must be greater than or equal to zero. This will trigger a loading sequence if used in the Void.");

            _helpInfo.Add("set seed",
                "\n/set seed [value] -> Set current active seed based on the input value"+
                "\nThis will trigger a loading sequence if used in the Void.");

            _helpInfo.Add("set speed",
                "\n/set speed [value] -> Set own movement speed with the given multiplier" +
                "\nValue must be greater than zero. High values will behave erratically.");

            _helpInfo.Add("set textures", 
                "\n/set textures [value] -> Load a complete (ALL) texture pack" + 
                "\n/set textures [value] [value] -> Load a separate wall and floor texture pack (in that order)" +
                "\n/set textures [value] [value] [value] -> Load separate wall, floor, and other texture packs (in that order)" +
                "\nValue(s) must be files that exist. This will freeze the game for a bit. It will also trigger a loading sequence if used in the Void.");
        }
        private static void SetupCommands()
        {
            //Prints useful help information
            void Help(string args)
            {
                if (args.Length == 0)
                    Log.ForceMessage("<help>", _elevatedMode ? _elevatedHelpString : _basicHelpString, Color.DarkGreen, Color.White);
                else if (_helpInfo.TryGetValue(args, out string line))
                    Log.ForceMessage("<help>", line, Color.DarkGreen, Color.White);
            }
            _basicCommands.Add("help", Help);

            //Exits the game. Executes same function as title menu exit button
            void Exit(string args)
            {
                _game.ExitGame();
            }
            _basicCommands.Add("exit", Exit);

            //Sets player to privileged mode
            void AscendPlayer(string args)
            {
                if (args.Length != 0) Log.ForceMessage("<warning>", "Ignoring unexpected argument(s) \"" + args + "\"", Color.DarkOrange, Color.White);
                Log.ForceMessage("<Asura>", _elevatedMode ? "Already in privileged mode" : "Your privileges have been extended", Color.Purple, Color.White);
                _elevatedMode = true;
            }
            _basicCommands.Add("ascend", AscendPlayer);

            //Exports the current map set to a file
            void ExportAllMaps(string args)
            {
                if (GameContainer.ActiveSectorID != GameContainer.SectorID.Void)
                {
                    Log.ForceMessage("<error>", "Can only export maps from within the void", Color.DarkRed, Color.White);
                    return;
                }

                if (args.Length != 0) Log.ForceMessage("<warning>", "Ignoring unexpected argument(s) \"" + args + "\"", Color.DarkOrange, Color.White);
                Log.ForceMessage("<Asura>", "Outputting current map set to the maps directory", Color.Purple, Color.White);
                _theVoid.PrintMap(true);
            }
            _basicCommands.Add("exportmap", ExportAllMaps);

            //Shows the current seed value
            void Seed(string args)
            {
                if (args.Length != 0) Log.ForceMessage("<warning>", "Ignoring unexpected argument(s) \"" + args + "\"", Color.DarkOrange, Color.White);
                Log.ForceMessage("<Asura>", "Current seed is " + _theVoid.GetSeed(), Color.Purple, Color.White);
            }
            _basicCommands.Add("seed", Seed);

            //****************************************************************************************

            //Sets player to basic mode
            void DescendPlayer(string args)
            {
                if (args.Length != 0) Log.ForceMessage("<warning>", "Ignoring unexpected argument(s) \"" + args + "\"", Color.DarkOrange, Color.White);
                Log.ForceMessage("<Asura>", "Your privileges have been revoked", Color.Purple, Color.White);
                _elevatedMode = false;
            }
            _elevatedCommands.Add("descend", DescendPlayer);

            //Sets an environment parameter
            void Set(string args)
            {
                var index = args.IndexOf(' ');
                if (index <= 0)
                    Log.ForceMessage("<error>", "Set command requires a value. \"/set <param> [value]\"", Color.DarkRed, Color.White);
                else if (_paramSet.TryGetValue(args.Substring(0, index), out var param))
                    param.Invoke(args.Substring(index + 1));
                else
                {
                    var givenParam = args.Substring(0, index);
                    Log.ForceMessage("<error>", "No such parameter \"" + givenParam + "\". Use \"/help set\" to see available parameters", Color.DarkRed, Color.White);
                }
            }
            _elevatedCommands.Add("set", Set);

            //Return from void to outerworld
            void Return(string args)
            {
                if (GameContainer.ActiveSectorID != GameContainer.SectorID.Void)
                {
                    Log.ForceMessage("<error>", "Can only return from within the void", Color.DarkRed, Color.White);
                    return;
                }

                if (args.Length != 0) Log.ForceMessage("<warning>", "Ignoring unexpected argument(s) \"" + args + "\"", Color.DarkOrange, Color.White);
                _game.RequestSectorChange(GameContainer.SectorID.Outerworld);
            }
            _elevatedCommands.Add("return", Return);

            //Force the game to save
            void Save(string args)
            {
                if (GameContainer.ActiveSectorID == GameContainer.SectorID.MainMenu)
                {
                    Log.ForceMessage("<error>", "Cannot force save on title screen", Color.DarkRed, Color.White);
                    return;
                }

                if (args.Length != 0) Log.ForceMessage("<warning>", "Ignoring unexpected argument(s) \"" + args + "\"", Color.DarkOrange, Color.White);
                _game.Pack();
            }
            _elevatedCommands.Add("save", Save);

            void GiveItem(string args)
            {
                if (GameContainer.ActiveSectorID == GameContainer.SectorID.MainMenu)
                {
                    Log.ForceMessage("<error>", "Cannot change character values on title screen", Color.DarkRed, Color.White);
                    return;
                }

                var set = args.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                try
                {
                    switch (set[0])
                    {
                        case "gold":
                            if (long.TryParse(set[1], out long gold) && gold >= 0)
                            {
                                Log.ForceMessage("<Asura>", "Giving " + args + " gold to current character", Color.Purple, Color.White);
                                _game.Player.Inventory.TapGold(gold);
                            }
                            else throw new ArgumentOutOfRangeException();
                            break;
                        case "exp":
                            if (int.TryParse(set[1], out int exp) && exp >= 0)
                            {
                                Log.ForceMessage("<Asura>", "Giving " + args + " Exp to current character", Color.Purple, Color.White);
                                _game.Player.Inventory.GainExp(exp);
                            }
                            else throw new ArgumentOutOfRangeException();
                            break;
                        case "potion":
                            if (int.TryParse(set[1], out int level1) && int.TryParse(set[2], out int qual1) && level1 > 0 && qual1 >= 0 && qual1 <= 10)
                            {
                                var item = ItemLoader.GetUse(MathD.urand, level1, qual1);
                                var done = _game.Player.Inventory.TryAddItem(item);
                                if (done) Log.ForceMessage("<Asura>", "Giving " + item.Name + " to current character", Color.Purple, Color.White);
                                else Log.ForceMessage("<Asura>", "Inventory full", Color.Purple, Color.White);
                            }
                            else throw new ArgumentOutOfRangeException();
                            break;
                        default:
                            if (int.TryParse(set[1], out int level2) && int.TryParse(set[2], out int qual2) && level2 > 0 && qual2 >= 0 && qual2 <= 10)
                            {
                                var slot = (EquipItem.SlotType)Enum.Parse(typeof(EquipItem.SlotType), set[0].ToUpper());
                                var item = ItemLoader.GetEquipment(MathD.urand, level2, slot, qual2);
                                var done = _game.Player.Inventory.TryAddItem(item);
                                if (done) Log.ForceMessage("<Asura>", "Giving " + item.Name + " to current character", Color.Purple, Color.White);
                                else Log.ForceMessage("<Asura>", "Inventory full", Color.Purple, Color.White);
                            }
                            break;
                    }
                }
                catch (Exception)
                {
                    Log.ForceMessage("<error>", "\"" + args + "\" is not valid input for give command", Color.DarkRed, Color.White);
                }
                
            }
            _elevatedCommands.Add("give", GiveItem);

            //****************************************************************************************

            //Temp debug function for toggling extra debug display
            void Debug(string args)
            {
                if (args.Length != 0) Log.ForceMessage("<warning>", "Ignoring unexpected argument(s) \"" + args + "\"", Color.DarkOrange, Color.White);
                D.Bug = !D.Bug;
            }
            _elevatedCommands.Add("debug", Debug);

            //Temp debug function for executing arbitrary code at will
            void Trigger(string args)
            {
                Log.ForceMessage("<Asura>", "Activating debug commands", Color.Purple, Color.White);
                _game.GLOBAL_DEBUG_COMMAND(args);
            }
            _elevatedCommands.Add("trigger", Trigger);
        }
        public static void Ascend(GameContainer g, VoidSector u, OuterworldSector o)
        {
            _game = g;
            _theTown = o;
            _theVoid = u;
            _basicCommands = new Dictionary<string, Action<string>>();
            _elevatedCommands = new Dictionary<string, Action<string>>();
            _paramSet = new Dictionary<string, Action<string>>();
            _helpInfo = new Dictionary<string, string>();
            SetupCommands();
            SetupParams();
            SetupHelpInfo();
        }
    }
}
