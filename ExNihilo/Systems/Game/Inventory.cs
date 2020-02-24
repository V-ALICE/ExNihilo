using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
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
        public readonly ItemInstance[] _inventory = new ItemInstance[InventorySize];

        public const int InventorySize = 24;
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

        [OnDeserialized]
        internal void OnDeserialize(StreamingContext context)
        {
            Dirty = true;
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

        public int GetFirstOpenInventorySlot()
        {
            for (int i = 0; i < _inventory.Length; i++)
            {
                if (_inventory[i] is null) return i;
            }

            return -1;
        }
        public bool TryAddItem(ItemInstance item)
        {
            if (item is InstantInstance inst)
            {
                TapGold(inst.Stats.gold);
                GainExp((uint) inst.Stats.exp);
                Offsets.Hp += inst.Stats.hp;
                Offsets.Mp += inst.Stats.mp;
                return true;
            }

            var slot = GetFirstOpenInventorySlot();
            if (slot == -1) return false;
            _inventory[slot] = item;

            Dirty = true;
            return true;
        }

        public bool CanGrabItem(int heldSlot, bool heldEquipSlot)
        {
            return heldEquipSlot ? _equipment[heldSlot] != null : _inventory[heldSlot] != null;
        }
        public bool TrySwapItem(int heldSlot, int destSlot, bool heldEquipSlot, bool destEquipSlot)
        {
            if (!CanGrabItem(heldSlot, heldEquipSlot)) return false;
            if (heldEquipSlot && destEquipSlot) return false;
            if (heldSlot == destSlot && !heldEquipSlot && !destEquipSlot) return false;
            var heldItem = heldEquipSlot ? _equipment[heldSlot] : _inventory[heldSlot];

            if (destEquipSlot)
            {
                //Equip an item from inventory
                if (heldItem is EquipInstance heldEquip && heldEquip.Type == (EquipItem.SlotType) destSlot)
                {
                    _inventory[heldSlot] = _equipment[destSlot];
                    _equipment[destSlot] = heldEquip;
                    return true;
                }

                return false;
            }

            if (heldEquipSlot)
            {
                //Unequip an item from equipment
                if (!(heldItem is EquipInstance heldEquip)) return false;
                var destItem = _inventory[destSlot] as EquipInstance;
                if (destItem is null || destItem.Type == heldEquip.Type)
                {
                    _equipment[heldSlot] = destItem;
                    _inventory[destSlot] = heldItem;
                    return true;
                }

                return false;
            }

            //Swap two items in inventory
            _inventory[heldSlot] = _inventory[destSlot];
            _inventory[destSlot] = heldItem;
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
