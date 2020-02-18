using System;
using System.Drawing;
using ExNihilo.Entity;
using ExNihilo.Util.Graphics;
using Microsoft.Xna.Framework.Graphics;
using Color = Microsoft.Xna.Framework.Color;

namespace ExNihilo.Systems.Bases
{
    public abstract class Item
    {
        public abstract class ItemInstance
        {
            public readonly int Level;
            public string Name { get; protected set; }
            public readonly Texture2D Texture;
            public Color IconColor { get; protected set; }
            //public Action<PlayerEntityContainer> Perform;

            protected ItemInstance(Item item, int level)
            {
                Level = level;
                Name = item.Name;
                Texture = item.Texture;
                IconColor = item.IconColor;
            }
        }

        public enum ItemType
        {
            Equip,   //Equipment items (armor, weapon, accessory)
            Instant, //Activates as soon as it enters the inventory (ex. gold)
            Use      //An item that can be manually used (ex. potion)
        }
        public readonly ItemType Type;

        public string Name { get; protected set; }
        public float Chance { get; protected set; }
        public Texture2D Texture { get; protected set; }
        public ColorScale IconColor { get; protected set; }

        protected Item(ItemType type)
        {
            Type = type;
        }

    }
}
