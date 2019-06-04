using ExNihilo.UI.Bases;
using ExNihilo.Util;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace ExNihilo.UI
{
    public class UIElement : UILibrary, IUI
    {
        public enum PositionType
        {
            TopLeft, TopRight,
            BottomLeft, BottomRight,
            CenterTop, CenterBottom,
            CenterLeft, CenterRight,
            Center,
        }

        protected PositionType Type;
        protected Texture2D Texture;
        protected Vector2 Pos;
        protected Coordinate BaseSize;
        protected Vector2 PosRel, TextureOffset;
        protected readonly float SizeMult;
        protected readonly string TexturePath;
        protected readonly bool AbsoluteOffset;
        protected bool Loaded;

        public UIElement(string path, Vector2 relPos, float multiplier, PositionType t, bool absolute)
        {
            ExceptionCheck.AssertCondition(absolute || (relPos.X >= 0 && relPos.X <= 1.0 && relPos.Y >= 0 && relPos.Y <= 1.0));
            TexturePath = path;
            PosRel = Utilities.Copy(relPos);
            SizeMult = multiplier;
            Type = t;
            AbsoluteOffset = absolute;
        }

        public virtual void LoadContent(GraphicsDevice graphics, ContentManager content)
        {
            Loaded = true;
            Texture = TextureLookUp[TexturePath];
            if (BaseSize is null) BaseSize = new Coordinate(Texture.Width, Texture.Height);
            TextureOffset = GetOffset();
        }

        protected Vector2 GetOffset()
        {
            switch (Type)
            {
                case PositionType.TopLeft:
                    return new Vector2(0, 0);
                case PositionType.TopRight:
                    return new Vector2(BaseSize.X, 0);
                case PositionType.BottomLeft:
                    return new Vector2(0, BaseSize.Y);
                case PositionType.BottomRight:
                    return new Vector2(BaseSize.X, BaseSize.Y);
                case PositionType.CenterTop:
                    return new Vector2(BaseSize.X / 2, 0);
                case PositionType.CenterBottom:
                    return new Vector2(BaseSize.X / 2, BaseSize.Y);
                case PositionType.CenterLeft:
                    return new Vector2(0, BaseSize.Y / 2);
                case PositionType.CenterRight:
                    return new Vector2(BaseSize.X, BaseSize.Y / 2);
                case PositionType.Center:
                    return new Vector2(BaseSize.X / 2, BaseSize.Y / 2);
            }

            return new Vector2();
        }
        public virtual void OnResize(GraphicsDevice graphics, Coordinate window, Vector2 origin)
        {
            if (!Loaded) return;

            Pos = AbsoluteOffset ? origin + PosRel - TextureOffset : origin + window * PosRel - TextureOffset;
        }

        public virtual void Draw(SpriteBatch spriteBatch)
        {
            Draw(spriteBatch, Color.White);
        }
        public virtual void Draw(SpriteBatch spriteBatch, Color color)
        {
            if (!Loaded) return;
            spriteBatch.Draw(Texture, Pos, null, color, 0, Vector2.Zero, SizeMult, SpriteEffects.None, 0);
        }
    }
}
