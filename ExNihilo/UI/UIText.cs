using ExNihilo.Systems;
using ExNihilo.Systems.Backend;
using ExNihilo.Util;
using ExNihilo.Util.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace ExNihilo.UI
{
    public class UIText : UIElement
    {
        protected TextDrawer.TextParameters param;
        protected Coordinate UnscaledSize;

        public string Text { get; protected set; }
        public ColorScale[] Colors { get; private set; }

        public UIText(string name, Vector2 relPos, string smartText, int lineLength, ColorScale[] colors, UIPanel superior, TextureUtilities.PositionType anchorPoint, 
            bool reducedSpaces = false) : base(name, "null", relPos, Color.White, superior, anchorPoint)
        {
            param = new TextDrawer.TextParameters(reducedSpaces);
            Text = TextDrawer.GetSmartSplit(smartText, lineLength);
            Colors = colors;
        }

        public UIText(string name, Coordinate pixelOffset, string smartText, int lineLength, ColorScale[] colors, UIElement superior, TextureUtilities.PositionType anchorPoint,
            TextureUtilities.PositionType superAnchorType, bool reducedSpaces = false) : base(name, "null", pixelOffset, Color.White, superior, anchorPoint, superAnchorType)
        {
            param = new TextDrawer.TextParameters(reducedSpaces);
            Text = TextDrawer.GetSmartSplit(smartText, lineLength);
            Colors = colors;
        }

        public UIText(string name, Vector2 relPos, string smartText, ColorScale[] colors, UIPanel superior, TextureUtilities.PositionType anchorPoint,
            bool reducedSpaces = false) : base(name, "null", relPos, Color.White, superior, anchorPoint)
        {
            param = new TextDrawer.TextParameters(reducedSpaces);
            Text = smartText;
            Colors = colors;
        }

        public UIText(string name, Coordinate pixelOffset, string smartText, ColorScale[] colors, UIElement superior, TextureUtilities.PositionType anchorPoint,
            TextureUtilities.PositionType superAnchorType, bool reducedSpaces = false) : base(name, "null", pixelOffset, Color.White, superior, anchorPoint, superAnchorType)
        {
            param = new TextDrawer.TextParameters(reducedSpaces);
            Text = smartText;
            Colors = colors;
        }

        public void SetText(string smartText, params ColorScale[] colors)
        {
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
                OriginPosition = scaledOffset + BaseElement.OriginPosition - TextureOffsetToOrigin + superOffset;
            }
            else
            {
                //Position of this element is relative to the space of its base panel
                OriginPosition = (Coordinate)(BaseElement.CurrentPixelSize * PositionRelativeToBase) + BaseElement.OriginPosition - TextureOffsetToOrigin;
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
            if (ScaleRules is null) ScaleRules = TextureLibrary.DefaultScaleRuleSet;
            UnscaledSize = TextDrawer.GetSmartTextSize(Text);
            CurrentPixelSize = new Coordinate((int)(CurrentScale * UnscaledSize.X), (int)(CurrentScale * UnscaledSize.Y));
            TextureOffsetToOrigin = TextureUtilities.GetOffset(AnchorType, CurrentPixelSize);
            LastResizeWindow = new Coordinate();
        }

        public override void Draw(SpriteBatch spriteBatch, Coordinate rightDownOffset)
        {
            if (!Loaded || DontDrawThis) return;
            TextDrawer.DrawSmartText(spriteBatch, OriginPosition + rightDownOffset, Text, CurrentScale, param, Colors);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (!Loaded || DontDrawThis) return;
            TextDrawer.DrawSmartText(spriteBatch, OriginPosition, Text, CurrentScale, param, Colors);
        }

        public override void ChangeColor(ColorScale color)
        {
            Colors = new[] {color};
        }

        public void ChangeColor(ColorScale[] colors)
        {
            Colors = colors;
        }
    }
}
