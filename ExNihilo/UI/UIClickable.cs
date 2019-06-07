using System;
using ExNihilo.UI.Bases;
using ExNihilo.Util;
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
        protected Color CurrentColor;
        protected Action<string> Function;
        
        public bool Disabled { get; protected set; }
        public bool Activated { get; protected set; }

        public UIClickable(string name, string path, Vector2 relPos, UIPanel superior, PositionType anchorPoint, string altPath = "", 
            bool mulligan = false) : base(name, path, relPos, superior, anchorPoint)
        {
            AltTexturePath = altPath;
            _allowMulligan = mulligan;
            CurrentColor = Color.White;
        }

        public UIClickable(string name, string path, Coordinate pixelOffset, UIElement superior, PositionType anchorPoint, PositionType superAnchorPoint,
            string altPath = "", bool mulligan = false) : base(name, path, pixelOffset, superior, anchorPoint, superAnchorPoint)
        {
            AltTexturePath = altPath;
            _allowMulligan = mulligan;
            CurrentColor = Color.White;
        }

        protected UIClickable(string name, Vector2 relPos, PositionType anchorPoint) : base(name, relPos, anchorPoint)
        {
            _allowMulligan = false;
            AltTexturePath = "";
        }

        public void RegisterCallback(Action<string> action)
        {
            Function = action;
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
                spriteBatch.Draw(AltTexture, OriginPosition, null, CurrentColor, 0, Vector2.Zero, CurrentScale, SpriteEffects.None, 0);
            else
                spriteBatch.Draw(Texture, OriginPosition, null, CurrentColor, 0, Vector2.Zero, CurrentScale, SpriteEffects.None, 0);
        }

        public virtual bool IsOver(Point mousePos)
        {
            int buttonX = (int)Math.Round(mousePos.X - OriginPosition.X);
            int buttonY = (int)Math.Round(mousePos.Y - OriginPosition.Y);
            if (buttonX < 0 || buttonY < 0 || buttonX >= CurrentPixelSize.X || buttonY >= CurrentPixelSize.Y) return false;
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
            if (Disabled) return false;
            Activated = IsOver(point);
            return Activated;
        }

        public virtual void OnLeftRelease()
        {
            if (Activated) Function?.Invoke(GivenName);
            Activated = false;
        }

        public virtual void Disable(Color c)
        {
            Activated = false;
            CurrentColor = c;
            Disabled = true;
        }

        public virtual  void Enable(Color c)
        {
            CurrentColor = c;
            Disabled = false;
        }
    }
}
