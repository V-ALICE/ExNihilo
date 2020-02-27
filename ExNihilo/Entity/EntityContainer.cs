using System;
using ExNihilo.Systems.Backend;
using ExNihilo.Systems.Game;
using ExNihilo.Util;
using ExNihilo.Util.Graphics;
using Microsoft.Xna.Framework.Graphics;

namespace ExNihilo.Entity
{
    public abstract class EntityContainer
    {
        public EntityTexture Entity;
        public AnimatableTexture Texture => Entity.CurrentTexture;
        public string Name;

        protected EntityContainer(GraphicsDevice graphics, string name, Texture2D sheet, int zeroIndexStaticColumn = 1)
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
            public int[] TextureSet;
            public string Name;
            public Inventory Inventory;
        }

        private readonly int[] _textureSet;
        public readonly Inventory Inventory;

        public PlayerEntityContainer(GraphicsDevice graphics, string name, int body, int hair, int cloth, int color, Inventory inv=null) : base(name)
        {
            var bodySheet = TextureLibrary.Lookup("Char/base/" + (body + 1));
            var hairSheet = TextureLibrary.Lookup("Char/hair/" + (hair + 1) + "-" + (color + 1));
            var clothSheet = TextureLibrary.Lookup("Char/cloth/" + (cloth + 1));
            Entity = new EntityTexture(graphics, TextureUtilities.CombineTextures(graphics, bodySheet, clothSheet, hairSheet), 1);
            _textureSet = new[] {body, hair, cloth, color};
            Inventory = inv ?? new Inventory();
        }

        public override string ToString()
        {
            return Name + " the Adventurer\n" + Inventory;
        }

        public PackedPlayerEntityContainer GetPacked()
        {
            return new PackedPlayerEntityContainer {Name = Name, TextureSet = _textureSet, Inventory = Inventory};
        }
    }
}
