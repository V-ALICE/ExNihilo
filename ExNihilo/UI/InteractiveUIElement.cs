using System;
using ExNihilo.Input.Controllers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace ExNihilo.UI
{
    public class InteractiveUIElement : UIElement, IClickable
    {
        protected byte[] alpha;
        protected Color altColor;
        protected Texture2D altTexture;
        protected readonly string altTexturePath;
        protected bool activated;

        public InteractiveUIElement(string path, Vector2 relPos, string altPath="", float multiplier = 1.0f, PositionType t = PositionType.Center) : base(path, relPos, multiplier, t)
        {
            altTexturePath = altPath;
            altColor = Color.White;
        }
        public InteractiveUIElement(string path, Vector2 relPos, Color colorAlt, float multiplier = 1.0f, PositionType t = PositionType.Center) : base(path, relPos, multiplier, t)
        {
            altColor = colorAlt;
            altTexturePath = "";
        }

        public override void LoadContent(GraphicsDevice graphics, ContentManager content)
        {
            base.LoadContent(graphics, content);

            if (altTexturePath.Length > 0) altTexture = content.Load<Texture2D>(altTexturePath);

            //Get alpha spread
            var _data = new Color[texture.Width * texture.Height];
            texture.GetData(_data);
            alpha = new byte[_data.Length];
            for (int i = 0; i < _data.Length; i++) alpha[i] = _data[i].A;
        }

        public override void Draw(SpriteBatch spriteBatch, Color color)
        {
            if (!loaded) return;
            if (activated && altTexture != null)
                spriteBatch.Draw(altTexture, pos, null, color, 0, Vector2.Zero, sizeMult, SpriteEffects.None, 0);
            else if (activated)
                spriteBatch.Draw(texture, pos, null, altColor, 0, Vector2.Zero, sizeMult, SpriteEffects.None, 0);
            else
                spriteBatch.Draw(texture, pos, null, color, 0, Vector2.Zero, sizeMult, SpriteEffects.None, 0);
        }

        public bool IsOver(Point mousePos)
        {
            int buttonX = (int)Math.Round((mousePos.X - pos.X) / sizeMult);
            int buttonY = (int)Math.Round((mousePos.Y - pos.Y) / sizeMult);
            if (buttonX < 0 || buttonY < 0 || buttonX >= texture.Width || buttonY >= texture.Height) return false;
            bool rtrn = alpha[buttonY * texture.Width + buttonX] != 0;
            return rtrn;
        }

        public void OnMoveMouse(Point point)
        {
            if (activated) activated = IsOver(point);
        }

        public void OnLeftClick(Point point)
        {
            activated = IsOver(point);
        }

        public void OnLeftRelease()
        {
            activated = false;
        }
    }
}
