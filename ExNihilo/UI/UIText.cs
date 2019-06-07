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
        protected Color[] Colors;

        public UIText(string name, Vector2 relPos, string smartText, Color[] colors, UIPanel superior, PositionType anchorPoint, 
            bool reducedSpaces = false) : base(name, "null", relPos, superior, anchorPoint)
        {
            SpacesReduced = reducedSpaces;
            Text = smartText;
            Colors = colors;
        }

        public UIText(string name, Coordinate pixelOffset, string smartText, Color[] colors, UIElement superior, PositionType anchorPoint,
            PositionType superAnchorType, bool reducedSpaces = false) : base(name, "null", pixelOffset, superior, anchorPoint, superAnchorType)
        {
            SpacesReduced = reducedSpaces;
            Text = smartText;
            Colors = colors;
        }

        public void SetText(string smartText, params Color[] colors)
        {
            //TODO: maybe make this build a texture so it can just draw it?
            Text = smartText;
            Colors = colors;
            UnscaledSize = TextDrawer.GetSmartTextSize(Text);
            CurrentPixelSize = new Coordinate((int)(CurrentScale * UnscaledSize.X), (int)(CurrentScale * UnscaledSize.Y));
            TextureOffsetToOrigin = GetOffset(AnchorType, CurrentPixelSize);
        }

        public override void ReinterpretScale(Coordinate window)
        {
            if (!Loaded) return;
            CurrentScale = ScaleRules.GetScale(window);
            CurrentPixelSize = new Coordinate((int) (CurrentScale * UnscaledSize.X), (int) (CurrentScale*UnscaledSize.Y));
            TextureOffsetToOrigin = GetOffset(AnchorType, CurrentPixelSize);
        }

        public override void LoadContent(GraphicsDevice graphics, ContentManager content)
        {
            Loaded = true;
            ScaleRules = UILibrary.DefaultScaleRuleSet;
            UnscaledSize = TextDrawer.GetSmartTextSize(Text);
            CurrentPixelSize = new Coordinate((int)(CurrentScale * UnscaledSize.X), (int)(CurrentScale * UnscaledSize.Y));
            TextureOffsetToOrigin = GetOffset(AnchorType, CurrentPixelSize);
            LastResizeWindow = new Coordinate();
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (!Loaded) return;
            TextDrawer.DrawSmartText(spriteBatch, OriginPosition, Text, CurrentScale, SpacesReduced, Colors);
        }
    }
}
