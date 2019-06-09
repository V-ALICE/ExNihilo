using ExNihilo.UI.Bases;
using ExNihilo.Util;
using ExNihilo.Util.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace ExNihilo.UI
{
    public class UIText : UIElement
    {
        protected bool SpacesReduced;
        protected string Text; 
        protected Coordinate UnscaledSize;
        protected ColorScale[] Colors;

        public UIText(string name, Vector2 relPos, string smartText, ColorScale[] colors, UIPanel superior, TextureUtilities.PositionType anchorPoint, 
            bool reducedSpaces = false) : base(name, "null", relPos, Color.White, superior, anchorPoint)
        {
            SpacesReduced = reducedSpaces;
            Text = smartText;
            Colors = colors;
        }

        public UIText(string name, Coordinate pixelOffset, string smartText, ColorScale[] colors, UIElement superior, TextureUtilities.PositionType anchorPoint,
            TextureUtilities.PositionType superAnchorType, bool reducedSpaces = false) : base(name, "null", pixelOffset, Color.White, superior, anchorPoint, superAnchorType)
        {
            SpacesReduced = reducedSpaces;
            Text = smartText;
            Colors = colors;
        }

        public void SetText(string smartText, params ColorScale[] colors)
        {
            //TODO: maybe make this build a texture so it can just draw it?
            Text = smartText;
            if (colors.Length > 0) Colors = colors;
            UnscaledSize = TextDrawer.GetSmartTextSize(Text);
            CurrentPixelSize = new Coordinate((int)(CurrentScale * UnscaledSize.X), (int)(CurrentScale * UnscaledSize.Y));
            TextureOffsetToOrigin = TextureUtilities.GetOffset(AnchorType, CurrentPixelSize);

            if (AbsoluteOffset)
            {
                //Position of this element is relative to the origin of its base element in scaled pixels
                var scaledOffset = new Coordinate(
                    (int)(BaseElement.CurrentScale * PixelOffsetFromBase.X),
                    (int)(BaseElement.CurrentScale * PixelOffsetFromBase.Y));
                var superOffset = TextureUtilities.GetOffset(SuperAnchorType, BaseElement.CurrentPixelSize);
                OriginPosition = BaseElement.OriginPosition + scaledOffset - TextureOffsetToOrigin + superOffset;
            }
            else
            {
                //Position of this element is relative to the space of its base panel
                OriginPosition = BaseElement.OriginPosition + BaseElement.CurrentPixelSize * PositionRelativeToBase - TextureOffsetToOrigin;
            }
        }

        public override void ReinterpretScale(Coordinate window)
        {
            if (!Loaded) return;
            CurrentScale = ScaleRules.GetScale(window);
            CurrentPixelSize = new Coordinate((int) (CurrentScale * UnscaledSize.X), (int) (CurrentScale*UnscaledSize.Y));
            TextureOffsetToOrigin = TextureUtilities.GetOffset(AnchorType, CurrentPixelSize);
        }

        public override void LoadContent(GraphicsDevice graphics, ContentManager content)
        {
            Loaded = true;
            if (ScaleRules is null) ScaleRules = UILibrary.DefaultScaleRuleSet;
            UnscaledSize = TextDrawer.GetSmartTextSize(Text);
            CurrentPixelSize = new Coordinate((int)(CurrentScale * UnscaledSize.X), (int)(CurrentScale * UnscaledSize.Y));
            TextureOffsetToOrigin = TextureUtilities.GetOffset(AnchorType, CurrentPixelSize);
            LastResizeWindow = new Coordinate();
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (!Loaded) return;
            TextDrawer.DrawSmartText(spriteBatch, OriginPosition, Text, CurrentScale, SpacesReduced, Colors);
        }
    }
}
