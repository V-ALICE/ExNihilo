﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ExNihilo.Sectors;
using ExNihilo.Systems.Backend;
using ExNihilo.Systems.Backend.Network;
using ExNihilo.Systems.Bases;
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

        private static Dictionary<string, Action<string>> _basicCommands;
        private static Dictionary<string, Action<string>> _elevatedCommands;
        private static Dictionary<string, Action<string>> _paramSet;
        private static string _basicHelpString, _elevatedHelpString;
        private static Dictionary<string, string> _helpInfo;
        private static string[] _giveOptions =
        {
            "gold", "exp",
            EquipItem.SlotType.WEAP.ToString().ToLower(),
            EquipItem.SlotType.HEAD.ToString().ToLower(),
            EquipItem.SlotType.CHEST.ToString().ToLower(),
            EquipItem.SlotType.HANDS.ToString().ToLower(),
            EquipItem.SlotType.LEGS.ToString().ToLower(),
            EquipItem.SlotType.FEET.ToString().ToLower(),
            EquipItem.SlotType.ACC.ToString().ToLower()
        };

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
                else SystemConsole.ForceMessage("<error>", "You do not have permission to use \"" + baseCommand + "\"", Color.DarkRed, Color.White);
            }
            else
            {
                SystemConsole.ForceMessage("<error>", "No such command \"" + baseCommand + "\". Use /help to see available commands", Color.DarkRed, Color.White);
            }
        }
        //Called when tab is pressed
        public static string GetSuggestion(string input)
        {
            string GetOption(string start, params Dictionary<string, Action<string>>[] sets)
            {
                var option = "";
                foreach (var s in sets)
                {
                    if (s is null) continue;
                    foreach (var com in s)
                    {
                        if (com.Key.StartsWith(start))
                        {
                            if (option.Length != 0) return ""; //Multiple matches
                            option = com.Key;
                        }
                    }
                }
                return option;
            }

            string GetOptionSimple(string start, params string[] opts)
            {
                var option = "";
                foreach (var com in opts)
                {
                    if (com.StartsWith(start))
                    {
                        if (option.Length != 0) return ""; //Multiple matches
                        option = com;
                    }
                }
                return option;
            }

            if (input.Length == 0 || input[0] != '/') return input;

            var set = input.Substring(1).Split(' ');
            if (set[0] == "set")
            {
                if (set.Length != 2) return input;
                
                //tab complete set param
                var option = GetOption(set[1], _paramSet);
                if (option.Length != 0) return "/set " + option;
            }
            else if (set[0] == "help")
            {
                if (set.Length != 2) return input;

                //tab complete help command
                var option = GetOption(set[1], _basicCommands, _elevatedMode ? _elevatedCommands : null);
                if (option.Length != 0) return "/help " + option;
            }
            else if (set[0] == "give")
            {
                if (set.Length != 2) return input;

                //tab complete give option
                var option = GetOptionSimple(set[1], _giveOptions);
                if (option.Length != 0) return "/give " + option;
            }
            else
            {
                if (set.Length != 1) return input;
                
                //tab complete command
                var option = GetOption(set[0], _basicCommands, _elevatedMode ? _elevatedCommands : null);
                if (option.Length != 0) return "/" + option;
            }

            return input;
        }

        //Initial setup
        private static void SetupParams()
        {
            //Sets player collisions on or off
            void SetCollisions(string args)
            {
                if (args == "on")
                {
                    SystemConsole.ForceMessage("<Asura>", "Enabling collisions", Color.Purple, Color.White);
                    _theVoid.ToggleCollisions(true);
                    _theTown.ToggleCollisions(true);
                }
                else if (args == "off")
                {
                    SystemConsole.ForceMessage("<Asura>", "Disabling collisions", Color.Purple, Color.White);
                    _theVoid.ToggleCollisions(false);
                    _theTown.ToggleCollisions(false);
                }
                else
                {
                    SystemConsole.ForceMessage("<error>", "\"" + args + "\" is not valid. Value must be \"on\" or \"off\"", Color.DarkRed, Color.White);
                }
            }
            _paramSet.Add("collision", SetCollisions);

            //Changes to the given floor
            void SetFloor(string args)
            {
                if (GameContainer.ActiveSectorID != GameContainer.SectorID.Void)
                {
                    SystemConsole.ForceMessage("<error>", "Can only change floors from within the void", Color.DarkRed, Color.White);
                    return;
                }

                if (NetworkManager.Active && !NetworkManager.Hosting)
                {
                    SystemConsole.ForceMessage("<error>", "Only host can force floor change", Color.DarkRed, Color.White);
                    return;
                }

                if (int.TryParse(args, out int num) && num > 0)
                {
                    SystemConsole.ForceMessage("<Asura>", "Swapping to floor " + args, Color.Purple, Color.White);
                    _game.PushVoid(VoidSector.Seed, MathD.urand.Next(), num);
                }
                else SystemConsole.ForceMessage("<error>", "\"" + args + "\" is not a valid floor value", Color.DarkRed, Color.White);
            }
            _paramSet.Add("floor", SetFloor);

            //Sets level generation type
            void SetGenType(string args)
            {
                if (_game.VoidIsActive)
                {
                    SystemConsole.ForceMessage("<warning>", "Generation change will apply next time the floor swaps", Color.DarkOrange, Color.White);
                }
                switch (args)
                {
                    case "standard1":
                        SystemConsole.ForceMessage("<Asura>", "Changing generation algorithm to standard1", Color.Purple, Color.White);
                        _theVoid.SetGenType(MapGenerator.Type.Standard1);
                        break;
                    case "standard2":
                        SystemConsole.ForceMessage("<Asura>", "Changing generation algorithm to standard2", Color.Purple, Color.White);
                        _theVoid.SetGenType(MapGenerator.Type.Standard2);
                        break;
                    case "messy":
                        SystemConsole.ForceMessage("<Asura>", "Changing generation algorithm to messy", Color.Purple, Color.White);
                        _theVoid.SetGenType(MapGenerator.Type.MessyBoxes);
                        break;
                    default:
                        SystemConsole.ForceMessage("<error>", "\"" + args + "\" is not a valid gentype value", Color.DarkRed, Color.White);
                        break;
                }
            }
            _paramSet.Add("gentype", SetGenType);

            //Sets HP to given value
            void SetHP(string args)
            {
                if (_game.Player is null)
                {
                    SystemConsole.ForceMessage("<error>", "No player loaded", Color.DarkRed, Color.White);
                    return;
                }

                if (int.TryParse(args, out int num) && num >= 0)
                {
                    SystemConsole.ForceMessage("<Asura>", "Setting current player HP to " + args, Color.Purple, Color.White);
                    _game.Player.Inventory.SetHPMP(num);
                }
                else SystemConsole.ForceMessage("<error>", "\"" + args + "\" is not a valid HP value", Color.DarkRed, Color.White);
            }
            _paramSet.Add("hp", SetHP);

            //Sets MP to given value
            void SetMP(string args)
            {
                if (_game.Player is null)
                {
                    SystemConsole.ForceMessage("<error>", "No player has been loaded", Color.DarkRed, Color.White);
                    return;
                }

                if (int.TryParse(args, out int num) && num >= 0)
                {
                    SystemConsole.ForceMessage("<Asura>", "Setting current player MP to " + args, Color.Purple, Color.White);
                    _game.Player.Inventory.SetHPMP(-1, num);
                }
                else SystemConsole.ForceMessage("<error>", "\"" + args + "\" is not a valid MP value", Color.DarkRed, Color.White);
            }
            _paramSet.Add("mp", SetMP);

            //Sets parallax level
            void SetParallax(string args)
            {
                if (int.TryParse(args, out int num) && num >= 0)
                {
                    if (_game.VoidIsActive)
                    {
                        SystemConsole.ForceMessage("<warning>", "Parallax change will apply next time the floor swaps", Color.DarkOrange, Color.White);
                    }
                    SystemConsole.ForceMessage("<Asura>", "Changing parallax to level " + args, Color.Purple, Color.White);
                    _theVoid.SetParallax(num);
                }
                else SystemConsole.ForceMessage("<error>", "\"" + args + "\" is not a valid parallax value", Color.DarkRed, Color.White);
            }
            _paramSet.Add("parallax", SetParallax);

            //Set currently active seed for map generation
            void SetSeed(string args)
            {
                if (_game.VoidIsActive)
                {
                    SystemConsole.ForceMessage("<warning>", "Seed change will apply next time the floor swaps", Color.DarkOrange, Color.White);
                }

                VoidSector.Seed = Utilities.GetAbsoluteSeed(MathD.urand, args);
                SystemConsole.ForceMessage("<Asura>", "Setting active seed to " + args, Color.Purple, Color.White);
            }
            _paramSet.Add("seed", SetSeed);

            //Set speed multiplier for player movement
            void SetSpeed(string args)
            {
                if (float.TryParse(args, out float num) && num > 0)
                {
                    _theTown.SetSpeedMultiplier(num);
                    _theVoid.SetSpeedMultiplier(num);
                    SystemConsole.ForceMessage("<Asura>", "Setting speed multiplier to " + args, Color.Purple, Color.White);
                }
                else SystemConsole.ForceMessage("<error>", "\"" + args + "\" is not a valid speed value", Color.DarkRed, Color.White);
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
                        if (!File.Exists(Environment.CurrentDirectory + "/Content/TexturePacks/" + file))
                        {
                            SystemConsole.ForceMessage("<error>", "\"" + file + "\" is not a valid file", Color.DarkRed, Color.White);
                            return;
                        }
                    }
                    if (_game.VoidIsActive)
                    {
                        SystemConsole.ForceMessage("<warning>", "Texture pack change will apply next time the floor swaps", Color.DarkOrange, Color.White);
                    }
                    SystemConsole.ForceMessage("<Asura>", "Setting texture pack to " + args, Color.Purple, Color.White);
                    _theVoid.SetTexturePack(set);
                }
                else SystemConsole.ForceMessage("<error>", "Can only set texture pack using 1 to 3 files", Color.DarkRed, Color.White);
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

            _helpInfo.Add("clear",
                "\n/clear -> Clears console window");

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

            _helpInfo.Add("randomseed", 
                "\nrandomseed -> Sets the current seed randomly");

            _helpInfo.Add("give", 
                "/give [type] [level] [quality] -> Give the current player an item of the given type, level, and quality" +
                "\n/give [type] [level]           -> Give the current player an item of the given type and level" +
                "\n/give gold [amount]            -> Give current player the given amount of gold" +
                "\n/give exp [amount]             -> Give current player the given amount of exp" +
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
                "\nValue must be greater than zero");

            _helpInfo.Add("set gentype",
                "\n/set gentype [value] -> Set active generation algorithm. Default is Standard2" +
                "\nValue must be standard1, standard2, messy");

            _helpInfo.Add("set hp",
                "\n/set hp [value] -> Sets current player's HP to given value");

            _helpInfo.Add("set mp",
                "\n/set mp [value] -> Sets current player's MP to given value");

            _helpInfo.Add("set parallax",
                "\n/set parallax [value] -> Set parallax level" +
                "\nValue must be greater than or equal to zero");

            _helpInfo.Add("set seed",
                "\n/set seed [value] -> Set current active seed based on the input value");

            _helpInfo.Add("set speed",
                "\n/set speed [value] -> Set own movement speed with the given multiplier" +
                "\nValue must be greater than zero. High values will behave erratically.");

            _helpInfo.Add("set textures", 
                "\n/set textures [value]                 -> Load a complete (ALL) texture pack" + 
                "\n/set textures [value] [value]         -> Load a separate wall and floor texture pack (in that order)" +
                "\n/set textures [value] [value] [value] -> Load separate wall, floor, and other texture packs (in that order)" +
                "\nValue(s) must be files that exist. This may freeze the game for a short time");
        }
        private static void SetupCommands()
        {
            //Prints useful help information
            void Help(string args)
            {
                if (args.Length == 0)
                    SystemConsole.ForceMessage("<Asura>", _elevatedMode ? _elevatedHelpString : _basicHelpString, Color.Purple, Color.White);
                else if (_helpInfo.TryGetValue(args, out string line))
                    SystemConsole.ForceMessage("<Asura>", line, Color.Purple, Color.White);
            }
            _basicCommands.Add("help", Help);

            //Clears console window
            void Clear(string args)
            {
                if (args.Length != 0) SystemConsole.ForceMessage("<warning>", "Ignoring unexpected argument(s) \"" + args + "\"", Color.DarkOrange, Color.White);
                SystemConsole.ClearConsole();
            }
            _basicCommands.Add("clear", Clear);

            //Exits the game. Executes same function as title menu exit button
            void Exit(string args)
            {
                _game.ExitGame();
            }
            _basicCommands.Add("exit", Exit);

            //Sets player to privileged mode
            void AscendPlayer(string args)
            {
                if (args.Length != 0) SystemConsole.ForceMessage("<warning>", "Ignoring unexpected argument(s) \"" + args + "\"", Color.DarkOrange, Color.White);
                SystemConsole.ForceMessage("<Asura>", _elevatedMode ? "Already in privileged mode" : "Your privileges have been extended", Color.Purple, Color.White);
                _elevatedMode = true;
            }
            _basicCommands.Add("ascend", AscendPlayer);

            //Exports the current map set to a file
            void ExportAllMaps(string args)
            {
                if (!_game.VoidIsActive)
                {
                    SystemConsole.ForceMessage("<error>", "No level loaded", Color.DarkRed, Color.White);
                    return;
                }

                if (args.Length != 0) SystemConsole.ForceMessage("<warning>", "Ignoring unexpected argument(s) \"" + args + "\"", Color.DarkOrange, Color.White);
                SystemConsole.ForceMessage("<Asura>", "Outputting current map set to the maps directory", Color.Purple, Color.White);
                _theVoid.PrintMap(true);
            }
            _basicCommands.Add("exportmap", ExportAllMaps);

            //Shows the current seed value
            void Seed(string args)
            {
                if (args.Length != 0) SystemConsole.ForceMessage("<warning>", "Ignoring unexpected argument(s) \"" + args + "\"", Color.DarkOrange, Color.White);
                SystemConsole.ForceMessage("<Asura>", "Current seed is " + VoidSector.Seed, Color.Purple, Color.White);
            }
            _basicCommands.Add("seed", Seed);

            //Sets a random seed
            void RandomSeed(string args)
            {
                if (args.Length != 0) SystemConsole.ForceMessage("<warning>", "Ignoring unexpected argument(s) \"" + args + "\"", Color.DarkOrange, Color.White);
                if (_game.VoidIsActive)
                {
                    SystemConsole.ForceMessage("<warning>", "Generation change will apply next time the floor swaps", Color.DarkOrange, Color.White);
                }
                VoidSector.Seed = MathD.urand.Next();
                SystemConsole.ForceMessage("<Asura>", "Setting random seed", Color.Purple, Color.White);
            }
            _basicCommands.Add("randomseed", RandomSeed);

            //****************************************************************************************

            //Sets player to basic mode
            void DescendPlayer(string args)
            {
                if (args.Length != 0) SystemConsole.ForceMessage("<warning>", "Ignoring unexpected argument(s) \"" + args + "\"", Color.DarkOrange, Color.White);
                SystemConsole.ForceMessage("<Asura>", "Your privileges have been revoked", Color.Purple, Color.White);
                _elevatedMode = false;
            }
            _elevatedCommands.Add("descend", DescendPlayer);

            //Sets an environment parameter
            void Set(string args)
            {
                var index = args.IndexOf(' ');
                if (index <= 0)
                    SystemConsole.ForceMessage("<error>", "Set command requires a value. \"/set <param> [value]\"", Color.DarkRed, Color.White);
                else if (_paramSet.TryGetValue(args.Substring(0, index), out var param))
                    param.Invoke(args.Substring(index + 1));
                else
                {
                    var givenParam = args.Substring(0, index);
                    SystemConsole.ForceMessage("<error>", "No such parameter \"" + givenParam + "\". Use \"/help set\" to see available parameters", Color.DarkRed, Color.White);
                }
            }
            _elevatedCommands.Add("set", Set);

            //Return from void to outerworld
            void Return(string args)
            {
                if (!_game.VoidIsActive)
                {
                    SystemConsole.ForceMessage("<error>", "Nowhere to return from", Color.DarkRed, Color.White);
                    return;
                }
                if (NetworkManager.Active && !NetworkManager.Hosting)
                {
                    SystemConsole.ForceMessage("<error>", "Only host can force return", Color.DarkRed, Color.White);
                    return;
                }

                if (args.Length != 0) SystemConsole.ForceMessage("<warning>", "Ignoring unexpected argument(s) \"" + args + "\"", Color.DarkOrange, Color.White);
                _game.ExitVoid();
            }
            _elevatedCommands.Add("return", Return);

            //Force the game to save
            void Save(string args)
            {
                if (_game.Player is null)
                {
                    SystemConsole.ForceMessage("<error>", "No save loaded", Color.DarkRed, Color.White);
                    return;
                }

                if (args.Length != 0) SystemConsole.ForceMessage("<warning>", "Ignoring unexpected argument(s) \"" + args + "\"", Color.DarkOrange, Color.White);
                _game.Pack();
            }
            _elevatedCommands.Add("save", Save);

            void GiveItem(string args)
            {
                if (_game.Player is null)
                {
                    SystemConsole.ForceMessage("<error>", "No player loaded", Color.DarkRed, Color.White);
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
                                SystemConsole.ForceMessage("<Asura>", "Giving " + args + " gold to current character", Color.Purple, Color.White);
                                _game.Player.Inventory.TapGold(gold);
                            }
                            else throw new ArgumentOutOfRangeException();
                            break;
                        case "exp":
                            if (int.TryParse(set[1], out int exp) && exp >= 0)
                            {
                                SystemConsole.ForceMessage("<Asura>", "Giving " + args + " Exp to current character", Color.Purple, Color.White);
                                _game.Player.Inventory.GainExp(exp);
                            }
                            else throw new ArgumentOutOfRangeException();
                            break;
                        default:
                            if (int.TryParse(set[1], out int level) && level > 0)
                            {
                                var qual = -1;
                                if (set.Length > 2 && int.TryParse(set[2], out int q) && q >= 0) qual = q;
                                ItemInstance item;
                                if (set[0] == "potion") item = ItemLoader.GetUse(MathD.urand, level, qual);
                                else
                                {
                                    var slot = (EquipItem.SlotType)Enum.Parse(typeof(EquipItem.SlotType), set[0].ToUpper());
                                    item = ItemLoader.GetEquipment(MathD.urand, level, slot, qual);
                                }
                                var done = _game.Player.Inventory.TryAddItem(item);
                                if (done) SystemConsole.ForceMessage("<Asura>", "Giving " + item.Name + " to current character", Color.Purple, Color.White);
                                else SystemConsole.ForceMessage("<Asura>", "Inventory full", Color.Purple, Color.White);
                            }
                            break;
                    }
                }
                catch (Exception)
                {
                    SystemConsole.ForceMessage("<error>", "\"" + args + "\" is not valid input for give command", Color.DarkRed, Color.White);
                }
                
            }
            _elevatedCommands.Add("give", GiveItem);

            //****************************************************************************************

            //Temp debug function for toggling extra debug display
            void Debug(string args)
            {
                D.Bug = !D.Bug;
            }
            _elevatedCommands.Add("debug", Debug);

            //Temp debug function for executing arbitrary code at will
            void Trigger(string args)
            {
                SystemConsole.ForceMessage("<Asura>", "Activating debug commands", Color.Purple, Color.White);
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
