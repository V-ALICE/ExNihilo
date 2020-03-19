using System;
using Microsoft.Xna.Framework;

namespace ExNihilo.Systems.Game
{
    /*
     * This structure holds temporary changes to player stats
     * These can be from de/buffs or simply lost health and used up MP
     */
    [Serializable]
    public struct StatOffset
    {
        public int Hp, Mp, MaxHp, MaxMp, Atk, Def, Luck;

        public static StatOffset operator +(StatOffset a, StatOffset b)
        {
            return new StatOffset
            {
                Hp = a.Hp + b.Hp,
                Mp = a.Mp + b.Mp,
                MaxHp = a.MaxHp + b.MaxHp,
                MaxMp = a.MaxMp + b.MaxMp,
                Atk = a.Atk + b.Atk,
                Def = a.Def + b.Def,
                Luck = a.Luck + b.Luck
            };
        }

        public static StatOffset operator -(StatOffset a, StatOffset b)
        {
            return new StatOffset
            {
                Hp = a.Hp - b.Hp,
                Mp = a.Mp - b.Mp,
                MaxHp = a.MaxHp - b.MaxHp,
                MaxMp = a.MaxMp - b.MaxMp,
                Atk = a.Atk - b.Atk,
                Def = a.Def - b.Def,
                Luck = a.Luck - b.Luck
            };
        }
    }
    

    /*
     * This structure holds the absolute value of the player's current stats
     * The only changes to stats in this set will be by leveling up or otherwise gaining
     * a permanent stat boost (for example: via non combat skills)
     * Stats have an effective cap of 2,147,483,647 each
     */
    [Serializable]
    public struct StatSet
    {
        public int MaxHp, MaxMp, Atk, Def, Luck;

        public static StatOffset operator +(StatSet a, StatOffset b)
        {
            return new StatOffset
            {
                Hp = MathHelper.Clamp(a.MaxHp + b.MaxHp + b.Hp, 0, a.MaxHp + b.MaxHp),
                Mp = MathHelper.Clamp(a.MaxMp + b.MaxMp + b.Mp, 0, a.MaxMp + b.MaxMp),
                MaxHp = a.MaxHp + b.MaxHp,
                MaxMp = a.MaxMp + b.MaxMp,
                Atk = a.Atk + b.Atk,
                Def = a.Def + b.Def,
                Luck = a.Luck + b.Luck
            };
        }
    }
}
