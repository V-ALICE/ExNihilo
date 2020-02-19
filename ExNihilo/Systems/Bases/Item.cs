using System;
using System.Runtime.Serialization;
using ExNihilo.Util.Graphics;
using Microsoft.Xna.Framework.Graphics;
using Color = Microsoft.Xna.Framework.Color;

namespace ExNihilo.Systems.Bases
{
    [Serializable]
    public abstract class ItemInstance
    {
        public readonly int Level;
        public string Name { get; protected set; }
        public Texture2D Texture { get; protected set; }
        
        protected readonly string ColorName;
        public ColorScale IconColor { get; protected set; }

        //public Action<PlayerEntityContainer> Perform;

        //TODO: the way this is set up, saving will make a lot of duped image files(?) or no images
        public void Restore(Item reference)
        {
            Texture = reference.Texture;
        }

        protected ItemInstance(Item item, int level)
        {
            Level = level;
            Name = item.Name;
            Texture = item.Texture;
            IconColor = item.IconColor;
            ColorName = item.IconColorLookup;
        }
    }

    public abstract class Item
    {
        public enum ItemType
        {
            Equip,   //Equipment items (armor, weapon, accessory)
            Instant, //Activates as soon as it enters the inventory (ex. gold)
            Use      //An item that can be manually used (ex. potion)
        }
        public readonly ItemType Type;

        public bool Valid { get; protected set; }
        public string Name { get; protected set; }
        public float Chance { get; protected set; }
        public Texture2D Texture { get; protected set; }
        public Color IconColor { get; protected set; }
        public string IconColorLookup { get; protected set; }

        protected Item(ItemType type)
        {
            Type = type;
        }

    }
}
