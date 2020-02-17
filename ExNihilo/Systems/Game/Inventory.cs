using System;
using System.Collections.Generic;
using ExNihilo.Util;

namespace ExNihilo.Systems.Game
{
    /*
     * Inventory is a multipurpose container for more than just items as would be normal
     * Inventory holds on to a player's items, equipment, level/stats, skills, and gold
     * This entire container is serialized into the save file for each character, thus it is important
     * not to make this too crazy (it probably wouldn't matter anyway but might as well)
     */
    [Serializable]
    public class Inventory
    {
        public StatOffset Offsets;
        private List<Tuple<int, Action>> _offsetTriggers;
        public StatSet Stats;

        //TODO: add equipment set
        //TODO: add inventory set

        private const uint BaseNeededExp = 100;
        private const int BaseHp = 25, BaseMp = 10, BaseAtk = 5, BaseDef = 5, BaseLuck = 0;

        private long _heldGold;
        private uint _heldLevel;
        private uint _heldSkillPoints = 2; //temp until skill set class exists
        private uint _heldExp, _nextExp;

        public Inventory()
        {
            Offsets = new StatOffset();
            Stats = new StatSet
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
            Offsets = new StatOffset();
            _offsetTriggers.Clear();
        }

        private void SettleStats(uint count)
        {
            for (int j = 0; j < count; j++)
            {
                var mark = MathD.urand.Next(5);
                if (mark == 0) Stats.MaxHp++;
                else if (mark == 1) Stats.MaxMp++;
                else if (mark == 2) Stats.Atk++;
                else if (mark == 3) Stats.Def++;
                else if (mark == 4) Stats.Luck++;
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

        public long GetGold()
        {
            return _heldGold;
        }
        public void TapGold(long change)
        {
            _heldGold += change;
            if (_heldGold < 0) _heldGold = 0;
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
    }
}
