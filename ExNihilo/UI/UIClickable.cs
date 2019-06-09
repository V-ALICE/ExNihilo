using System;
using ExNihilo.UI.Bases;
using ExNihilo.Util;
using ExNihilo.Util.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace ExNihilo.UI
{
    public struct UICallbackPackage
    {
        public string caller;
        public float[] value;
        public Point screenPos;
        public Coordinate elementPos;

        public UICallbackPackage(string name, Point screen, Vector2 origin, params float[] values)
        {
            caller = name;
            value = values;
            screenPos = screen;
            elementPos = new Coordinate((int) Math.Round(screen.X-origin.X), (int) Math.Round(screen.Y-origin.Y));
        }
    }

    public class UIClickable : UIElement, IClickable
    {
        protected byte[] Alpha;
        protected ColorScale DisabledColor;
        protected AnimatableTexture DownTexture, OverTexture;
        protected readonly string DownTexturePath, OverTexturePath;
        protected readonly bool AllowMulligan;
        protected Action<UICallbackPackage> Function;
        
        public bool Disabled { get; protected set; }
        public bool Down { get; protected set; }
        public bool Over { get; protected set; }

        public UIClickable(string name, string path, Vector2 relPos, ColorScale color, UIPanel superior, TextureUtilities.PositionType anchorPoint, 
            string downPath = "", string overPath ="", bool mulligan = false) : base(name, path, relPos, color, superior, anchorPoint)
        {
            DownTexturePath = downPath;
            OverTexturePath = overPath;
            AllowMulligan = mulligan;
            DisabledColor = color;
        }

        public UIClickable(string name, string path, Coordinate pixelOffset, ColorScale color, UIElement superior, TextureUtilities.PositionType anchorPoint, 
            TextureUtilities.PositionType superAnchorPoint, string downPath = "", string overPath = "", bool mulligan = false) : 
            base(name, path, pixelOffset, color, superior, anchorPoint, superAnchorPoint)
        {
            DownTexturePath = downPath;
            OverTexturePath = overPath;
            AllowMulligan = mulligan;
            DisabledColor = color;
        }

        protected UIClickable(string name, Vector2 relPos, TextureUtilities.PositionType anchorPoint) : base(name, relPos, anchorPoint)
        {
            AllowMulligan = false;
            DownTexturePath = "";
            OverTexturePath = "";
        }

        public void RegisterCallback(Action<UICallbackPackage> action)
        {
            Function = action;
        }

        public override void LoadContent(GraphicsDevice graphics, ContentManager content)
        {
            base.LoadContent(graphics, content);

            Alpha = UILibrary.TextureAlphaLookUp[TexturePath];
            if (DownTexturePath.Length > 0) DownTexture = UILibrary.TextureLookUp[DownTexturePath];
            if (OverTexturePath.Length > 0) OverTexture = UILibrary.TextureLookUp[OverTexturePath];
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (!Loaded) return;
            if (Down && DownTexture != null) DownTexture.Draw(spriteBatch, OriginPosition, ColorScale?.Get() ?? Color.White, CurrentScale);
            else if (Over && OverTexture != null) OverTexture.Draw(spriteBatch, OriginPosition, ColorScale?.Get() ?? Color.White, CurrentScale);
            else if (Disabled) Texture.Draw(spriteBatch, OriginPosition, DisabledColor?.Get() ?? Color.White, CurrentScale);
            else Texture.Draw(spriteBatch, OriginPosition, ColorScale?.Get() ?? Color.White, CurrentScale);
        }

        public virtual bool IsOver(Point mousePos)
        {
            if (TexturePath == "null") return false;
            int buttonX = (int) (Math.Round(mousePos.X - OriginPosition.X)/CurrentScale);
            int buttonY = (int) (Math.Round(mousePos.Y - OriginPosition.Y)/CurrentScale);
            if (buttonX < 0 || buttonY < 0 || buttonX >= Texture.Width || buttonY >= Texture.Height) return false;
            return Alpha[buttonY * Texture.Width + buttonX] != 0;
        }

        public virtual void OnMoveMouse(Point point)
        {
            if (Disabled) return;
            if ((AllowMulligan && Down) || (!Down && OverTexture != null))
            {
                var isOver = IsOver(point);
                if (Down && AllowMulligan) Down = isOver;
                if (!Down && OverTexture != null) Over = isOver;
            }
        }

        public virtual bool OnLeftClick(Point point)
        {
            if (Disabled) return false;
            Down = IsOver(point);
            if (Down) Over = false;
            return Down;
        }

        public virtual void OnLeftRelease(Point point)
        {
            if (Disabled) return;
            if (Down) Function?.Invoke(new UICallbackPackage(GivenName, point, OriginPosition));
            Down = false;
        }

        public virtual void Disable(ColorScale c)
        {
            Down = false;
            Over = false;
            DisabledColor = c;
            Disabled = true;
        }

        public virtual void Enable()
        {
            Disabled = false;
        }
    }
}
