using System;
using System.Collections.Generic;
using System.Drawing.Text;
using System.Linq;
using ExNihilo.Sectors;
using ExNihilo.Util.Graphics;
using Microsoft.Xna.Framework;

namespace ExNihilo.Systems
{
    public static class Asura
    {
        private static bool _elevatedMode=true;
        private static GameContainer _game;
        private static OverworldSector _theTown;
        private static UnderworldSector _theVoid;

        private static Dictionary<string, Action<ConsoleHandler, string>> _basicCommands;
        private static Dictionary<string, Action<ConsoleHandler, string>> _elevatedCommands;
        private static Dictionary<string, Action<ConsoleHandler, string>> _paramSet;
        private static string _basicHelpString, _elevatedHelpString;
        private static Dictionary<string, string> _helpInfo;

        //Called whenever a command-form message is issued
        public static void Handle(ConsoleHandler g, string com)
        {
            var command = com.Substring(1).ToLower(); //remove command starter /
            if (command.Length < 1) return; //if the command is blank don't do anything

            var sepIndex = command.IndexOf(' ');
            var baseCommand = sepIndex >= 0 ? command.Substring(0, sepIndex) : command;
            var extra = sepIndex >= 0 ? command.Substring(sepIndex+1) : "";
            if (_basicCommands.TryGetValue(baseCommand, out var basic))
            {
                basic.Invoke(g, extra);
            }
            else if (_elevatedCommands.TryGetValue(baseCommand, out var elevated))
            {
                if (_elevatedMode) elevated.Invoke(g, extra);
                else g.ForceMessage("<error>", "You do not have permission to use \"" + baseCommand + "\"", Color.DarkRed, Color.White);
            }
            else
            {
                g.ForceMessage("<error>", "No such command \"" + baseCommand + "\". Use /help to see available commands", Color.DarkRed, Color.White);
            }
        }

        //Initial setup
        private static void SetupParams()
        {
            //Set speed multiplier for player movement
            void SetSpeed(ConsoleHandler g, string args)
            {
                if (float.TryParse(args, out float num) && num > 0)
                {
                    _theTown.SetSpeedMultiplier(num);
                    _theVoid.SetSpeedMultiplier(num);
                    g.ForceMessage("<Asura>", "Setting speed multiplier to " + args, Color.Purple, Color.White);
                }
                else g.ForceMessage("<error>", "\"" + args + "\" is not a valid speed value", Color.DarkRed, Color.White);
            }
            _paramSet.Add("speed", SetSpeed);

            //Set currently active seed for map generation
            void SetSeed(ConsoleHandler g, string args)
            {
                _theVoid.SetSeed(args.GetHashCode());
                g.ForceMessage("<Asura>", "Setting active seed to " + args, Color.Purple, Color.White);
            }
            _paramSet.Add("seed", SetSeed);

            //Changes to the given floor
            void SetFloor(ConsoleHandler g, string args)
            {
                if (_game.ActiveSectorID != GameContainer.SectorID.Underworld)
                {
                    g.ForceMessage("<error>", "Can only change floors from within the void", Color.DarkRed, Color.White);
                    return;
                }

                if (int.TryParse(args, out int num) && num > 0)
                {
                    g.ForceMessage("<Asura>", "Swapping to floor " + args, Color.Purple, Color.White);
                    _theVoid.SetFloor(num);
                }
                else g.ForceMessage("<error>", "\"" + args + "\" is not a valid floor value", Color.DarkRed, Color.White);
            }
            _paramSet.Add("floor", SetFloor);

            //Sets parallax level
            void SetParallax(ConsoleHandler g, string args)
            {
                if (int.TryParse(args, out int num) && num >= 0)
                {
                    g.ForceMessage("<Asura>", "Changing parallax to level " + args, Color.Purple, Color.White);
                    _theVoid.SetParallax(num);
                }
                else g.ForceMessage("<error>", "\"" + args + "\" is not a valid parallax value", Color.DarkRed, Color.White);
            }
            _paramSet.Add("parallax", SetParallax);

            //Sets player collisions on or off
            void SetCollisions(ConsoleHandler g, string args)
            {
                if (args == "on")
                {
                    _theVoid.ToggleCollisions(true);
                    _theTown.ToggleCollisions(true);
                }
                else if (args == "off")
                {
                    _theVoid.ToggleCollisions(false);
                    _theTown.ToggleCollisions(false);
                }
                else
                {
                    g.ForceMessage("<error>", "\"" + args + "\" is not valid. Value must be \"on\" or \"off\"", Color.DarkRed, Color.White);
                }
            }
            _paramSet.Add("collisions", SetCollisions);
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
                "\n/exportmap -> Save the current level as a PNG file to the maps directory");

            _helpInfo.Add("ascend",
                "\n/ascend          -> Set self to privileged mode" +
                "\n/ascend [player] -> Set given player to privileged mode");

            _helpInfo.Add("descend",
                "\n/descend          -> Set self to default mode" +
                "\n/descend [player] -> Set given player to default mode");

            _helpInfo.Add("set",
                "\n/set <param> [value] -> set an environment parameter." +
                "\nPossible parameters: Collision, Floor, Parallax, Seed, Speed" +
                "\nUse \"/help set <param>\" to see how a certain parameter works");

            _helpInfo.Add("set speed",
                "\n/set speed [multiplier] -> Set own movement speed with the given multiplier" +
                "\nMultiplier must be greater than zero. High values will behave erratically.");

