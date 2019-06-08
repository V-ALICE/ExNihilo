using ExNihilo.Util;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ExNihilo.UI
{
    public class UIMovable : UIClickable
    {
        protected Point Anchor;
        protected Vector2 ShiftedPos;
        protected bool Ghosting;

        public UIMovable(string name, string path, Vector2 relPos, UIPanel superior, PositionType anchorPoint, string altPath = "", bool ghost = false) :
            base(name, path, relPos, superior, anchorPoint, altPath)
        {
            Ghosting = ghost;
        }

        //Right now Movables can't be absolute because relative positioning is required for resizing screen adjustments
        protected UIMovable(string name, string path, Coordinate pixelOffset, UIElement superior, PositionType anchorPoint, PositionType superAnchorPoint,
            string altPath = "", bool ghost = false) : base(name, path, pixelOffset, superior, anchorPoint, superAnchorPoint, altPath)
        {
            Ghosting = ghost;
        }

        public override void OnResize(GraphicsDevice graphics, Coordinate gameWindow)
        {
            if (!Loaded || WasResized(gameWindow)) return;
            base.OnResize(graphics, gameWindow);

            //Make sure moved elements don't go out of bounds
            var opp = BaseElement.CurrentPixelSize + BaseElement.OriginPosition;
            var newX = MathHelper.Clamp(OriginPosition.X + TextureOffsetToOrigin.X, BaseElement.OriginPosition.X, opp.X);
            var newY = MathHelper.Clamp(OriginPosition.Y + TextureOffsetToOrigin.Y, BaseElement.OriginPosition.Y, opp.Y);
            OriginPosition = new Vector2(newX, newY) - TextureOffsetToOrigin;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (!Loaded) return;

            if (Activated)
            {
                if (Ghosting)
                {
                    var ghost = new Color(CurrentColor.R, CurrentColor.G, CurrentColor.B, CurrentColor.A/4);
                    spriteBatch.Draw(AltTexture ?? Texture, OriginPosition, null, CurrentColor, 0, Vector2.Zero, CurrentScale, SpriteEffects.None, 0);
                    spriteBatch.Draw(Texture, OriginPosition + ShiftedPos, null, ghost, 0, Vector2.Zero, CurrentScale, SpriteEffects.None, 0);
                }
                else
                {
                    spriteBatch.Draw(Texture, OriginPosition + ShiftedPos, null, CurrentColor, 0, Vector2.Zero, CurrentScale, SpriteEffects.None, 0);
                }
            }
            else
            {
                spriteBatch.Draw(Texture, OriginPosition, null, CurrentColor, 0, Vector2.Zero, CurrentScale, SpriteEffects.None, 0);
            }
        }

        public override void OnMoveMouse(Point point)
        {
            if (Activated)
            {
                var opp = BaseElement.CurrentPixelSize + BaseElement.OriginPosition;
                var newX = MathHelper.Clamp(OriginPosition.X + TextureOffsetToOrigin.X + point.X - Anchor.X, BaseElement.OriginPosition.X, opp.X);
                var newY = MathHelper.Clamp(OriginPosition.Y + TextureOffsetToOrigin.Y + point.Y - Anchor.Y, BaseElement.OriginPosition.Y, opp.Y);
                ShiftedPos = new Vector2(newX, newY) - OriginPosition - TextureOffsetToOrigin;
            }
        }

        public override bool OnLeftClick(Point point)
        {
            if (Disabled) return false;
            if (IsOver(point))
            {
                Activated = true;
                Anchor = point;
            }

            return Activated;
        }

        public override void OnLeftRelease(Point point)
        {
            if (Activated)
            {
                Activated = false;
                OriginPosition += ShiftedPos;
                ShiftedPos = new Vector2();

                var relX = BaseElement.CurrentPixelSize.X == 0 ? 0
                    : (OriginPosition.X - BaseElement.OriginPosition.X + TextureOffsetToOrigin.X) / BaseElement.CurrentPixelSize.X;
                var relY = BaseElement.CurrentPixelSize.Y == 0 ? 0
                    : (OriginPosition.Y - BaseElement.OriginPosition.Y + TextureOffsetToOrigin.Y) / BaseElement.CurrentPixelSize.Y;
                PositionRelativeToBase = new Vector2(relX, relY);

                Function?.Invoke(new UICallbackPackage(GivenName, 0, point, OriginPosition));
            }
        }
    }
}
