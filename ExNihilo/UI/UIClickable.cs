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

        public UIClickable(string path, Vector2 relPos, string altPath = "", PositionType t = PositionType.Center, float multiplier = 1.0f,
            bool pixelOffset = false, bool mulligan = false) : base(path, relPos, t, multiplier, pixelOffset)
        {
            AltTexturePath = altPath;
            _allowMulligan = mulligan;
        }

        public override void LoadContent(GraphicsDevice graphics, ContentManager content)
        {
            base.LoadContent(graphics, content);

            if (AltTexturePath.Length > 0) AltTexture = TextureLookUp[AltTexturePath];

            //Get alpha spread
            var data = new Color[Texture.Width * Texture.Height];
            Texture.GetData(data);
            Alpha = new byte[data.Length];
            for (int i = 0; i < data.Length; i++) Alpha[i] = data[i].A;
        }

        public override void Draw(SpriteBatch spriteBatch, Color color)
        {
            if (!Loaded) return;
            if (Activated && AltTexture != null)
                spriteBatch.Draw(AltTexture, Pos, null, color, 0, Vector2.Zero, SizeMult, SpriteEffects.None, 0);
            else
                spriteBatch.Draw(Texture, Pos, null, color, 0, Vector2.Zero, SizeMult, SpriteEffects.None, 0);
        }

        public virtual bool IsOver(Point mousePos)
        {
            int buttonX = (int)Math.Round((mousePos.X - Pos.X) / SizeMult);
            int buttonY = (int)Math.Round((mousePos.Y - Pos.Y) / SizeMult);
            if (buttonX < 0 || buttonY < 0 || buttonX >= BaseSize.X || buttonY >= BaseSize.Y) return false;
            bool rtrn = TexturePath=="null" || Alpha[buttonY * Texture.Width + buttonX] != 0;
            if (Activated && !rtrn && _allowMulligan) Activated = false;
            return rtrn;
        }

        public virtual void OnMoveMouse(Point point)
        {
           // if (activated) activated = IsOver(point);
        }

        public virtual void OnLeftClick(Point point)
        {
            Activated = IsOver(point);
        }

        public virtual void OnLeftRelease()
        {
            Activated = false;
        }
    }
}
