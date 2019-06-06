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

        public UIText(Vector2 relPos, string smartText, Color[] colors, PositionType t = PositionType.Center, bool reducedSpaces = false, bool pixelOffset = false) :
            base("null", relPos, t, pixelOffset)
        {
            ExceptionCheck.AssertCondition(pixelOffset || (relPos.X >= 0 && relPos.X <= 1.0 && relPos.Y >= 0 && relPos.Y <= 1.0));
            PosRel = Utilities.Copy(relPos);
            CurrentScale = 1;
            Type = t;
            AbsoluteOffset = pixelOffset;
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
            BaseSize = new Coordinate((int)(CurrentScale * UnscaledSize.X), (int)(CurrentScale * UnscaledSize.Y));
            ReinterpretOffset();
        }

        public override void ReinterpretScale(Coordinate window)
        {
            if (!Loaded) return;
            CurrentScale = ScaleRules.GetScale(window);
            BaseSize = new Coordinate((int) (CurrentScale * UnscaledSize.X), (int) (CurrentScale*UnscaledSize.Y));
            ReinterpretOffset();
        }

        public override void LoadContent(GraphicsDevice graphics, ContentManager content)
        {
            Loaded = true;
            ScaleRules = UILibrary.DefaultScaleRuleSet;
            UnscaledSize = TextDrawer.GetSmartTextSize(Text);
            BaseSize = new Coordinate((int)(CurrentScale * UnscaledSize.X), (int)(CurrentScale * UnscaledSize.Y));
            ReinterpretOffset();
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (!Loaded) return;
            TextDrawer.DrawSmartText(spriteBatch, Pos, Text, CurrentScale, SpacesReduced, Colors);
        }
    }
}
