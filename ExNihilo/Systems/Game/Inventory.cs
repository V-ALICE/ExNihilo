using System;
using System.Collections.Generic;
using ExNihilo.Systems.Bases;
using ExNihilo.Systems.Game.Items;
using ExNihilo.Systems.Game.Items.ExNihilo.Systems.Game.Items;
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
        private List<Tuple<int, StatOffset>> _offsetTriggers;
        public StatSet Stats;

        public readonly EquipInstance[] _equipment = new EquipInstance[7];
        public readonly List<ItemInstance> _inventory = new List<ItemInstance>(InventorySize);

        private const int InventorySize = 24;
        private const uint BaseNeededExp = 100;
        private const int BaseHp = 25, BaseMp = 10, BaseAtk = 5, BaseDef = 5, BaseLuck = 0;

        public long HeldGold;
        public uint HeldLevel;
        public uint HeldSkillPoints = 2; //temp until skill set class exists
        public uint HeldExp, NextExp;

        public bool Dirty;

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
            NextExp = BaseNeededExp;
            _offsetTriggers = new List<Tuple<int, StatOffset>>();
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
            SoftReset();
            for (uint i = 0; i < count; i++)
            {
                HeldLevel++;
                SettleStats(HeldLevel / 2 + 1);
                HeldSkillPoints += HeldLevel < 5 ? 1 : HeldLevel / 5;
            }
        }
        public void GainExp(uint exp)
        {
            HeldExp += exp;
            while (HeldExp > NextExp)
            {
                HeldExp -= NextExp;
                NextExp = (uint) (1.25 * NextExp);
                LevelUp();
            }
        }

        public long GetGold()
        {
            return HeldGold;
        }
        public void TapGold(long change)
        {
            HeldGold += change;
            if (HeldGold < 0) HeldGold = 0;
        }

        public bool CanAddItem()
        {
            return _inventory.Count < _inventory.Capacity;
        }
        public bool TryAddItem(ItemInstance item)
        {
            if (item is InstantInstance)
            {

            }
            if (!CanAddItem()) return false;

            _inventory.Add(item);
            Dirty = true;
            return true;
        }

        public void AddTriggeredOffset(int turns, StatOffset diff)
        {
            _offsetTriggers.Add(new Tuple<int, StatOffset>(turns, diff));
            Offsets += diff;
        }
        public void PassTurn(int count=1)
        {
            for (int i = 0; i < count; i++)
            {
                for (int j = _offsetTriggers.Count-1; j>=0; j++)
                {
                    if (_offsetTriggers[j].Item1 == 1)
                    {
                        Offsets -= _offsetTriggers[j].Item2;
                        _offsetTriggers.RemoveAt(j);
                    }
                    else _offsetTriggers[j] = new Tuple<int, StatOffset>(_offsetTriggers[j].Item1-1, _offsetTriggers[j].Item2);
                }
            }
        }
    }
}
