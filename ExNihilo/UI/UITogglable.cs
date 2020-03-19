using System;
using ExNihilo.Util;
using ExNihilo.Util.Graphics;
using Microsoft.Xna.Framework;

namespace ExNihilo.UI
{
    public class UITogglable : UIClickable
    {
        protected bool WasOver;
        public bool DeactivateOnExternalClick;

        public UITogglable(string name, string path, Vector2 relPos, ColorScale color, UIPanel superior,
            TextureUtilities.PositionType anchorPoint, bool down, bool deactivateOnExternalClick = false,
            bool mulligan = false) : base(name, path, relPos, color, superior, anchorPoint, mulligan)
        {
            Down = down;
            DeactivateOnExternalClick = deactivateOnExternalClick;
        }

        public UITogglable(string name, string path, Coordinate pixelOffset, ColorScale color, UIElement superior, TextureUtilities.PositionType anchorPoint, 
            TextureUtilities.PositionType superAnchorPoint, bool down, bool deactivateOnExternalClick = false,
            bool mulligan=false) : base(name, path, pixelOffset, color, superior, anchorPoint, superAnchorPoint, mulligan)
        {
            Down = down;
            DeactivateOnExternalClick = deactivateOnExternalClick;
        }

        public void ForcePush(bool down, bool doAction=true)
        {
            Down = down;
            if (doAction) Function?.Invoke(new UICallbackPackage(GivenName, new Point(), OriginPosition, Down ? 1 : -1));
            WasOver = false;
            Over = false;
        }

        public override void RegisterCallback(Action<UICallbackPackage> action)
        {
            base.RegisterCallback(action);
            if (Down) Function?.Invoke(new UICallbackPackage(GivenName, new Point(), OriginPosition, Down ? 1 : -1));
        }

        public override bool OnMoveMouse(Point point)
        {
            if (Disabled) return false;
            if (AllowMulligan && WasOver)
            {
                var isOver = IsOver(point);
                if (WasOver && AllowMulligan) WasOver = isOver;
                return isOver;
            }
            if ((OverTexture != null || OverColor != null))
            {
                Over = IsOver(point);
                if (WasOver && AllowMulligan) WasOver = Over;
                return Over;
            }

            return false;
        }

        public override bool OnLeftClick(Point point)
        {
            if (Disabled) return false;
            WasOver = IsOver(point);
            return WasOver;
        }

        public override void OnLeftRelease(Point point)
        {
            if (Disabled) return;
            if (Down && DeactivateOnExternalClick && !WasOver)
            {
                Down = false;
                Function?.Invoke(new UICallbackPackage(GivenName, point, OriginPosition, Down ? 1 : -1));
                Over = IsOver(point);
            }
            else if (WasOver && (!DeactivateOnExternalClick || !Down))
            {
                Down = !Down;
                Function?.Invoke(new UICallbackPackage(GivenName, point, OriginPosition, Down ? 1 : -1));
                WasOver = false;
                if (!Down && OverTexture != null) Over = IsOver(point);
                else Over = false;
            }
        }

        public override void Disable(ColorScale c)
        {
            Over = false;
            DisabledColor = c;
            Disabled = true;
        }
    }
}
