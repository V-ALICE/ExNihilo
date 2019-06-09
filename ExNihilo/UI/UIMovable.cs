using ExNihilo.Util;
using ExNihilo.Util.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ExNihilo.UI
{
    public class UIMovable : UIClickable
    {
        protected Point Anchor;
        protected Vector2 ShiftedPos;
        protected bool Ghosting;

        public UIMovable(string name, string path, Vector2 relPos, ColorScale color, UIPanel superior, TextureUtilities.PositionType anchorPoint, 
            string downPath = "", string overPath = "", bool ghost = false) : base(name, path, relPos, color, superior, anchorPoint, downPath, overPath)
        {
            Ghosting = ghost;
        }

        //Right now Movables can't be absolute because relative positioning is required for resizing screen adjustments
        protected UIMovable(string name, string path, Coordinate pixelOffset, ColorScale color, UIElement superior, TextureUtilities.PositionType anchorPoint, 
            TextureUtilities.PositionType superAnchorPoint, string downPath = "", string overPath = "", bool ghost = false) : 
            base(name, path, pixelOffset, color, superior, anchorPoint, superAnchorPoint, downPath, overPath)
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

            if (Down)
            {
                if (Ghosting)
                {
                    var ghost = ColorScale.Get();
                    ghost.A /= 4;
                    if (DownTexture is null) Texture.Draw(spriteBatch, OriginPosition, ColorScale, CurrentScale);
                    else DownTexture.Draw(spriteBatch, OriginPosition, ColorScale, CurrentScale);
                    Texture.Draw(spriteBatch, OriginPosition + ShiftedPos, ghost, CurrentScale);
                }
                else
                {
                    Texture.Draw(spriteBatch, OriginPosition + ShiftedPos, ColorScale, CurrentScale);
                }
            }
            else
            {
                Texture.Draw(spriteBatch, OriginPosition, Disabled ? DisabledColor : ColorScale, CurrentScale);
            }
        }

        public override void OnMoveMouse(Point point)
        {
            if (Disabled) return;
            if (Down)
            {
                var opp = BaseElement.CurrentPixelSize + BaseElement.OriginPosition;
                var newX = MathHelper.Clamp(OriginPosition.X + TextureOffsetToOrigin.X + point.X - Anchor.X, BaseElement.OriginPosition.X, opp.X);
                var newY = MathHelper.Clamp(OriginPosition.Y + TextureOffsetToOrigin.Y + point.Y - Anchor.Y, BaseElement.OriginPosition.Y, opp.Y);
                ShiftedPos = new Vector2(newX, newY) - OriginPosition - TextureOffsetToOrigin;
            }
            else if (OverTexture != null)
            {
                Down = IsOver(point);
            }
        }

        public override bool OnLeftClick(Point point)
        {
            if (Disabled) return false;
            if (IsOver(point))
            {
                Over = false;
                Down = true;
                Anchor = point;
            }

            return Down;
        }

        public override void OnLeftRelease(Point point)
        {
            if (Disabled) return;
            if (Down)
            {
                Down = false;
                OriginPosition += ShiftedPos;
                ShiftedPos = new Vector2();

                var relX = BaseElement.CurrentPixelSize.X == 0 ? 0
                    : (OriginPosition.X - BaseElement.OriginPosition.X + TextureOffsetToOrigin.X) / BaseElement.CurrentPixelSize.X;
                var relY = BaseElement.CurrentPixelSize.Y == 0 ? 0
                    : (OriginPosition.Y - BaseElement.OriginPosition.Y + TextureOffsetToOrigin.Y) / BaseElement.CurrentPixelSize.Y;
                PositionRelativeToBase = new Vector2(relX, relY);

                Function?.Invoke(new UICallbackPackage(GivenName, point, OriginPosition));
            }
        }
    }
}
