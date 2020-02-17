using System;
using ExNihilo.Entity;

namespace ExNihilo.Systems.Bases
{
    public abstract class Item
    {
        public enum ItemType
        {
            Equip,   //Equipment items (armor, weapon, accessory)
            Instant, //Activates as soon as it enters the inventory (ex. gold)
            Use      //An item that can be manually used (ex. potion)
        }
        public readonly ItemType Type;
        public Action<PlayerEntityContainer> Perform { get; protected set; }

        public string Name { get; protected set; }
        public string Description { get; protected set; }

        protected Item(ItemType type)
        {
            Type = type;
        }
    }
}
