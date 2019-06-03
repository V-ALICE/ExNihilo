using System.Collections.Generic;
using ExNihilo.Util;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace ExNihilo.UI
{
    public class UIElement : IUI
    {
        public enum PositionType
        {
            TopLeft, TopRight,
            BottomLeft, BottomRight,
            CenterTop, CenterBottom,
            CenterLeft, CenterRight,
            Center
        }

        protected static Dictionary<string, Texture2D> TextureLookUp = new Dictionary<string, Texture2D>(); //TODO: this doesn't really work with sheets, only single textures

        protected PositionType type;
        protected Texture2D texture;
        protected Vector2 pos;
        protected readonly Vector2 posRel;
        protected readonly float sizeMult;
        protected readonly string texturePath;
        protected bool loaded;

        public UIElement(string path, Vector2 relPos, float multiplier, PositionType t)
        {
            ExceptionCheck.AssertCondition(relPos.X >= 0 && relPos.X <= 1.0 && relPos.Y >= 0 && relPos.Y <= 1.0);
            texturePath = path;
            posRel = Utilities.Copy(relPos);
            sizeMult = multiplier;
            type = t;
        }

        public virtual void LoadContent(GraphicsDevice graphics, ContentManager content)
        {
            loaded = true;
            texture = content.Load<Texture2D>(texturePath);
            
        }

        public virtual void OnResize(GraphicsDevice graphics, Coordinate window)
        {
            if (!loaded) return;
            switch (type)
            {
                case PositionType.TopLeft:
                    pos = new Vector2(posRel.X * window.X, posRel.Y * window.Y);
                    break;
                case PositionType.TopRight:
                    pos = new Vector2(posRel.X * window.X - texture.Width, posRel.Y * window.Y);
                    break;
                case PositionType.BottomLeft:
                    pos = new Vector2(posRel.X * window.X, posRel.Y * window.Y - texture.Height);
                    break;
                case PositionType.BottomRight:
                    pos = new Vector2(posRel.X * window.X - texture.Width, posRel.Y * window.Y - texture.Height);
                    break;
                case PositionType.CenterTop:
                    pos = new Vector2(posRel.X * window.X - texture.Width / 2, posRel.Y * window.Y);
                    break;
                case PositionType.CenterBottom:
                    pos = new Vector2(posRel.X * window.X - texture.Width / 2, posRel.Y * window.Y - texture.Height);
                    break;
                case PositionType.CenterLeft:
                    pos = new Vector2(posRel.X * window.X, posRel.Y * window.Y - texture.Height / 2);
                    break;
                case PositionType.CenterRight:
                    pos = new Vector2(posRel.X * window.X - texture.Width, posRel.Y * window.Y - texture.Height / 2);
                    break;
                case PositionType.Center:
                    pos = new Vector2(posRel.X * window.X - texture.Width / 2, posRel.Y * window.Y - texture.Height / 2);
                    break;
            }
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
