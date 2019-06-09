using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using ExNihilo.Util;

namespace ExNihilo.Systems
{
    [Serializable]
    public class PackedGame
    {
        public string ID { get; }
        public string TitleCard { get; private set; }
        private DateTime _lastSaveDate;

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
        public PackedGame(string id)
        {
            //Default game file stuff
            ID = id;
            _lastSaveDate = DateTime.Now;
            FormatTitleCard();
        }

        public void Pack(GameContainer game)
        {
            _lastSaveDate = DateTime.Now;
        }
    }

    [Serializable]
    public class GameParameters
    {
        public float MusicVolume, EffectVolume;

        public GameParameters()
        {
            //Default Parameter set
            MusicVolume = 0.5f;
            EffectVolume = 0.5f;
        }
    }

    public static class SaveHandler
    {
        public const string FILE_1 = "File1.mem", FILE_2 = "File2.mem", FILE_3 = "File3.mem", PARAMETERS = "Parameters.mem";

        private static readonly Dictionary<string, PackedGame> _saveSet = new Dictionary<string, PackedGame>();
        public static GameParameters Parameters;

        public static void DeleteSave(string file)
        {
            if (_saveSet.ContainsKey(file)) _saveSet.Remove(file);
            string fileName = Environment.CurrentDirectory + "/Content/" + file;
            if (File.Exists(fileName)) File.Delete(fileName);
        }

        public static void Save(string file, PackedGame game)
        {
            string fileName = Environment.CurrentDirectory + "/Content/" + file;
            if (!_saveSet.ContainsKey(file)) _saveSet.Add(file, game);
            EncryptedSerializer.SerializeOut(fileName, game);
        }

        public static bool HasSave(string file)
        {
            return _saveSet.ContainsKey(file);
        }
        public static PackedGame GetSave(string file)
        {
            return _saveSet.ContainsKey(file) ? _saveSet[file] : null;
        }

        public static void LoadAllSaves(params string[] files)
        {
            foreach (var file in files)
            {
                string fileName = Environment.CurrentDirectory + "/Content/" + file;
                if (EncryptedSerializer.DeserializeIn(fileName) is PackedGame game) _saveSet.Add(file, game);
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
