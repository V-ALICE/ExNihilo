
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using ExNihilo.Systems.Bases;
using ExNihilo.Systems.Game.Items.ExNihilo.Systems.Game.Items;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ExNihilo.Systems.Game.Items
{
    public static class ItemLoader
    {
        private static readonly Dictionary<EquipItem.SlotType, List<EquipItem>> _equipSet = new Dictionary<EquipItem.SlotType, List<EquipItem>>();
        private static readonly List<InstantItem> _instantSet = new List<InstantItem>();
        private static readonly List<UseItem> _useSet = new List<UseItem>();
        private static float _totalEquipChance, _totalInstantChance, _totalUseChance;

        public static void LoadItems(GraphicsDevice g, string materialFile)
        {
            EquipItem.SetUpMaterials(materialFile);
            foreach (var e in (EquipItem.SlotType[]) Enum.GetValues(typeof(EquipItem.SlotType)))
            {
                _equipSet.Add(e, new List<EquipItem>());
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
                                    var equip = new EquipItem(g, curTex, name, lines);
                                    if (equip.Valid)
                                    {
                                        _equipSet[equip.Slot].Add(equip);
                                        _totalEquipChance += equip.Chance;
                                    }
                                    else GameContainer.Console.ForceMessage("<error>", "Item with name \"" + equip.Name + "\" is invalid", Color.DarkRed, Color.White);
                                    break;
                                case "USE":
                                    var use = new UseItem(g, curTex, name, lines);
                                    if (use.Valid)
                                    {
                                        _useSet.Add(use);
                                        _totalUseChance += use.Chance;
                                    }
                                    else GameContainer.Console.ForceMessage("<error>", "Item with name \"" + use.Name + "\" is invalid", Color.DarkRed, Color.White);
                                    break;
                                case "INSTANT":
                                    var instant = new InstantItem(g, curTex, name, lines);
                                    if (instant.Valid)
                                    {
                                        _instantSet.Add(instant);
                                        _totalInstantChance += instant.Chance;
                                    }
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

        public static EquipInstance GetEquipment(Random rand, int level, int quality = -1)
        {
            var selection = (float)(_totalEquipChance * rand.NextDouble());
            foreach (var l in _equipSet)
            {
                foreach (var e in l.Value)
                {
                    selection -= e.Chance;
                    if (selection < 0) return EquipItem.GetInstance(e, rand, level, quality);
                }
            }

            return null;
        }

        public static EquipInstance GetEquipment(Random rand, int level, EquipItem.SlotType type, int quality = -1)
        {
            if (!_equipSet.ContainsKey(type)) return null;

            var selection = (float)(_totalEquipChance * rand.NextDouble());

            //Since individual sets don't have total chances this isn't a fully fair fetch
            //For most equipment sets items in the back of the list will have a slightly lower chance than normal
            //This function should only be used for initial character equips
            while (true)
            {
                foreach (var e in _equipSet[type])
                {
                    selection -= e.Chance;
                    if (selection < 0) return EquipItem.GetInstance(e, rand, level, quality);
                }
            }
        }

        public static InstantInstance GetInstant(Random rand, int level, int quality = -1)
        {
            var selection = (float)(_totalInstantChance * rand.NextDouble());
            foreach (var i in _instantSet)
            {
                selection -= i.Chance;
                if (selection < 0) return InstantItem.GetInstance(i, rand, level, quality);
            }

            return null;
        }

        public static UseInstance GetUse(Random rand, int level, int quality = -1)
        {
            var selection = (float)(_totalUseChance * rand.NextDouble());
            foreach (var u in _useSet)
            {
                selection -= u.Chance;
                if (selection < 0) return UseItem.GetInstance(u, rand, level, quality);
            }

            return null;
        }

        public static ItemInstance GetItem(Random rand, int level)
        {
            var chance = _totalEquipChance + _totalInstantChance + _totalUseChance;
            var selection = (float)(chance * rand.NextDouble());

            if (selection > _totalEquipChance) selection -= _totalEquipChance;
            else
            {
                foreach (var l in _equipSet)
                {
                    foreach (var e in l.Value)
                    {
                        selection -= e.Chance;
                        if (selection < 0) return EquipItem.GetInstance(e, rand, level);
                    }
                }
            }

            if (selection > _totalInstantChance) selection -= _totalInstantChance;
            else
            {
                foreach (var i in _instantSet)
                {
                    selection -= i.Chance;
                    if (selection < 0) return InstantItem.GetInstance(i, rand, level);
                }
            }

            foreach (var u in _useSet)
            {
                selection -= u.Chance;
                if (selection < 0) return UseItem.GetInstance(u, rand, level);
            }

            return null;
        }

        public static bool RestoreItemInstance(ItemInstance item)
        {
            if (item is EquipInstance e)
            {
                foreach (var i in _equipSet[e.Type])
                {
                    if (i.UID == e.UID)
                    {
                        item.Restore(i);
                        return true;
                    } 
                }
            }
            else if (item is UseInstance u)
            {
                foreach (var i in _useSet)
                {
                    if (i.UID == u.UID)
                    {
                        item.Restore(i);
                        return true;
                    }
                }
            }

            return false;
        }
    }
}
