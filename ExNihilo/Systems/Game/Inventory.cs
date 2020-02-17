
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using ExNihilo.Systems.Backend;
using ExNihilo.Systems.Bases;
using ExNihilo.Util;
using Microsoft.Xna.Framework;

namespace ExNihilo.Systems.Game
{
    /*
     * Inventory is a multipurpose container for more than just items as would be normal
     * Inventory holds on to a player's items, equipment, level/stats, skills, and gold
     * This entire container is serialized into the save file for each character, thus it is important
     * not to make this too crazy (it probably wouldn't matter anyway but might as well)
     */

    [Serializable]
    public class Inventory : ISavable
    {
        /*
         * This structure holds temporary changes to player stats
         * These can be from de/buffs or simply lost health and used up MP
         */
        private struct StatOffset
        {
            public int Hp, Mp, MaxHp, MaxMp, Atk, Def, Luck;
        }
        private StatOffset _offsets;
        private List<Tuple<int, Action>> _offsetTriggers;

        /*
         * This structure holds the absolute value of the player's current stats
         * The only changes to stats in this set will be by leveling up or otherwise gaining
         * a permanent stat boost (for example: via non combat skills)
         * Stats have an effective cap of 2,147,483,647 each
         */
        private struct StatSet
        {
            public int MaxHp, MaxMp, Atk, Def, Luck;

            public static StatOffset operator +(StatSet a, StatOffset b)
            {
                return new StatOffset
                {
                    Hp = MathHelper.Clamp(a.MaxHp + b.Hp, 0, a.MaxHp + b.MaxHp),
                    Mp = MathHelper.Clamp(a.MaxMp + b.Mp, 0, a.MaxMp + b.MaxMp),
                    MaxHp = a.MaxHp + b.MaxHp,
                    MaxMp = a.MaxMp + b.MaxMp,
                    Atk = a.Atk + b.Atk,
                    Def = a.Def + b.Def,
                    Luck = a.Luck + b.Luck
                };
            }
        }
        private StatSet _stats;

        private const uint BaseNeededExp = 100;
        private const int BaseHp = 25, BaseMp = 10, BaseAtk = 5, BaseDef = 5, BaseLuck = 0;

        private ulong _heldGold;
        private uint _heldLevel;
        private uint _heldSkillPoints = 2; //temp until skill set class exists
        private uint _heldExp, _nextExp;

        public Inventory()
        {
            _offsets = new StatOffset();
            _stats = new StatSet
            {
                MaxHp = BaseHp,
                MaxMp = BaseMp,
                Atk = BaseAtk,
                Def = BaseDef,
                Luck = BaseLuck
            };
            _nextExp = BaseNeededExp;
            _offsetTriggers = new List<Tuple<int, Action>>();
        }

        public void SoftReset()
        {
            _offsets = new StatOffset();
            _offsetTriggers.Clear();
        }

        private void SettleStats(uint count)
        {
            for (int j = 0; j < count; j++)
            {
                var mark = MathD.urand.Next(5);
                if (mark == 0) _stats.MaxHp++;
                else if (mark == 1) _stats.MaxMp++;
                else if (mark == 2) _stats.Atk++;
                else if (mark == 3) _stats.Def++;
                else if (mark == 4) _stats.Luck++;
            }
        }
        public void LevelUp(uint count = 1)
        {
            for (uint i = 0; i < count; i++)
            {
                _heldLevel++;
                SettleStats(_heldLevel / 2 + 1);
                _heldSkillPoints += _heldLevel < 5 ? 1 : _heldLevel / 5;
            }
        }
        public void GainExp(uint exp)
        {
            _heldExp += exp;
            while (_heldExp > _nextExp)
            {
                _heldExp -= _nextExp;
                _nextExp = (uint) (1.25 * _nextExp);
                LevelUp();
            }
        }

        public void PassTurn(int count=1)
        {
            for (int i = 0; i < count; i++)
            {
                for (int j = _offsetTriggers.Count-1; j>=0; j++)
                {
                    if (_offsetTriggers[j].Item1 == 1)
                    {
                        _offsetTriggers[j].Item2.Invoke();
                        _offsetTriggers.RemoveAt(j);
                    }
                    else _offsetTriggers[j] = new Tuple<int, Action>(_offsetTriggers[j].Item1-1, _offsetTriggers[j].Item2);
                }
            }
        }

        public void Pack(PackedGame game)
        {
            throw new NotImplementedException();
        }
        public void Unpack(PackedGame game)
        {
            throw new NotImplementedException();
        }
    }
}
