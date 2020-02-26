using System;
using ExNihilo.Systems;
using ExNihilo.Systems.Backend;
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
        public string Caller;
        public float[] Value;
        public Point ScreenPos;
        public Coordinate ElementPos;

        public UICallbackPackage(string name, Point screen, Coordinate origin, params float[] values)
        {
            Caller = name;
            Value = values;
            ScreenPos = screen;
            ElementPos = new Coordinate(screen.X-origin.X, screen.Y-origin.Y);
        }
    }

    public class UIClickable : UIElement, IClickable
    {
        protected byte[] Alpha;
        protected ColorScale DisabledColor, DownColor, OverColor;
        protected AnimatableTexture DownTexture, OverTexture;
        protected string DownTexturePath, OverTexturePath;
        protected readonly bool AllowMulligan;
        protected Action<UICallbackPackage> Function;
        
        public bool Disabled { get; protected set; }
        public bool Down { get; protected set; }
        public bool Over { get; protected set; }

        public UIClickable(string name, string path, Vector2 relPos, ColorScale color, UIPanel superior, TextureUtilities.PositionType anchorPoint, 
            bool mulligan = false) : base(name, path, relPos, color, superior, anchorPoint)
        {
            AllowMulligan = mulligan;
            DisabledColor = color;
            DownTexturePath = "";
            OverTexturePath = "";
        }

        public UIClickable(string name, string path, Coordinate pixelOffset, ColorScale color, UIElement superior, TextureUtilities.PositionType anchorPoint, 
            TextureUtilities.PositionType superAnchorPoint, bool mulligan = false) : base(name, path, pixelOffset, color, superior, anchorPoint, superAnchorPoint)
        {
            AllowMulligan = mulligan;
            DisabledColor = color;
            DownTexturePath = "";
            OverTexturePath = "";
        }

        protected UIClickable(string name, Vector2 relPos, TextureUtilities.PositionType anchorPoint) : base(name, relPos, anchorPoint)
        {
            AllowMulligan = false;
            DownTexturePath = "";
            OverTexturePath = "";
        }

        public virtual void RegisterCallback(Action<UICallbackPackage> action)
        {
            Function = action;
        }

        public void SetExtraStates(string downPath = "", string overPath = "", ColorScale down=null, ColorScale over =null)
        {
            DownColor = down;
            OverColor = over;
            DownTexturePath = downPath;
            OverTexturePath = overPath;
        }

        public override void LoadContent(GraphicsDevice graphics, ContentManager content)
        {
            base.LoadContent(graphics, content);

            Alpha = Texture.GetAlphaMask();
            if (DownTexturePath.Length > 0) DownTexture = TextureLibrary.Lookup(DownTexturePath);
            if (OverTexturePath.Length > 0) OverTexture = TextureLibrary.Lookup(OverTexturePath);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (!Loaded) return;
            if (Disabled)
            {
                Texture.Draw(spriteBatch, OriginPosition, DisabledColor?.Get() ?? ColorScale.Get(), CurrentScale);
            }
            else if (Down)
            {
                if (DownTexture != null) DownTexture.Draw(spriteBatch, OriginPosition, DownColor?.Get() ?? ColorScale.Get(), CurrentScale);
                else Texture.Draw(spriteBatch, OriginPosition, DownColor?.Get() ?? ColorScale.Get(), CurrentScale);
            }
            else if (Over)
            {
                if (OverTexture != null) OverTexture.Draw(spriteBatch, OriginPosition, OverColor?.Get() ?? ColorScale.Get(), CurrentScale);
                else Texture.Draw(spriteBatch, OriginPosition, OverColor?.Get() ?? ColorScale.Get(), CurrentScale);
            }
            else
            {
                Texture.Draw(spriteBatch, OriginPosition, ColorScale.Get(), CurrentScale);
            }
        }

        public virtual bool IsOver(Point mousePos)
        {
            if (Disabled) return false;
            if (TexturePath == "null") return false;
            int buttonX = (int) ((mousePos.X - OriginPosition.X)/CurrentScale);
            int buttonY = (int) ((mousePos.Y - OriginPosition.Y)/CurrentScale);
            if (buttonX < 0 || buttonY < 0 || buttonX >= Texture.Width || buttonY >= Texture.Height) return false;
            return Alpha[buttonY * Texture.Width + buttonX] != 0;
        }

        public void ForceNotOver()
        {
            Over = false;
        }
        public virtual bool OnMoveMouse(Point point)
        {
            if (Disabled) return false;
            if (AllowMulligan && Down)
            {
                Down = IsOver(point);
                return Down;
            }
            if ((OverTexture != null || OverColor != null))
            {
                Over = IsOver(point);
                return Over;
            }

            return false;
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
            if (Down)
            {
                Function?.Invoke(new UICallbackPackage(GivenName, point, OriginPosition));
                Over = IsOver(point);
            }
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

        public override void ChangeTexture(AnimatableTexture texture)
        {
            base.ChangeTexture(texture);
            Alpha = Texture.GetAlphaMask();
        }

        public override void ChangeTexture(string path)
        {
            base.ChangeTexture(path);
            Alpha = Texture.GetAlphaMask();
        }
    }
}
