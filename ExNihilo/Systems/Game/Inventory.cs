using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using ExNihilo.Systems.Bases;
using ExNihilo.Systems.Game.Items;
using ExNihilo.Systems.Game.Items.ExNihilo.Systems.Game.Items;
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
    public class Inventory
    {
        private StatOffset _offsets;
        private readonly List<Tuple<int, StatOffset>> _offsetTriggers;
        private StatSet _stats;

        public readonly EquipInstance[] Equipment = new EquipInstance[7];
        public readonly ItemInstance[] Items = new ItemInstance[InventorySize];

        private ItemInstance _lastTrashedItem;

        public const int InventorySize = 23;
        private const int BaseNeededExp = 100;
        private const int BaseHp = 25, BaseMp = 10, BaseAtk = 5, BaseDef = 5, BaseLuck = 0;

        public long HeldGold;
        public int HeldLevel;
        public int HeldSkillPoints = 2; //temp until skill set class exists
        public int HeldExp, NextExp;

        public bool Dirty;

        public Inventory()
        {
            HeldLevel = 1;
            _offsets = new StatOffset();
            _stats = new StatSet
            {
                MaxHp = BaseHp,
                MaxMp = BaseMp,
                Atk = BaseAtk,
                Def = BaseDef,
                Luck = BaseLuck
            };
            NextExp = BaseNeededExp;
            _offsetTriggers = new List<Tuple<int, StatOffset>>();
            for (int i = 0; i < Equipment.Length; i++)
            {
                Equipment[i] = ItemLoader.GetEquipment(MathD.urand, 1, (EquipItem.SlotType) i, 2);
            }
        }

        [OnDeserialized]
        internal void OnDeserialized(StreamingContext context)
        {
            Dirty = true;
            foreach (var e in Equipment)
            {
                if (e is null) continue;
                ItemLoader.RestoreItemInstance(e);
            }

            foreach (var i in Items)
            {
                if (i is null) continue;
                ItemLoader.RestoreItemInstance(i);
            }

            ItemLoader.RestoreItemInstance(_lastTrashedItem);
        }

        public void SoftReset()
        {
            _offsets = new StatOffset();
            _offsetTriggers.Clear();
        }

        private void SettleStats(int count)
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
        public void LevelUp(int count = 1)
        {
            SoftReset();
            for (uint i = 0; i < count; i++)
            {
                HeldLevel++;
                SettleStats(HeldLevel / 2 + 1);
                HeldSkillPoints += HeldLevel < 5 ? 1 : HeldLevel / 5;
            }
        }
        public void GainExp(int exp)
        {
            HeldExp += exp;
            while (HeldExp > NextExp)
            {
                HeldExp -= NextExp;
                NextExp = (int) (1.25 * NextExp);
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

        public StatOffset GetArmorOffset()
        {
            var offset = new StatOffset();
            return Equipment.Where(e => !(e is null)).Aggregate(offset, (current, e) => current + e.Stats);
        }
        public StatOffset GetTrueStats()
        {
            return _stats + (_offsets + GetArmorOffset());
        }
        public float GetHealthAsPercentage()
        {
            var total = GetTrueStats();
            return (float)total.Hp / total.MaxHp;
        }
        public float GetManaAsPercentage()
        {
            var total = GetTrueStats();
            return (float)total.Mp / total.MaxMp;
        }
        public float GetExpAsPercentage()
        {
            return (float) HeldExp / NextExp;
        }

        public void AdjustHPMP(int hp, int mp)
        {
            Dirty = true;
            _offsets.Hp += hp;
            _offsets.Mp += mp;
            if (_offsets.Hp > 0) _offsets.Hp = 0;
            if (_offsets.Mp > 0) _offsets.Mp = 0;
        }
        public void SetHPMP(int hp = -1, int mp = -1)
        {
            Dirty = true;
            var total = GetTrueStats();
            if (hp != -1) _offsets.Hp = hp - total.MaxHp;
            if (mp != -1) _offsets.Mp = mp - total.MaxMp;
        }
        public void AdjustInstant(InstantItem.InstantItemStats stats)
        {
            TapGold(stats.gold);
            GainExp(stats.exp);
            AdjustHPMP(stats.hp, stats.mp);
        }

        public int GetFirstOpenInventorySlot()
        {
            for (int i = 0; i < Items.Length; i++)
            {
                if (Items[i] is null) return i;
            }

            return -1;
        }
        public bool TryAddItem(ItemInstance item)
        {
            if (item is InstantInstance inst)
            {
                inst.Trigger(this);
                return true;
            }

            var slot = GetFirstOpenInventorySlot();
            if (slot == -1) return false;
            Items[slot] = item;

            Dirty = true;
            return true;
        }

        public bool CanRestoreTrashedItem()
        {
            return _lastTrashedItem != null && GetFirstOpenInventorySlot() != -1;
        }
        public bool TryRestoreTrashedItem()
        {
            if (_lastTrashedItem == null) return false;
            var b = TryAddItem(_lastTrashedItem);
            if (b) _lastTrashedItem = null;
            return b;
        }
        public void RemoveItem(int slot, bool equipSlot, bool trash=false)
        {
            if (trash)
            {
                if (equipSlot)
                {
                    _lastTrashedItem = Equipment[slot];
                    Equipment[slot] = null;
                }
                else
                {
                    _lastTrashedItem = Items[slot];

                    Items[slot] = null;
                }
            }
            else
            {
                if (equipSlot) Equipment[slot] = null;
                else Items[slot] = null;
            }

            Dirty = true;
        }

        public bool CanGrabItem(int heldSlot, bool heldEquipSlot)
        {
            return heldEquipSlot ? Equipment[heldSlot] != null : Items[heldSlot] != null;
        }
        public bool TrySwapItem(int heldSlot, int destSlot, bool heldEquipSlot, bool destEquipSlot)
        {
            if (!CanGrabItem(heldSlot, heldEquipSlot)) return false;
            if (heldEquipSlot && destEquipSlot) return false;
            if (heldSlot == destSlot && !heldEquipSlot && !destEquipSlot) return false;
            var heldItem = heldEquipSlot ? Equipment[heldSlot] : Items[heldSlot];

            if (destEquipSlot)
            {
                //Equip an item from inventory
                if (heldItem is EquipInstance heldEquip && heldEquip.Type == (EquipItem.SlotType) destSlot)
                {
                    Items[heldSlot] = Equipment[destSlot];
                    Equipment[destSlot] = heldEquip;
                    Dirty = true;
                    return true;
                }

                return false;
            }

            if (heldEquipSlot)
            {
                //Unequip an item from equipment
                if (!(heldItem is EquipInstance heldEquip)) return false;
                var destItem = Items[destSlot] as EquipInstance;
                if (Items[destSlot] is null || (destItem != null && destItem.Type == heldEquip.Type))
                {
                    Equipment[heldSlot] = destItem;
                    Items[destSlot] = heldItem;
                    Dirty = true;
                    return true;
                }

                return false;
            }

            //Swap two items in inventory
            Items[heldSlot] = Items[destSlot];
            Items[destSlot] = heldItem;
            Dirty = true;
            return true;
        }

        public void AddTriggeredOffset(int turns, StatOffset diff)
        {
            _offsetTriggers.Add(new Tuple<int, StatOffset>(turns, diff));
            _offsets += diff;
        }
        public void PassTurn(int count=1)
        {
            for (int i = 0; i < count; i++)
            {
                for (int j = _offsetTriggers.Count-1; j>=0; j++)
                {
                    if (_offsetTriggers[j].Item1 == 1)
                    {
                        _offsets -= _offsetTriggers[j].Item2;
                        _offsetTriggers.RemoveAt(j);
                    }
                    else _offsetTriggers[j] = new Tuple<int, StatOffset>(_offsetTriggers[j].Item1-1, _offsetTriggers[j].Item2);
                }
            }
        }

        public override string ToString()
        {
            var total = GetTrueStats();
            return "Level: " + HeldLevel + "\n" +
                   "Gold:  " + HeldGold + "\n" +
                   "EXP:   " + HeldExp + "/" + NextExp + "\n" +
                   "HP:    " + total.Hp + "/" + total.MaxHp + "\n" +
                   "MP:    " + total.Mp + "/" + total.MaxMp + "\n" +
                   "ATK:   " + total.Atk + "\n" +
                   "DEF:   " + total.Def + "\n" +
                   "LUCK:  " + total.Luck;
        }
    }
}
