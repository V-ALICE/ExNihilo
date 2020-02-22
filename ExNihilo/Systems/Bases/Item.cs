using System;
using System.Diagnostics.Eventing.Reader;
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
        public Texture2D Texture { get; private set; }

        protected readonly int Quality;
        public ColorScale QualityColor { get; private set; }
        protected readonly string ColorName;
        public ColorScale IconColor { get; protected set; }

       //These must have 11 elements. Note last element represents perfection
        public static readonly string[] ModifierSet =
        {
            //Colors: Fine = green, Grand = blue, Legendary = yellow/orange, Mythical = purple, Absolute = rainbow
            "Broken ", "Damaged ", "Shabby ", "Basic ", "", "", "Fine ", "Grand ", "Legendary ", "Mythical ", "Absolute "
        };

        [OnDeserialized]
        internal void OnDeserialize(StreamingContext context)
        {
           if (ColorName.Length > 0) IconColor = ColorScale.GetFromGlobal(ColorName);
           if (Quality < 3) QualityColor = Color.DarkRed;
           else if (Quality == 6) QualityColor = Color.ForestGreen;
           else if (Quality == 7) QualityColor = Color.DeepSkyBlue;
           else if (Quality == 8) QualityColor = Color.DarkOrange;
           else if (Quality == 9) QualityColor = Color.MediumPurple;
           else if (Quality == 10) QualityColor = ColorScale.GetFromGlobal("Rainbow");
           else QualityColor = Color.Black;
        }

        //TODO: the way this is set up, saving will make a lot of duped image files(?) or no images
        public void Restore(Item reference)
        {
            Texture = reference.Texture;
        }

        public string GetFullName()
        {
            return ModifierSet[Quality] + Name;
        }
        public virtual string GetSmartDesc()
        {
            return ModifierSet[Quality] + "@c1" + Name + "\n";
        }
        public virtual Color[] GetSmartColors(Color basic)
        {
            return new Color[] {QualityColor, basic};
        }
        //public abstract int GetPrice();

        protected ItemInstance(Item item, int level, int quality)
        {
            Level = level;
            Name = item.Name;
            Texture = item.Texture;
            IconColor = item.IconColor;
            ColorName = item.IconColorLookup;
            Quality = quality;

            if (Quality < 3) QualityColor = Color.DarkRed;
            else if (Quality == 6) QualityColor = Color.ForestGreen;
            else if (Quality == 7) QualityColor = Color.DeepSkyBlue;
            else if (Quality == 8) QualityColor = Color.DarkOrange;
            else if (Quality == 9) QualityColor = Color.MediumPurple;
            else if (Quality == 10) QualityColor = ColorScale.GetFromGlobal("Rainbow");
            else QualityColor = Color.Black;
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
