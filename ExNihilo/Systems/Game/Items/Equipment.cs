using System;
using System.Collections.Generic;
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

        //TODO: these need colors
        private static readonly Dictionary<Material, string[]> MaterialSets = new Dictionary<Material, string[]>
        {
            {Material.WOOD, new []{"Wooden", "Oak" } },
            {Material.METAL, new []{"Iron", "Steel", "Bronze", "Copper", "Silver"} },
            {Material.GEM, new []{"Crystal", "Jade", "Amethyst", "Topaz"} },
            {Material.CLOTH, new []{"Cloth", "Leather", "Fur"} }
        };
        private static readonly Dictionary<Material, string[]> SuperMaterialSets = new Dictionary<Material, string[]>
        {
            {Material.WOOD, new []{"Living Wood", "Mahogany", "Ebony" } },
            {Material.METAL, new []{"Platinum", "Mithril", "Orichalcum"} },
            {Material.GEM, new []{"Emerald", "Ruby", "Sapphire", "Diamond"} },
            {Material.CLOTH, new []{"Silk", "Leather", "Fur"} }
        };

        //These must have 11 elements. Note last element represents perfection
        private static readonly string[] ModifierSet =
        {
            "Broken", "Damaged", "Shabby", "Basic", "", "", "Fine", "Grand", "Legendary", "Mythical", "Absolute"
        };

        private enum Type
        {
            HEAD, CHEST, HANDS, LEGS, FEET, ACC
        }
        private Type _type;

        private enum Material
        {
            WOOD, METAL, GEM, CLOTH, SUPERMETAL
        }
        private readonly List<Material> _materials = new List<Material>();

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
                            for (int i = 1; i < set.Length; i++) _materials.Add((Material) Enum.Parse(typeof(Material), set[i].ToUpper()));
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
            //  10 +  2-  4 total points at level   1 with standard mults
            //  30 +  6- 18 total points at level  10 with standard mults
            // 210 + 51-153 total points at level 100 with standard mults
            var basic = 2 + 2 * level / 5;
            var stats = new StatOffset
            {
                MaxHp = (int) (item._hp * basic),
                MaxMp = (int) (item._mp * basic),
                Atk = (int) (item._atk * basic),
                Def = (int) (item._def * basic),
                Luck = (int) (item._luck * basic)
            };
            var count = (int) MathD.BellRange(MathD.urand, level / 2 + 1, 3 * level / 2 + 1);
            var quality = 10 * (count - level / 2 - 1) / level; //0-10
            for (int j = 0; j < count; j++)
            {
                var mark = rand.Next(5);
                if (mark == 0) stats.MaxHp++;
                else if (mark == 1) stats.MaxMp++;
                else if (mark == 2) stats.Atk++;
                else if (mark == 3) stats.Def++;
                else if (mark == 4) stats.Luck++;
            }

            //Figure out item's name
            var matType = MaterialSets[item._materials[rand.Next(item._materials.Count)]];
            var mat = matType.Length > 0 ? matType[rand.Next(matType.Length)] + " " : "";
            var mod = ModifierSet[quality];
            var trueName = (mod.Length > 0 ? mod + " " : "") + mat + item.Name;

            //Return generated instance of input item
            return new EquipmentInstance(item, trueName, level, stats, ColorScale.White, ColorScale.White);
        }
    }
}