            _helpInfo.Add("set seed",
                "\n/set seed [value] -> Set current active seed based on the input value");

            _helpInfo.Add("set floor",
                "\n/set floor [value] -> Set current floor to given value" +
                "\nValue must be greater than zero. This will trigger a loading sequence.");

            _helpInfo.Add("set collision",
                "\n/set collision [value] -> Set own collisions on or off" +
                "\nValue must be \"on\" or \"off\"");

            _helpInfo.Add("set parallax",
                "\n/set parallax [value] -> Set parallax level" +
                "\nValue must be greater than or equal to zero");
        }
        private static void SetupCommands()
        {
            //Prints useful help information
            void Help(ConsoleHandler g, string args)
            {
                if (args.Length == 0)
                    g.ForceMessage("<help>", _elevatedMode ? _elevatedHelpString : _basicHelpString, Color.ForestGreen, Color.White);
                else if (_helpInfo.TryGetValue(args, out string line))
                    g.ForceMessage("<help>", line, Color.ForestGreen, Color.White);
            }
            _basicCommands.Add("help", Help);

            //Exits the game. Executes same function as title menu exit button
            void Exit(ConsoleHandler g, string args)
            {
                _game.ExitGame();
            }
            _basicCommands.Add("exit", Exit);

            //Sets player to privileged mode
            void AscendPlayer(ConsoleHandler g, string args)
            {
                if (args.Length != 0) g.ForceMessage("<warning>", "Ignoring unexpected argument(s) \"" + args + "\"", Color.DarkOrange, Color.White);
                g.ForceMessage("<Asura>", _elevatedMode ? "Already in privileged mode" : "Your privileges have been extended", Color.Purple, Color.White);
                _elevatedMode = true;
            }
            _basicCommands.Add("ascend", AscendPlayer);
            
            //Exports the current map to a file
            void ExportMap(ConsoleHandler g, string args)
            {
                if (_game.ActiveSectorID != GameContainer.SectorID.Underworld)
                {
                    g.ForceMessage("<error>", "Can only export map from within the void", Color.DarkRed, Color.White);
                    return;
                }

                if (args.Length != 0) g.ForceMessage("<warning>", "Ignoring unexpected argument(s) \"" + args + "\"", Color.DarkOrange, Color.White);
                _theVoid.PrintMap();
                g.ForceMessage("<Asura>", "Outputting current map to the maps directory", Color.Purple, Color.White);
            }
            _basicCommands.Add("exportmap", ExportMap);

            //Shows the current seed value
            void Seed(ConsoleHandler g, string args)
            {
                if (args.Length != 0) g.ForceMessage("<warning>", "Ignoring unexpected argument(s) \"" + args + "\"", Color.DarkOrange, Color.White);
                g.ForceMessage("<Asura>", "Current seed is " + _theVoid.GetSeed(), Color.Purple, Color.White);
            }
            _basicCommands.Add("seed", Seed);

            //****************************************************************************************

            //Sets player to basic mode
            void DescendPlayer(ConsoleHandler g, string args)
            {
                if (args.Length != 0) g.ForceMessage("<warning>", "Ignoring unexpected argument(s) \"" + args + "\"", Color.DarkOrange, Color.White);
                g.ForceMessage("<Asura>", "Your privileges have been revoked", Color.Purple, Color.White);
                _elevatedMode = false;
            }
            _elevatedCommands.Add("descend", DescendPlayer);

            //Sets an environment parameter
            void Set(ConsoleHandler g, string args)
            {
                var index = args.IndexOf(' ');
                if (index <= 0)
                    g.ForceMessage("<error>", "Set command requires a value. \"/set <param> [value]\"", Color.DarkRed, Color.White);
                else if (_paramSet.TryGetValue(args.Substring(0, index), out var param))
                    param.Invoke(g, args.Substring(index + 1));
                else
                {
                    var givenParam = args.Substring(index);
                    g.ForceMessage("<error>", "No such parameter \"" + givenParam + "\". Use \"/help set\" to see available parameters", Color.DarkRed, Color.White);
                }
            }
            _elevatedCommands.Add("set", Set);

            //Temp debug function for toggling extra debug display
            void Debug(ConsoleHandler g, string args)
            {
                if (args.Length != 0) g.ForceMessage("<warning>", "Ignoring unexpected argument(s) \"" + args + "\"", Color.DarkOrange, Color.White);
                D.Bug = !D.Bug;
            }
            _elevatedCommands.Add("debug", Debug);

            //Temp debug function for executing arbitrary code at will
            void Trigger(ConsoleHandler g, string args)
            {
                _game.GLOBAL_DEBUG_COMMAND(args);
                g.ForceMessage("<Asura>", "Activating debug commands", Color.Purple, Color.White);
            }
            _elevatedCommands.Add("trigger", Trigger);
        }
        public static void Ascend(GameContainer g, UnderworldSector u, OverworldSector o)
        {
            _game = g;
            _theTown = o;
            _theVoid = u;
            _basicCommands = new Dictionary<string, Action<ConsoleHandler, string>>();
            _elevatedCommands = new Dictionary<string, Action<ConsoleHandler, string>>();
            _paramSet = new Dictionary<string, Action<ConsoleHandler, string>>();
            _helpInfo = new Dictionary<string, string>();
            SetupCommands();
            SetupHelpInfo();
            SetupParams();
        }
    }
}
