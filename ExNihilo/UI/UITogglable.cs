using ExNihilo.Util;
using ExNihilo.Util.Graphics;
using Microsoft.Xna.Framework;

namespace ExNihilo.UI
{
    public class UITogglable : UIClickable
    {
        protected bool WasOver;
        public UITogglable(string name, string path, Vector2 relPos, UIPanel superior, PositionType anchorPoint, string altPath, bool mulligan=false) : base(name, path, relPos, superior, anchorPoint, altPath, mulligan)
        {
        }

        public UITogglable(string name, string path, Coordinate pixelOffset, UIElement superior, PositionType anchorPoint, PositionType superAnchorPoint, string altPath, bool mulligan=false) : base(name, path, pixelOffset, superior, anchorPoint, superAnchorPoint, altPath, mulligan)
        {
        }

        public override void OnMoveMouse(Point point)
        {
            //mulligans only check the size/alpha of the original texture
            if (WasOver && AllowMulligan) Activated = IsOver(point);
        }

        public override bool OnLeftClick(Point point)
        {
            if (Disabled) return false;
            WasOver = IsOver(point);
            return WasOver;
        }

        public override void OnLeftRelease()
        {
            if (WasOver)
            {
                Activated = !Activated;
                Function?.Invoke(GivenName, 0, Activated);
                WasOver = false;
            }
        }

        public override void Disable(ColorScale c)
        {
            CurrentColor = c;
            Disabled = true;
        }
    }
}
