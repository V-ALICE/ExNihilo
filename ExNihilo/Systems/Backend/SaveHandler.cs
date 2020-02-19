using System;
using System.Collections.Generic;
//using System.Drawing;
using System.Globalization;
using System.IO;
using ExNihilo.Entity;
using ExNihilo.Systems.Game;
using ExNihilo.Util;
using ExNihilo.Util.Graphics;
using Microsoft.Xna.Framework;

namespace ExNihilo.Systems.Backend
{
    [Serializable]
    public class PackedGame
    {
        // Update this whenever the save contents changes MMDDYYHH
        public const string _version = "02182021";

        //configuration
        private readonly DateTime _lastSaveDate;
        public readonly string Version;
        public string ID { get; }
        public string TitleCard { get; private set; }

        //flags
        public bool InVoid;

        //data
        public List<PlayerEntityContainer.PackedPlayerEntityContainer> SavedCharacters { get; }
        public int CurrentPlayer;
        public int Floor, Parallax, Seed;
        public MapGenerator.Type GenType;
        public string[] TexturePack;

        private void FormatTitleCard()
        {
            var diff = _lastSaveDate.ToString(CultureInfo.InvariantCulture).Length - ID.Length;
            var paddedID = ID;
            var paddedDate = _lastSaveDate.ToString(CultureInfo.InvariantCulture);

            if (diff < 0 && diff % 2 == 0)
            {
                diff = Math.Abs(diff);
                paddedDate = paddedDate.PadLeft(paddedDate.Length + diff / 2).PadRight(paddedDate.Length + diff / 2);
            }
            else if (diff < 0)
            {
                diff = Math.Abs(diff);
                paddedDate = "@h"+paddedDate.PadLeft(paddedDate.Length + diff / 2).PadRight(paddedDate.Length + diff / 2)+"@h";
            }
            else if (diff > 0 && diff % 2 == 0)
            {
                paddedID = paddedID.PadLeft(paddedID.Length + diff / 2).PadRight(paddedID.Length + diff / 2);
            }
            else if (diff > 0)
            {
                paddedID = "@h" + paddedID.PadLeft(paddedID.Length + diff / 2).PadRight(paddedID.Length + diff / 2) + "@h";
            }

            TitleCard = paddedID + "\n" + paddedDate;
        }
        public PackedGame(GameContainer game, string id)
        {
            //Default game file stuff
            ID = id;
            Version = _version;
            _lastSaveDate = DateTime.Now;
            FormatTitleCard();
            SavedCharacters = new List<PlayerEntityContainer.PackedPlayerEntityContainer>
            {
                new PlayerEntityContainer(game.GraphicsDevice, "Player", 0,0,0,0).GetPacked()
            };
            CurrentPlayer = 0;
            InVoid = false;
            Floor = 1;
            Parallax = 2;
            Seed = 123;
            GenType = MapGenerator.Type.Standard2;
            TexturePack = new[] {"DawnLikeComplete.tmf", "", ""};
        }

    }

    [Serializable]
    public class GameParameters
    {
        public float MusicVolume, EffectVolume;
        public byte ParticleType, ParticleColor;

        public GameParameters()
        {
            //Default Parameter set
            MusicVolume = 0.5f;
            EffectVolume = 0.5f;
            ParticleType = 4;
            ParticleColor = 0;
        }
    }

    public static class SaveHandler
    {
        private static readonly Dictionary<string, PackedGame> _saveSet = new Dictionary<string, PackedGame>();

        public const string FILE_1 = "File1.mem", FILE_2 = "File2.mem", FILE_3 = "File3.mem", PARAMETERS = "Parameters.mem";
        public static GameParameters Parameters { get; private set; }
        public static string LastLoadedFile { get; private set; }

        public static void DeleteSave(string file)
        {
            if (_saveSet.ContainsKey(file)) _saveSet.Remove(file);
            string fileName = Environment.CurrentDirectory + "/Content/" + file;
            if (File.Exists(fileName)) File.Delete(fileName);
        }

        public static void Save(string file, PackedGame game)
        {
            string fileName = Environment.CurrentDirectory + "/Content/" + file;
            if (_saveSet.ContainsKey(file)) _saveSet[file] = game;
            else _saveSet.Add(file, game);
            EncryptedSerializer.SerializeOut(fileName, game);
        }

        public static bool HasSave(string file)
        {
            return _saveSet.ContainsKey(file);
        }
        public static string GetLastID()
        {
            if (!HasSave(LastLoadedFile)) return "";
            return _saveSet[LastLoadedFile].ID;
        }
        public static PackedGame GetSave(string file, bool intentToLoad)
        {
            if (intentToLoad) LastLoadedFile = file;
            return _saveSet.ContainsKey(file) ? _saveSet[file] : null;
        }

        public static void LoadAllSaves(params string[] files)
        {
            foreach (var file in files)
            {
                string fileName = Environment.CurrentDirectory + "/Content/" + file;
                try
                {
                    if (EncryptedSerializer.DeserializeIn(fileName) is PackedGame game)
                    {
                        if (game.Version == PackedGame._version) _saveSet.Add(file, game);
                    }
                }
                catch (Exception e)
                {
                    GameContainer.Console.ForceMessage("<error>", e.Message, Color.DarkRed, ColorScale.White);
                }
            }
        }

        public static void SaveParameters()
        {
            string fileName = Environment.CurrentDirectory + "/Content/" + PARAMETERS;
            EncryptedSerializer.SerializeOut(fileName, Parameters);
        }

        public static void LoadParameters()
        {
            string fileName = Environment.CurrentDirectory + "/Content/" + PARAMETERS;
            Parameters = EncryptedSerializer.DeserializeIn(fileName) as GameParameters;
            if (Parameters is null)
            {
                //The parameters file is either broken or not present
                if (File.Exists(fileName)) File.Delete(fileName);
                Parameters = new GameParameters();
                EncryptedSerializer.SerializeOut(fileName, Parameters);
            }
        }
    }
}
