using ExNihilo.Util;
using ExNihilo.Util.Graphics;
using Microsoft.Xna.Framework;

namespace ExNihilo.UI
{
    public class UIRadioSet : UIPanel
    {
        public UIRadioSet(string name, Vector2 relPos, Coordinate absoluteSize, UIPanel superior, TextureUtilities.PositionType anchorPoint) :
            base(name, relPos, absoluteSize, superior, anchorPoint)
        {
        }

        public UIRadioSet(string name, Coordinate pixelSize, Vector2 relSize, UIPanel superior, TextureUtilities.PositionType anchorPoint,
            TextureUtilities.PositionType superAnchorPoint) : base(name, pixelSize, relSize, superior, anchorPoint, superAnchorPoint)
        {
        }

        public UIRadioSet(string name, Vector2 relPos, Vector2 relSize, UIPanel superior, TextureUtilities.PositionType anchorPoint) :
            base(name, relPos, relSize, superior, anchorPoint)
        {
        }

        public UIRadioSet(string name, Coordinate pixelSize, Coordinate absoluteSize, UIElement superior, TextureUtilities.PositionType anchorPoint,
            TextureUtilities.PositionType superAnchorPoint) : base(name, pixelSize, absoluteSize, superior, anchorPoint, superAnchorPoint)
        {
        }

        public override void OnLeftRelease(Point point)
        {
            foreach (var element in Set)
            {
                if (element is UITogglable item)
                {
                    var old = item.Down;
                    item.OnLeftRelease(point);
                    if (!old && item.Down)
                    {
                        foreach (var i in Set)
                        {
                            if (!i.Equals(item)) (i as UITogglable)?.ForcePush(false, false);
                        }
                    }
                    else if (old && !item.Down) item.ForcePush(true, false);
                }
            }
        }

    }
}
