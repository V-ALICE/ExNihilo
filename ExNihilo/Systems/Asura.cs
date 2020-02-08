using System;
using ExNihilo.Sectors;
using ExNihilo.Util.Graphics;
using Microsoft.Xna.Framework;

namespace ExNihilo.Systems
{
    public static class Asura
    {
        private static bool _cheatyMode;
        private static GameContainer _game;
        private static OverworldSector _theTown;
        private static UnderworldSector _theVoid;

        private static void PostHelp(ConsoleHandler g, string com)
        {
            switch (com)
            {
                case "help":
                    g.ForceMessage("<help>",
                        "\n/help           -> Display all available commands" +
                        "\n/help [command] -> Display info about given command", Color.DarkOrange, Color.White);
                    break;
                case "exportmap":
                    g.ForceMessage("<help>",
                        "\n/exportmap [filename] -> Save the current level as a PNG file", Color.DarkOrange, Color.White);
                    break;
                case "ascend":
                    g.ForceMessage("<help>",
                        "\n/ascend          -> Set self to privileged mode" +
                        "\n/ascend [player] -> Set given player to privileged mode", Color.DarkOrange, Color.White);
                    break;
                case "descend":
                    g.ForceMessage("<help>",
                        "\n/descend          -> Set self to default mode" +
                        "\n/descend [player] -> Set given player to default mode", Color.DarkOrange, Color.White);
                    break;
                case "set":
                    g.ForceMessage("<help>",
                        "\n/set <param> [value] -> set an environment parameter." + 
                        "\nPossible parameters: Floor, NoClip, Seed, Speed" +
                        "\nUse \"/help set <param>\" to see how a certain parameter works", Color.DarkOrange, Color.White);
                    break;
                case "set speed":
                    g.ForceMessage("<help>",
                        "\n/set speed [multiplier] -> Set own movement speed with the given multiplier" +
                        "\nMultiplier must be greater than zero. High values will behave erratically.", Color.DarkOrange, Color.White);
                    break;
                case "set seed":
                    g.ForceMessage("<help>",
                        "\n/set seed [value] -> Set current active seed to the given numeric value", Color.DarkOrange, Color.White);
                    break;
                case "set floor":
                    g.ForceMessage("<help>",
                        "\n/set floor [value] -> Set current floor to given value" +
                        "\nValue must be greater than zero. This will trigger a loading sequence.", Color.DarkOrange, Color.White);
                    break;
                case "set noclip":
                    g.ForceMessage("<help>",
                        "\n/set noclip [value] -> Set own collisions on or off" +
                        "\nValue must be \"on\" or \"off\"", Color.DarkOrange, Color.White);
                    break;
                default:
                    g.ForceMessage("<error>", "No such command \"" + com + "\". Use /help to see available commands", Color.DarkRed, Color.White);
                    break;
            }
        }
        private static void PostHelp(ConsoleHandler g)
        {
            if (_cheatyMode)
            {
                g.ForceMessage("<help>",
                    "Possible commands: help, ascend, descend, exportmap, noclip, set" +
                    "\nUse \"/help <command>\" to see how a certain command works", Color.DarkOrange, Color.White);
            }
            else
            {
                g.ForceMessage("<help>",
                    "Possible commands: help, ascend, exportmap" +
                    "\nUse \"/help <command>\" to see how a certain command works", Color.DarkOrange, Color.White);
            }
        }

