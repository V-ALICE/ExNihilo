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

        protected PositionType type;
        protected Texture2D texture;
        protected Vector2 pos;
        protected Coordinate baseSize;
        protected readonly Vector2 posRel;
        protected readonly float sizeMult;
        protected readonly string texturePath;
        protected readonly bool absoluteOffset;
        protected bool loaded;

        public UIElement(string path, Vector2 relPos, float multiplier, PositionType t, bool absolute)
        {
            ExceptionCheck.AssertCondition(absolute || (relPos.X >= 0 && relPos.X <= 1.0 && relPos.Y >= 0 && relPos.Y <= 1.0));
            texturePath = path;
            posRel = Utilities.Copy(relPos);
            sizeMult = multiplier;
            type = t;
            absoluteOffset = absolute;
        }

        public virtual void LoadContent(GraphicsDevice graphics, ContentManager content)
        {
            loaded = true;
            texture = TextureLookUp[texturePath];
            if (baseSize is null) baseSize = new Coordinate(texture.Width, texture.Height);
        }

        public virtual void OnResize(GraphicsDevice graphics, Coordinate window, Vector2 origin)
        {
            if (!loaded) return;
            var offset = new Vector2();
            switch (type)
            {
                case PositionType.TopLeft:
                    offset = new Vector2(0, 0);
                    break;
                case PositionType.TopRight:
                    offset = new Vector2(baseSize.X, 0);
                    break;
                case PositionType.BottomLeft:
                    offset = new Vector2(0, baseSize.Y);
                    break;
                case PositionType.BottomRight:
                    offset = new Vector2(baseSize.X, baseSize.Y);
                    break;
                case PositionType.CenterTop:
                    offset = new Vector2(baseSize.X / 2, 0);
                    break;
                case PositionType.CenterBottom:
                    offset = new Vector2(baseSize.X / 2, baseSize.Y);
                    break;
                case PositionType.CenterLeft:
                    offset = new Vector2(0, baseSize.Y/2);
                    break;
                case PositionType.CenterRight:
                    offset = new Vector2(baseSize.X, baseSize.Y / 2);
                    break;
                case PositionType.Center:
                    offset = new Vector2(baseSize.X / 2, baseSize.Y / 2);
                    break;
            }

            pos = absoluteOffset ? origin + posRel - offset : origin + window * posRel - offset;
        }

        public virtual void Draw(SpriteBatch spriteBatch)
        {
            Draw(spriteBatch, Color.White);
        }
        public virtual void Draw(SpriteBatch spriteBatch, Color color)
        {
            if (!loaded) return;
            spriteBatch.Draw(texture, pos, null, color, 0, Vector2.Zero, sizeMult, SpriteEffects.None, 0);
        }
    }
}
