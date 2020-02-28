using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using ExNihilo.Systems.Bases;
using ExNihilo.Util;
using ExNihilo.Util.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ExNihilo.Systems.Game.Items
{
    namespace ExNihilo.Systems.Game.Items
    {
        public class InstantInstance : ItemInstance
        {
            private readonly InstantItem.InstantItemStats _stats;

            public override string GetSmartDesc()
            {
                var text = base.GetSmartDesc() + '\n';
                if (_stats.hp > 0) text += "Gain " + _stats.hp + " health\n";
                else if (_stats.hp < 0) text += "Lose " + Math.Abs(_stats.hp) + " health\n";
                if (_stats.mp > 0) text += "Gain " + _stats.mp + " mana\n";
                else if (_stats.mp < 0) text += "Lose " + Math.Abs(_stats.mp) + " mana\n";
                if (_stats.exp > 0) text += "Gain " + _stats.exp + " exp\n";
                else if (_stats.exp < 0) text += "Lose " + Math.Abs(_stats.exp) + " exp\n";
                if (_stats.gold > 0) text += "Gain " + _stats.gold + " gold\n";
                else if (_stats.gold < 0) text += "Lose " + Math.Abs(_stats.gold) + " gold\n";
                return text;
            }

            public void Trigger(Inventory i)
            {
                i.AdjustInstant(_stats);
            }

            public InstantInstance(Item item, int level, int quality, InstantItem.InstantItemStats stats) : base(item, level, quality)
            {
                _stats = stats;
            }

        }

        public class InstantItem : Item
        {
            public struct InstantItemStats
            {
                public int gold, exp, hp, mp;
            }

            private readonly float _gold, _exp, _hp, _mp;

            public InstantItem(GraphicsDevice g, Texture2D sheet, string name, List<string> lines) : base(ItemType.Instant)
            {
                Name = name;
                var tokens = new[] {1, 1, 1, 1};
                while (lines.Count > 0)
                {
                    try
                    {
                        var set = lines[0].Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                        switch (set[0])
                        {
                            case "GOLD":
                                _gold = float.Parse(set[1]);
                                tokens[1] = 0;
                                break;
                            case "EXP":
                                _exp = float.Parse(set[1]);
                                tokens[1] = 0;
                                break;
                            case "HP":
                                _hp = float.Parse(set[1]);
                                tokens[1] = 0;
                                break;
                            case "MP":
                                _mp = float.Parse(set[1]);
                                tokens[1] = 0;
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
                                IconColor = new Color(int.Parse(set[1]), int.Parse(set[2]), int.Parse(set[3]));
                                tokens[0] = 0;
                                break;
                            case "NEW":
                            case "OPEN":
                                Valid = tokens.All(t => t == 0);
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
            }

            public static InstantInstance GetInstance(InstantItem item, Random rand, int level, int qual = -1)
            {
                var basic = 2 + 2 * level / 5;
                var min = level + 2;
                var max = 2 * level + 10;

                int count, quality = qual;
                if (qual < 0)
                {
                    count = (int) MathD.BellRange(MathD.urand, min, max);
                    quality = 10 * (count - min) / (max - min); //0-10
                    count /= 4;
                }
                else
                {
                    //inverse calculation from quality->rough count
                    count = (int) ((quality / 10.0 * (max - min) + min) / 4);
                }

                var stats = new InstantItemStats
                {
                    hp = (int) (item._hp * (rand.NextDouble() / 5 + 0.9) * (basic + count)),
                    mp = (int) (item._mp * (rand.NextDouble() / 5 + 0.9) * (basic + count)),
                    exp = (int) (item._exp * (rand.NextDouble() / 5 + 0.9) * (basic + count)),
                    gold = (int) (item._gold * (rand.NextDouble() / 5 + 0.9) * (basic + count))
                };

                return new InstantInstance(item, level, quality, stats);
            }
        }
    }
}
