using ExNihilo.Util;
using ExNihilo.Util.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ExNihilo.UI
{
    public class UITogglable : UIClickable
    {
        protected bool WasOver;

        public UITogglable(string name, string path, Vector2 relPos, ColorScale color, UIPanel superior, 
            TextureUtilities.PositionType anchorPoint, string downPath, string midPath = "", bool mulligan=false) :
            base(name, path, relPos, color, superior, anchorPoint, downPath, midPath, mulligan)
        {
        }

        public UITogglable(string name, string path, Coordinate pixelOffset, ColorScale color, UIElement superior, 
            TextureUtilities.PositionType anchorPoint, TextureUtilities.PositionType superAnchorPoint, string downPath, string midPath = "",
            bool mulligan=false) : base(name, path, pixelOffset, color, superior, anchorPoint, superAnchorPoint, downPath, midPath, mulligan)
        {
        }

        public override void OnMoveMouse(Point point)
        {
            if (Disabled) return;
            if ((AllowMulligan && WasOver) || (!Down && OverTexture != null))
            {
                var isOver = IsOver(point);
                if (WasOver && AllowMulligan) WasOver = isOver;
                if (!Down && OverTexture != null) Over = isOver;
            }
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
            if (WasOver)
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
            ColorScale = c;
            Disabled = true;
        }
    }
}
