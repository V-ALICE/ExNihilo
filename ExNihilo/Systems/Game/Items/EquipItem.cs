using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using ExNihilo.Systems.Bases;
using ExNihilo.Util;
using ExNihilo.Util.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ExNihilo.Systems.Game.Items
{
    [Serializable]
    public class EquipInstance : ItemInstance
    {
        public readonly StatOffset Stats;
        public readonly EquipItem.SlotType Type;

        public EquipInstance(EquipItem item, int level, int quality, string fullName, StatOffset stats, ColorScale icon) : base(item, level, quality)
        {
            Stats = stats;
            Name = fullName;
            IconColor = icon;
            Type = item.Slot;
        }

        public override string GetSmartDesc()
        {
            var desc = base.GetSmartDesc();

            desc += "HP   " + Stats.MaxHp + "\n";
            desc += "MP   " + Stats.MaxMp + "\n";
            desc += "ATK  " + Stats.Atk + "\n";
            desc += "DEF  " + Stats.Def + "\n";
            desc += "LUCK " + Stats.Luck + "\n";

            return desc;
        }
        public override Color[] GetSmartColors(Color basic)
        {
            return new Color[] { GetQualityColor(), basic, Color.ForestGreen, Color.DarkRed };
        }

        public string GetSmartDesc(EquipInstance other)
        {
            var desc = base.GetSmartDesc();

            string GetDiff(int value)
            {
                if (value == 0) return "(@c1+0)";
                if (value > 0) return "(@c2+" + value + ")";
                return "(@c3" + value + ")";
            }

            var diff = other.Stats - Stats; //Other is going to be currently equipped item
            desc += "HP   " + Stats.MaxHp + GetDiff(Stats.MaxHp) + "\n";
            desc += "MP   " + Stats.MaxMp + GetDiff(Stats.MaxMp) + "\n";
            desc += "ATK  " + Stats.Atk + GetDiff(Stats.Atk) + "\n";
            desc += "DEF  " + Stats.Def + GetDiff(Stats.Def) + "\n";
            desc += "LUCK " + Stats.Luck + GetDiff(Stats.Luck) + "\n";

            return desc;
        }
    }

    public class EquipItem : Item
    {
        private static readonly Dictionary<string, List<Tuple<string, ColorScale>>> MaterialSets = new Dictionary<string, List<Tuple<string, ColorScale>>>();
        private static readonly Dictionary<string, List<Tuple<string, ColorScale>>> SuperMaterialSets = new Dictionary<string, List<Tuple<string, ColorScale>>>();
        public static void SetUpMaterials(string file)
        {
            var fileName = Environment.CurrentDirectory + "/Content/Resources/" + file;
            if (!File.Exists(fileName)) return;
            var lines = File.ReadAllLines(fileName);

            var category = "";
            var super = false;
            foreach (var line in lines)
            {
                if (line.Length == 0) continue;
                try
                {
                    var set = line.Split(' ');
                    if (set.Length == 1)
                    {
                        //Changing category
                        if (line == "SUPER") super = true;
                        else
                        {
                            category = line;
                            super = false;
                            MaterialSets.Add(category, new List<Tuple<string, ColorScale>>());
                            SuperMaterialSets.Add(category, new List<Tuple<string, ColorScale>>());
                        }
                    }
                    else
                    {
                        //Type for current category (can be rgb255 or a name)
                        ColorScale color;
                        if (set.Length == 2) color = ColorScale.GetFromGlobal(set[1]);
                        else color = new Color(int.Parse(set[1]), int.Parse(set[2]), int.Parse(set[3]));

                        if (super) SuperMaterialSets[category].Add(Tuple.Create(set[0], color));
                        else MaterialSets[category].Add(Tuple.Create(set[0], color));
                    }
                }
                catch (Exception)
                {
                    GameContainer.Console.ForceMessage("<warning>", "Ignoring malformed material line \"" + line + "\"", Color.DarkOrange, Color.White);
                }
            }
        }

        public enum SlotType : byte
        {
            WEAP = 0, HEAD = 1, CHEST = 2, HANDS = 3, LEGS = 4, FEET = 5, ACC = 6
        }
        public readonly SlotType Slot;

        private readonly List<string> _materials;

        private readonly float _hp, _mp, _atk, _def, _luck;

        public EquipItem(GraphicsDevice g, Texture2D sheet, string name, List<string> lines) : base(ItemType.Equip)
        {
            Name = name;
            _materials = new List<string>();
            var tokens = new[] { 1, 1, 1, 1, 1 };
            while (lines.Count > 0)
            {
                try
                {
                    var set = lines[0].Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    switch (set[0])
                    {
                        case "TYPE":
                            Slot = (SlotType)Enum.Parse(typeof(SlotType), set[1]);
                            tokens[0] = 0;
                            break;
                        case "STAT":
                            _hp = float.Parse(set[1]);
                            _mp = float.Parse(set[2]);
                            _atk = float.Parse(set[3]);
                            _def = float.Parse(set[4]);
                            _luck = float.Parse(set[5]);
                            tokens[3] = 0;
                            break;
                        case "MAT":
                            for (int j = 1; j < set.Length; j++)
                            {
                                if (MaterialSets.ContainsKey(set[j]))
                                {
                                    _materials.Add(set[j]);
                                    tokens[4] = 0;
                                }
                                else GameContainer.Console.ForceMessage("<warning>", set[j] + " is not a valid material", Color.DarkOrange, Color.White);
                            }
                            break;
                        case "ICON":
                            var rect = new Rectangle(int.Parse(set[1]), int.Parse(set[2]), int.Parse(set[3]), int.Parse(set[4]));
                            Texture = TextureUtilities.GetSubTexture(g, sheet, rect);
                            tokens[1] = 0;
                            break;
                        case "CHANCE":
                            Chance = float.Parse(set[1]);
                            if (Chance < 0) throw new ArgumentOutOfRangeException();
                            tokens[2] = 0;
                            break;
                        case "COLOR":
                            IconColor = new Color(int.Parse(set[1]), int.Parse(set[2]), int.Parse(set[3]));
                            tokens[4] = 0;
                            break;
                        case "NEW":
                        case "OPEN":
                            Valid = tokens.All(t => t == 0);
                            UID = name + Texture.TextureStrip.Name;
                            //These symbolize the end of the current item if they appear
                            return;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
                catch (Exception)
                {
                    GameContainer.Console.ForceMessage("<warning>", "Ignoring malformed item pack line \"" + lines[0] + "\"", Color.DarkOrange, Color.White);
                }
                lines.RemoveAt(0);
            }
            Valid = tokens.All(t => t == 0);
            UID = name + Texture.TextureStrip.Name;
        }

        public static EquipInstance GetInstance(EquipItem item, Random rand, int level, int qual = -1)
        {
            //Set stats
            //   10 +    3-  12 total points at level    1 with standard mults
            //   30 +   12-  30 total points at level   10 with standard mults
            //  210 +  102- 210 total points at level  100 with standard mults
            // 2010 + 1002-2010 total points at level 1000 with standard mults
            var basic = 2 + 2 * level / 5;
            var min = level + 2;
            var max = 2 * level + 10;

            int count, quality = qual;
            if (qual < 0)
            {
                count = (int)MathD.BellRange(MathD.urand, min, max);
                quality = 10 * (count - min) / (max - min); //0-10
                count /= 5;
            }
            else
            {
                //inverse calculation from quality->rough count
                count = (int)((quality / 10.0 * (max - min) + min) / 5);
            }

            var stats = new StatOffset
            {
                MaxHp = (int)(item._hp * (basic + count)),
                MaxMp = (int)(item._mp * (basic + count)),
                Atk = (int)(item._atk * (basic + count)),
                Def = (int)(item._def * (basic + count)),
                Luck = (int)(item._luck * (basic + count))
            };

            //Figure out item's name
            var matName = "";
            var color = item.IconColor;
            if (item._materials.Count > 0)
            {
                var matSet = quality > 6 ? SuperMaterialSets[item._materials[rand.Next(item._materials.Count)]] : MaterialSets[item._materials[rand.Next(item._materials.Count)]];
                var material = matSet[rand.Next(matSet.Count)];
                matName = material.Item1 + " ";
                color = material.Item2;
            }
            var trueName = matName + item.Name;

            //Return generated instance of input item
            return new EquipInstance(item, level, quality, trueName, stats, color);
        }
    }

}
