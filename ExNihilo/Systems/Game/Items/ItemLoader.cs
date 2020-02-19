
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ExNihilo.Systems.Game.Items
{
    public static class ItemLoader
    {
        private static readonly Dictionary<Equipment.SlotType, List<Equipment>> _equipSet = new Dictionary<Equipment.SlotType, List<Equipment>>();
        private static readonly List<InstantItem> _instantSet = new List<InstantItem>();
        private static readonly List<UseItem> _useSet = new List<UseItem>();

        public static void LoadItems(GraphicsDevice g, string materialFile)
        {
            Equipment.SetUpMaterials(materialFile);
            foreach (var e in (Equipment.SlotType[]) Enum.GetValues(typeof(Equipment.SlotType)))
            {
                _equipSet.Add(e, new List<Equipment>());
            }

            var fileSet = Directory.GetFiles(Environment.CurrentDirectory + "/Content/Items/");
            foreach (var fileName in fileSet)
            {
                try
                {
                    //Open tmi file archive and confirm it has a description file
                    var file = File.OpenRead(fileName);
                    var zip = new ZipArchive(file, ZipArchiveMode.Read);
                    var desc = zip.Entries.FirstOrDefault(f => f.FullName == "desc");
                    if (desc is null) throw new IndexOutOfRangeException();

                    //Get all valid description lines
                    var lines = new List<string>();
                    var descStream = new StreamReader(desc.Open());
                    while (!descStream.EndOfStream)
                    {
                        var line = descStream.ReadLine();
                        if (line is null) break;
                        if (line.Length == 0 || line.StartsWith("#")) continue;
                        lines.Add(line);
                    }
                    descStream.Close();

                    Texture2D curTex = null;
                    while (lines.Count > 0)
                    {
                        if (lines[0].StartsWith("OPEN "))
                        {
                            //Open a new texture file
                            var tex = zip.Entries.FirstOrDefault(f => f.FullName == lines[0].Substring(5));
                            if (tex is null) throw new IndexOutOfRangeException();
                            var texStream = tex.Open();
                            curTex = Texture2D.FromStream(g, texStream);
                            texStream.Dispose();
                            lines.RemoveAt(0);
                        }
                        else if (curTex != null && lines[0].StartsWith("NEW "))
                        {
                            var data = lines[0].Substring(4);
                            var type = data.Substring(0, data.IndexOf(' '));
                            var name = data.Substring(data.IndexOf(' ') + 1);
                            lines.RemoveAt(0);
                            switch (type)
                            {
                                case "EQUIP":
                                    var equip = new Equipment(g, curTex, name, lines);
                                    if (equip.Valid) _equipSet[equip.Slot].Add(equip);
                                    else GameContainer.Console.ForceMessage("<error>", "Item with name \"" + equip.Name + "\" is invalid", Color.DarkRed, Color.White);
                                    break;
                                case "USE":
                                    var use = new UseItem(g, curTex, name, lines);
                                    if (use.Valid) _useSet.Add(use);
                                    else GameContainer.Console.ForceMessage("<error>", "Item with name \"" + use.Name + "\" is invalid", Color.DarkRed, Color.White);
                                    break;
                                case "INSTANT":
                                    var instant = new InstantItem(g, curTex, name, lines);
                                    if (instant.Valid) _instantSet.Add(instant);
                                    else GameContainer.Console.ForceMessage("<error>", "Item with name \"" + instant.Name + "\" is invalid", Color.DarkRed, Color.White);
                                    break;
                                default:
                                    throw new IndexOutOfRangeException();
                            }
                        }
                        else
                        {
                            GameContainer.Console.ForceMessage("<warning>", "Ignoring unexpected line \"" + lines[0] + "\" in item pack description", Color.DarkOrange, Color.White);
                            lines.RemoveAt(0);
                        }
                    }

                    file.Close(); //Close tmi file
                }
                catch (IndexOutOfRangeException)
                {
                    GameContainer.Console.ForceMessage("<error>", "Item pack file \"" + fileName + "\" is malformed", Color.DarkRed, Color.White);
                }
                catch (Exception e)
                {
                    GameContainer.Console.ForceMessage("<error>", e.Message, Color.DarkRed, Color.White);
                }
            }

            if (_equipSet.Count == 0 && _useSet.Count == 0 && _instantSet.Count == 0) throw new FileLoadException("No items could be loaded successfully");
        }

        public static EquipmentInstance GetEquipment(Random rand, int level)
        {
            var list = _equipSet.Values.ToList()[rand.Next(_equipSet.Count)];
            return Equipment.GetInstance(list[rand.Next(list.Count)], rand, level);
        }

        public static EquipmentInstance GetEquipment(Random rand, int level, Equipment.SlotType type, int quality)
        {
            if (!_equipSet.ContainsKey(type)) return null;
            return Equipment.GetInstance(_equipSet[type][rand.Next(_equipSet[type].Count)], rand, level, quality);
        }
    }
}
