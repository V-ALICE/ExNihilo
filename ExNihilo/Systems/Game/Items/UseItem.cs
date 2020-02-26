using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using ExNihilo.Entity;
using ExNihilo.Systems.Bases;
using ExNihilo.Util;
using ExNihilo.Util.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ExNihilo.Systems.Game.Items
{
    [Serializable]
    public class UseInstance : ItemInstance
    {
        private List<UseItem.AOE> Perform;
        private int _mp, _hp;

        public void Activate(Inventory a, List<EntityContainer> b)
        {
            if (Perform.Contains(UseItem.AOE.PLAYER) || Perform.Contains(UseItem.AOE.ALL))
            {
                a.Offsets.Hp += _hp;
                a.Offsets.Mp += _mp;
            }

            if (Perform.Contains(UseItem.AOE.ALL) || Perform.Contains(UseItem.AOE.MULTIENEMY))
            {
                foreach (var e in b)
                {
                    //e.Offsets.Hp += (int)(item._hp * (basic + count));
                    //e.Offsets.Mp += (int)(item._mp * (basic + count));
                }
            }
            else if (Perform.Contains(UseItem.AOE.ENEMY))
            {
                //b[0].Offsets.Hp += (int)(item._hp * (basic + count));
                //b[0].Offsets.Mp += (int)(item._mp * (basic + count));
            }
        }

        public UseInstance(Item item, int level, int quality, int mp, int hp, List<UseItem.AOE> action) : base(item, level, quality)
        {
            Perform = action;
            _mp = mp;
            _hp = hp;
        }

    }

    public class UseItem : Item
    {
        public enum AOE
        {
            PLAYER, ENEMY, MULTIENEMY, ALL
        }
        private readonly List<AOE> _flags = new List<AOE>();

        private readonly float _hp, _mp;

        public UseItem(GraphicsDevice g, Texture2D sheet, string name, List<string> lines) : base(ItemType.Use)
        {
            Name = name;
            var tokens = new[] { 1, 1, 1, 1, 1 };
            while (lines.Count > 0)
            {
                try
                {
                    var set = lines[0].Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    switch (set[0])
                    {
                        case "FLAGS":
                            for (int i = 1; i < set.Length; i++) _flags.Add((AOE)Enum.Parse(typeof(AOE), set[i]));
                            tokens[4] = 0;
                            break;
                        case "HP":
                            _hp = float.Parse(set[1]);
                            tokens[0] = 0;
                            break;
                        case "MP":
                            _mp = float.Parse(set[1]);
                            tokens[0] = 0;
                            break;
                        case "EFFECT":
                            //TODO:
                            tokens[0] = 0;
                            break;
                        case "ICON":
                            var rect = new Rectangle(int.Parse(set[1]), int.Parse(set[2]), int.Parse(set[3]), int.Parse(set[4]));
                            Texture = TextureUtilities.GetSubTexture(g, sheet, rect);
                            tokens[3] = 0;
                            break;
                        case "CHANCE":
                            Chance = float.Parse(set[1]);
                            if (Chance < 0) throw new ArgumentOutOfRangeException();
                            tokens[2] = 0;
                            break;
                        case "COLOR":
                            if (set.Length == 2) IconColorLookup = set[1];
                            else IconColor = new Color(int.Parse(set[1]), int.Parse(set[2]), int.Parse(set[3]));
                            tokens[1] = 0;
                            break;
                        case "NEW":
                        case "OPEN":
                            Valid = tokens.All(t => t == 0);
                            UID = name + Chance;
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
            UID = name + Chance;
        }

        public static UseInstance GetInstance(UseItem item, Random rand, int level, int qual = -1)
        {
            var basic = 2 + 2 * level / 5;
            var min = level / 2 + 2;
            var max = level + 10;

            int count, quality = qual;
            if (qual < 0)
            {
                count = (int) MathD.BellRange(MathD.urand, min, max);
                quality = 10 * (count - min) / (max - min); //0-10
                count /= 2;
            }
            else
            {
                //inverse calculation from quality->rough count
                count = (int) ((quality / 10.0 * (max - min) + min) / 4);
            }

            var mp = (int) ((basic + count) * item._mp);
            var hp = (int) ((basic + count) * item._hp);

            return new UseInstance(item, level, quality, mp, hp, item._flags);
        }
    }
}