        private static void SetParameter(ConsoleHandler g, string command)
        {
            if (command.IndexOf(' ') <= 0)
            {
                g.ForceMessage("<error>", "Set command requires a value. \"/set <param> [value]\"", Color.DarkRed, Color.White);
            }
            else if (command.StartsWith("speed "))
            {
                var extra = command.Substring(6);
                if (float.TryParse(extra, out float num) && num > 0)
                {
                    _theTown.SetSpeedMultiplier(num);
                    _theVoid.SetSpeedMultiplier(num);
                    g.ForceMessage("<Asura>", "Setting speed multiplier to " + extra, Color.Purple, Color.White);
                }
                else g.ForceMessage("<error>", "\"" + extra + "\" is not a valid speed value", Color.DarkRed, Color.White);
            }
            else if (command.StartsWith("seed "))
            {
                var extra = command.Substring(5);
                if (int.TryParse(extra, out int num))
                {
                    _theVoid.SetSeed(num);
                    g.ForceMessage("<Asura>", "Setting active seed to " + extra, Color.Purple, Color.White);
                }
                else g.ForceMessage("<error>", "\"" + extra + "\" is not a valid seed value", Color.DarkRed, Color.White);
            }
            else if (command.StartsWith("floor "))
            {
                if (_game.ActiveSectorID != GameContainer.SectorID.Underworld)
                {
                    g.ForceMessage("<error>", "Can only change floors from within the void", Color.Purple, Color.White);
                    return;
                }

                var extra = command.Substring(6);
                if (int.TryParse(extra, out int num) && num > 0)
                {
                    g.ForceMessage("<Asura>", "Swapping to floor " + extra, Color.Purple, Color.White);
                    _theVoid.SetFloor(num);
                }
                else g.ForceMessage("<error>", "\"" + extra + "\" is not a valid floor value", Color.DarkRed, Color.White);
            }
            else if (command.StartsWith("noclip "))
            {
                var extra = command.Substring(7);
                switch (extra)
                {
                    case "on":
                        _theVoid.ToggleCollisions(true);
                        _theTown.ToggleCollisions(true);
                        break;
                    case "off":
                        _theVoid.ToggleCollisions(false);
                        _theTown.ToggleCollisions(false);
                        break;
                    default:
                        g.ForceMessage("<error>", "\"" + extra + "\" is not valid. Value must be \"on\" or \"off\"", Color.DarkRed, Color.White);
                        break;
                }
            }
            else
            {
                var param = command.Substring(command.IndexOf(' '));
                g.ForceMessage("<error>", "No such parameter \"" + param + "\". Use \"/help set\" to see available parameters", Color.DarkRed, Color.White);
            }
        }

        private static void HandleNoArgCommands(ConsoleHandler g, string command)
        {
            if (command.Equals("help"))
            {
                PostHelp(g);
            }
            else if (command.Equals("ascend"))
            {
                _cheatyMode = true;
                g.ForceMessage("<Asura>", "Your privileges have been extended", Color.Purple, Color.White);
            }
            else if (command.Equals("descend"))
            {
                _cheatyMode = false;
                g.ForceMessage("<Asura>", "Your privileges have been revoked", Color.Purple, Color.White);
            }
            else if (!_cheatyMode)
            {
                g.ForceMessage("<error>", "You do not have permission to use this command, or it does not exist", Color.DarkRed, Color.White);
            }
            else
            {
                switch (command)
                {
                    case "hello":
                        g.ForceMessage("<Asura>", "salutations", Color.Purple, Color.White);
                        break;
                    default:
                        g.ForceMessage("<error>", "No such command \"" + command + "\". Use /help to see available commands", Color.DarkRed, Color.White);
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
            else if (!_cheatyMode)
            {
                g.ForceMessage("<error>", "You do not have permission to use this command, or it does not exist", Color.DarkRed, Color.White);
            }
            else if (command.StartsWith("trigger "))
            {
                _game.GLOBAL_DEBUG_COMMAND(command.Substring(8));
                g.ForceMessage("<Asura>", "Activating debug commands", Color.Purple, Color.White);
            }
            else if (command.StartsWith("echo "))
            {
                var extra = command.Substring(5);
                g.ForceMessage("<Asura>", extra, Color.Purple, Color.White);
            }
            else if (command.StartsWith("exportmap "))
            {
                var extra = command.Substring(10);
                try
                {
                    _theVoid.PrintMap(extra);
                    g.ForceMessage("<Asura>", "Outputting current map to maps/" + extra, Color.Purple, Color.White);
                }
                catch (Exception e)
                {
                    g.ForceMessage("<error>", e.Message, Color.DarkRed, Color.White);
                }
            }
            else if (command.StartsWith("set "))
            {
                SetParameter(g, command.Substring(4));
            }
            else
            {
                g.ForceMessage("<error>", "No such command \"" + command + "\". Use /help to see available commands", Color.DarkRed, Color.White);
            }
        }

        public static void Handle(ConsoleHandler g, string com)
        {
            var command = com.Substring(1).ToLower();
            if (command.Length < 1) return;

            if (command.Contains(" ")) HandleArgCommands(g, command);
            else HandleNoArgCommands(g, command);
        }

        public static void Ascend(GameContainer g, UnderworldSector u, OverworldSector o)
        {
            _game = g;
            _theTown = o;
            _theVoid = u;
        }
    }
}
