using System;
using ExNihilo.Systems.Backend;
using ExNihilo.Systems.Game;
using ExNihilo.Util;
using ExNihilo.Util.Graphics;
using Microsoft.Xna.Framework.Graphics;

namespace ExNihilo.Entity
{
    public class EntityContainer
    {
        public EntityTexture Entity;
        public AnimatableTexture Texture => Entity.CurrentTexture;
        public string Name;

        public EntityContainer(GraphicsDevice graphics, string name, Texture2D sheet, int zeroIndexStaticColumn = 1)
        {
            Name = name;
            Entity = new EntityTexture(graphics, sheet, zeroIndexStaticColumn);
        }

        protected EntityContainer(string name)
        {
            Name = name;
        }

        public void Push(Coordinate push)
        {
            Entity.ApplyMovementUpdate(push);
        }
    }

    public class PlayerEntityContainer : EntityContainer
    {
        [Serializable]
        public class PackedPlayerEntityContainer
        {
            public int CharIndex;
            public string Name, CharSet;
            public Inventory Inventory;
        }

        public readonly string CharSet;
        public readonly int CharIndex;
        public readonly Inventory Inventory;

        public PlayerEntityContainer(GraphicsDevice graphics, string name, string texSet, int texIndex, Inventory inv=null) : base(name)
        {
            Entity = new EntityTexture(graphics, TextureLibrary.CharLookup(texSet, texIndex), 1);
            CharSet = texSet;
            CharIndex = texIndex;
            Inventory = inv ?? new Inventory();
        }

        public override string ToString()
        {
            return Name + " the Adventurer\n" + Inventory;
        }

        public PackedPlayerEntityContainer GetPacked()
        {
            return new PackedPlayerEntityContainer {Name = Name, CharSet = CharSet, CharIndex = CharIndex, Inventory = Inventory};
        }
    }
}
