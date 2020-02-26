using ExNihilo.Util;
using ExNihilo.Util.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ExNihilo.UI
{
    public class UIParallaxElement : UIClickable
    { 
        protected readonly float Weight;
        protected Vector2 AdjustedOrigin, LastAnchor;

        public UIParallaxElement(string name, string path, Vector2 relPos, ColorScale color, UIPanel superior, 
            TextureUtilities.PositionType anchorPoint, float weight) : base(name, path, relPos, color, superior, anchorPoint)
        {
            Weight = weight;
        }

        public UIParallaxElement(string name, string path, Coordinate pixelOffset, ColorScale color, UIElement superior, 
            float weight, TextureUtilities.PositionType anchorPoint, TextureUtilities.PositionType superAnchorType) :
            base(name, path, pixelOffset, color, superior, anchorPoint, superAnchorType)
        {
            Weight = weight;
        }

        public override bool OnMoveMouse(Point point)
        {
            AdjustedOrigin = new Vector2(LastAnchor.X - Weight * point.X, LastAnchor.Y - Weight * point.Y);
            return false;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (!Loaded) return;
            Texture.Draw(spriteBatch, AdjustedOrigin, ColorScale?.Get() ?? Color.White, CurrentScale);
        }

        public override void OnResize(GraphicsDevice graphics, Coordinate gameWindow)
        {
            base.OnResize(graphics, gameWindow);
            LastAnchor = OriginPosition + Weight * (OriginPosition + TextureOffsetToOrigin);
        }

        public override bool IsOver(Point mousePos)
        {
            return false;
        }
        //Not actually clickable
        public override bool OnLeftClick(Point point)
        {
            return false;
        }
        public override void OnLeftRelease(Point point)
        {
        }
        public override void Disable(ColorScale c)
        {
        }
    }
}
