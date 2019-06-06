using System;
using ExNihilo.UI.Bases;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace ExNihilo.UI
{
    public class UIClickable : UIElement, IClickable
    {
        protected byte[] Alpha;
        protected Texture2D AltTexture;
        protected readonly string AltTexturePath;
        private readonly bool _allowMulligan;
        public bool Activated { get; protected set; }

        public UIClickable(string path, Vector2 relPos, string altPath = "", PositionType t = PositionType.Center, 
            bool pixelOffset = false, bool mulligan = false) : base(path, relPos, t, pixelOffset)
        {
            AltTexturePath = altPath;
            _allowMulligan = mulligan;
        }

        public override void LoadContent(GraphicsDevice graphics, ContentManager content)
        {
            base.LoadContent(graphics, content);

            Alpha = UILibrary.TextureAlphaLookUp[TexturePath];
            if (AltTexturePath.Length > 0) AltTexture = UILibrary.TextureLookUp[AltTexturePath];
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (!Loaded) return;
            if (Activated && AltTexture != null)
                spriteBatch.Draw(AltTexture, Pos, null, Color.White, 0, Vector2.Zero, CurrentScale, SpriteEffects.None, 0);
            else
                spriteBatch.Draw(Texture, Pos, null, Color.White, 0, Vector2.Zero, CurrentScale, SpriteEffects.None, 0);
        }

        public virtual bool IsOver(Point mousePos)
        {
            int buttonX = (int)Math.Round(mousePos.X - Pos.X);
            int buttonY = (int)Math.Round(mousePos.Y - Pos.Y);
            if (buttonX < 0 || buttonY < 0 || buttonX >= BaseSize.X || buttonY >= BaseSize.Y) return false;
            var arrayPos = (int) MathHelper.Clamp((buttonY * Texture.Width + buttonX) / CurrentScale, 0, Alpha.Length-1); //TODO: figure out why this was actually breaking
            bool rtrn = TexturePath=="null" || Alpha[arrayPos] != 0;
            return rtrn;
        }

        public virtual void OnMoveMouse(Point point)
        {
            //mulligans only check the size/alpha of the original texture
            if (Activated && _allowMulligan) Activated = IsOver(point);
        }

        public virtual bool OnLeftClick(Point point)
        {
            Activated = IsOver(point);
            return Activated;
        }

        public virtual void OnLeftRelease()
        {
            Activated = false;
        }
    }
}
