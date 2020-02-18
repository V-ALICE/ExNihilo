using System;
using System.Collections.Generic;
using System.IO;
using ExNihilo.Systems.Bases;
using ExNihilo.Util;
using ExNihilo.Util.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ExNihilo.Systems.Game.Items
{
    public class Equipment : Item
    {
        public class EquipmentInstance : ItemInstance
        {
            public ColorScale QualityColor;
            public readonly StatOffset Stats;

            public EquipmentInstance(Equipment item, string fullName, int level, StatOffset stats, ColorScale quality, ColorScale icon) : base(item, level)
            {
                Stats = stats;
                Name = fullName;
                QualityColor = quality;
                IconColor = icon;
            }
        }

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

        //These must have 11 elements. Note last element represents perfection
        private static readonly string[] ModifierSet =
        {
            //Colors: Fine = green, Grand = blue, Legendary = yellow/orange, Mythical = purple, Absolute = rainbow
            "Broken ", "Damaged ", "Shabby ", "Basic ", "", "", "Fine ", "Grand ", "Legendary ", "Mythical ", "Absolute "
        };

        private enum Type
        {
            HEAD, CHEST, HANDS, LEGS, FEET, ACC
        }
        private Type _type;

        private List<string> _materials;

        private readonly float _hp, _mp, _atk, _def, _luck;

        public Equipment(GraphicsDevice g, Texture2D sheet, string name, IEnumerable<string> block) : base(ItemType.Equip)
        {
            Name = name;
            foreach (var line in block)
            {
                try
                {
                    var set = line.Split(' ');
                    switch (set[0])
                    {
                        case "TYPE":
                            _type = (Type) Enum.Parse(typeof(Type), set[1].ToUpper());
                            break;
                        case "ICON":
                            var rect = new Rectangle(int.Parse(set[1]), int.Parse(set[2]), int.Parse(set[3]), int.Parse(set[4]));
                            Texture = TextureUtilities.GetSubTexture(g, sheet, rect);
                            break;
                        case "CHANCE":
                            Chance = float.Parse(set[1]);
                            break;
                        case "STAT":
                            _hp = float.Parse(set[1]);
                            _mp = float.Parse(set[2]);
                            _atk = float.Parse(set[3]);
                            _def = float.Parse(set[4]);
                            _luck = float.Parse(set[5]);
                            break;
                        case "MAT":
                            for (int i = 1; i < set.Length; i++)
                            {
                                if (MaterialSets.ContainsKey(set[i])) _materials.Add(set[i]);
                                else GameContainer.Console.ForceMessage("<warning>", set[i] + " is not a valid material", Color.DarkOrange, Color.White);
                            }
                            break;
                        case "COLOR":
                            IconColor = new Color(int.Parse(set[1]), int.Parse(set[2]), int.Parse(set[3]));
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
                catch (Exception)
                {
                    GameContainer.Console.ForceMessage("<warning>", "Ignoring malformed item pack line \""+line+"\"", Color.DarkOrange, Color.White);
                }
            }
        }

        public static EquipmentInstance GetInstance(Equipment item, int level, Random rand)
        {
            //Set stats
            //  10 +  2-  4 total points at level   1 with standard mults
            //  30 +  6- 18 total points at level  10 with standard mults
            // 210 + 51-153 total points at level 100 with standard mults
            var basic = 2 + 2 * level / 5;
            var count = (int)MathD.BellRange(MathD.urand, level / 2 + 1, 3 * level / 2 + 1);
            var quality = 10 * (count - level / 2 - 1) / level; //0-10
            var stats = new StatOffset
            {
                MaxHp = (int) (item._hp * (basic + count)),
                MaxMp = (int) (item._mp * (basic + count)),
                Atk = (int) (item._atk * (basic + count)),
                Def = (int) (item._def * (basic + count)),
                Luck = (int) (item._luck * (basic + count))
            };

            //Figure out item's name
            var matName = "";
            Color color = item.IconColor;
            if (item._materials.Count > 0)
            {
                var matSet = quality > 6 ? SuperMaterialSets[item._materials[rand.Next(item._materials.Count)]] : MaterialSets[item._materials[rand.Next(item._materials.Count)]];
                var material = matSet[rand.Next(matSet.Count)];
                matName = material.Item1 + " ";
                color = material.Item2;
            }
            var trueName = ModifierSet[quality] + matName + item.Name;

            //Figure out quality color if any
            var textColor = Color.Black;
            if (quality < 3) textColor = Color.DarkRed;
            else if (quality == 6) textColor = Color.ForestGreen;
            else if (quality == 7) textColor = Color.DeepSkyBlue;
            else if (quality == 8) textColor = Color.DarkOrange;
            else if (quality == 9) textColor = Color.Purple;
            else if (quality == 10) textColor = ColorScale.GetFromGlobal("Rainbow");

            //Return generated instance of input item
            return new EquipmentInstance(item, trueName, level, stats, textColor, color);
        }
    }

}
