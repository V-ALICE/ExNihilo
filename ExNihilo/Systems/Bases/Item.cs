using System;
using System.Runtime.Serialization;
using ExNihilo.Util.Graphics;
using Color = Microsoft.Xna.Framework.Color;

namespace ExNihilo.Systems.Bases
{
    [Serializable]
    public abstract class ItemInstance
    {
        public readonly string BaseUID;
        public readonly int UID;
        public readonly int Level;
        public string Name { get; protected set; }
        [NonSerialized] protected AnimatableTexture Texture;

        protected readonly int Quality;
        [NonSerialized] private ColorScale QualityColor;
        protected string ColorName;
        protected int r, g, b, a;
        [NonSerialized] protected ColorScale IconColor;

       //This must have 11 elements. Note last element represents perfection
        public static readonly string[] ModifierSet =
        {
            //Colors: Fine = green, Grand = blue, Legendary = yellow/orange, Mythical = purple, Absolute = rainbow
            "Broken ", "Damaged ", "Shabby ", "", "", "", "Fine ", "Grand ", "Legendary ", "Mythical ", "Absolute "
        };

        [OnDeserialized]
        internal void OnDeserialized(StreamingContext context)
        {
           if (Quality < 3) QualityColor = ColorScale.VioletRed;
           else if (Quality == 6) QualityColor = ColorScale.Green;
           else if (Quality == 7) QualityColor = ColorScale.BlueViolet;
           else if (Quality == 8) QualityColor = ColorScale.OrangeYellow;
           else if (Quality == 9) QualityColor = ColorScale.Violet;
           else if (Quality == 10) QualityColor = ColorScale.GetFromGlobal("Rainbow");
           else QualityColor = ColorScale.Black;

           IconColor = ColorName.Length > 0 ? ColorScale.GetFromGlobal(ColorName) : (ColorScale)new Color(r, g, b, a);
        }

        public void Restore(Item reference)
        {
            Texture = reference.Texture;
        }

        public AnimatableTexture GetTexture()
        {
            return Texture;
        }

        public ColorScale GetIconColor()
        {
            return IconColor;
        }

        public ColorScale GetQualityColor()
        {
            return QualityColor;
        }

        public string GetFullName()
        {
            return ModifierSet[Quality] + Name;
        }
        public virtual string GetSmartDesc()
        {
            return ModifierSet[Quality] + "@c1" + Name + " (rank " + Level + ")\n";
        }
        public virtual ColorScale[] GetSmartColors(ColorScale basic)
        {
            return new[] {QualityColor, basic};
        }
        //public abstract int GetPrice();

        protected ItemInstance(Item item, int level, int quality, int uniqueID)
        {
            BaseUID = item.UID;
            UID = uniqueID;
            Level = level;
            Name = item.Name;
            Texture = item.Texture;
            IconColor = item.IconColor;
            ColorName = item.IconColorLookup;
            r = item.IconColor.R;
            g = item.IconColor.G;
            b = item.IconColor.B;
            a = item.IconColor.A;
            Quality = quality;

            if (Quality < 3) QualityColor = ColorScale.VioletRed;
            else if (Quality == 6) QualityColor = ColorScale.Green;
            else if (Quality == 7) QualityColor = ColorScale.BlueViolet;
            else if (Quality == 8) QualityColor = ColorScale.OrangeYellow;
            else if (Quality == 9) QualityColor = ColorScale.Violet;
            else if (Quality == 10) QualityColor = ColorScale.GetFromGlobal("Rainbow");
            else QualityColor = ColorScale.Black;
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
        public AnimatableTexture Texture { get; protected set; }
        public Color IconColor { get; protected set; }
        public string IconColorLookup { get; protected set; }
        public string UID { get; protected set; }

        protected Item(ItemType type)
        {
            Type = type;
            IconColorLookup = "";
            IconColor = new Color();
        }

    }
}
