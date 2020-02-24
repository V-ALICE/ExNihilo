using ExNihilo.Systems.Backend;
using ExNihilo.Util;
using ExNihilo.Util.Graphics;
using Microsoft.Xna.Framework;

namespace ExNihilo.UI
{
    public class UIDynamicMovable : UIMovable
    {
        public UIDynamicMovable(string name, string path, Vector2 relPos, ColorScale color, UIPanel superior, TextureUtilities.PositionType anchorPoint, bool ghost = false) 
            : base(name, path, relPos, color, superior, anchorPoint, ghost)
        {
        }

        public UIDynamicMovable(string name, string path, Coordinate pixelOffset, ColorScale color, UIElement superior, TextureUtilities.PositionType anchorPoint, TextureUtilities.PositionType superAnchorPoint, bool ghost = false)
            : base(name, path, pixelOffset, color, superior, anchorPoint, superAnchorPoint, ghost)
        {
        }

        public void ChangeColor(ColorScale color)
        {
            ColorScale = color;
        }

        public void ResetTextureFrame()
        {
            Texture.ResetFrame();
        }

        public void SetNullTexture()
        {
            Texture = TextureLibrary.Lookup("null");
        }

        public void ChangeTexture(AnimatableTexture texture)
        {
            Texture = texture.Copy();
            CurrentPixelSize = new Coordinate((int)(CurrentScale * Texture.Width), (int)(CurrentScale * Texture.Height));
            TextureOffsetToOrigin = TextureUtilities.GetOffset(AnchorType, CurrentPixelSize);

            if (AbsoluteOffset)
            {
                //Position of this element is relative to the origin of its base element in scaled pixels
                var scaledOffset = new Coordinate(
                    (int)(BaseElement.CurrentScale * PixelOffsetFromBase.X),
                    (int)(BaseElement.CurrentScale * PixelOffsetFromBase.Y));
                var superOffset = TextureUtilities.GetOffset(SuperAnchorType, BaseElement.CurrentPixelSize);
                OriginPosition = scaledOffset + BaseElement.OriginPosition - TextureOffsetToOrigin + superOffset;
            }
            else
            {
                //Position of this element is relative to the space of its base panel
                OriginPosition = (Coordinate)(BaseElement.CurrentPixelSize * PositionRelativeToBase) + BaseElement.OriginPosition - TextureOffsetToOrigin;
            }
        }
    }
}
