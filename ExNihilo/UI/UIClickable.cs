using System;
using ExNihilo.UI.Bases;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace ExNihilo.UI
{
    public class UIClickable : UIElement, IClickable
    {
        protected byte[] alpha;
        protected Texture2D altTexture;
        protected readonly string altTexturePath;
        public bool Activated { get; protected set; }

        public UIClickable(string path, Vector2 relPos, string altPath = "", bool absolute = false, float multiplier = 1.0f,
            PositionType t = PositionType.Center) : base(path, relPos, multiplier, t, absolute)
        {
            altTexturePath = altPath;
        }

        public override void LoadContent(GraphicsDevice graphics, ContentManager content)
        {
            base.LoadContent(graphics, content);

            if (altTexturePath.Length > 0) altTexture = TextureLookUp[altTexturePath];

            //Get alpha spread
            var _data = new Color[texture.Width * texture.Height];
            texture.GetData(_data);
            alpha = new byte[_data.Length];
            for (int i = 0; i < _data.Length; i++) alpha[i] = _data[i].A;
        }

        public override void Draw(SpriteBatch spriteBatch, Color color)
        {
            if (!loaded) return;
            if (Activated && altTexture != null)
                spriteBatch.Draw(altTexture, pos, null, color, 0, Vector2.Zero, sizeMult, SpriteEffects.None, 0);
            else
                spriteBatch.Draw(texture, pos, null, color, 0, Vector2.Zero, sizeMult, SpriteEffects.None, 0);
        }

        public bool IsOver(Point mousePos)
        {
            int buttonX = (int)Math.Round((mousePos.X - pos.X) / sizeMult);
            int buttonY = (int)Math.Round((mousePos.Y - pos.Y) / sizeMult);
            if (buttonX < 0 || buttonY < 0 || buttonX >= baseSize.X || buttonY >= baseSize.Y) return false;
            bool rtrn = texturePath=="null" || alpha[buttonY * texture.Width + buttonX] != 0;
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
