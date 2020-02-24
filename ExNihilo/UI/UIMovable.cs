using ExNihilo.Util;
using ExNihilo.Util.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ExNihilo.UI
{
    public class UIMovable : UIClickable
    {
        protected Point Anchor;
        protected Coordinate ShiftedPos;
        protected bool Ghosting, TrueMove;

        public UIMovable(string name, string path, Vector2 relPos, ColorScale color, UIPanel superior, TextureUtilities.PositionType anchorPoint, 
           bool ghost = false, bool allowMove=true) : base(name, path, relPos, color, superior, anchorPoint)
        {
            Ghosting = ghost;
            TrueMove = allowMove;
        }

        //Right now Movables can't be absolute because relative positioning is required for resizing screen adjustments
        public UIMovable(string name, string path, Coordinate pixelOffset, ColorScale color, UIElement superior, TextureUtilities.PositionType anchorPoint,
            TextureUtilities.PositionType superAnchorPoint, bool ghost = false, bool allowMove = true) : base(name, path, pixelOffset, color, superior, anchorPoint, superAnchorPoint)
        {
            Ghosting = ghost;
            TrueMove = allowMove;
        }

        public override void OnResize(GraphicsDevice graphics, Coordinate gameWindow)
        {
            if (!Loaded || WasResized(gameWindow)) return;
            base.OnResize(graphics, gameWindow);

            //TODO: doesn't work for absolute offsets
            //Make sure moved elements don't go out of bounds
            var opp = BaseElement.CurrentPixelSize + BaseElement.OriginPosition;
            var newX = MathHelper.Clamp(OriginPosition.X + TextureOffsetToOrigin.X, BaseElement.OriginPosition.X, opp.X - CurrentPixelSize.X);
            var newY = MathHelper.Clamp(OriginPosition.Y + TextureOffsetToOrigin.Y, BaseElement.OriginPosition.Y, opp.Y - CurrentPixelSize.Y);
            //OriginPosition = (Coordinate) ((new Coordinate(newX, newY) - TextureOffsetToOrigin) * PositionRelativeToBase);
            OriginPosition = new Coordinate(newX, newY) - TextureOffsetToOrigin;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (!Loaded) return;

            if (Disabled)
            {
                Texture.Draw(spriteBatch, OriginPosition, DisabledColor?.Get() ?? ColorScale.Get(), CurrentScale);
            }
            else if (Down)
            {
                if (Ghosting)
                {
                    var ghost = ColorScale.Get();
                    ghost.A /= 4;

                    if (DownTexture != null) DownTexture.Draw(spriteBatch, OriginPosition, DownColor?.Get() ?? ColorScale.Get(), CurrentScale);
                    else Texture.Draw(spriteBatch, OriginPosition, DownColor?.Get() ?? ColorScale.Get(), CurrentScale);

                    Texture.Draw(spriteBatch, OriginPosition + ShiftedPos, ghost, CurrentScale);
                }
                else
                {
                    if (DownTexture != null) DownTexture.Draw(spriteBatch, OriginPosition + ShiftedPos, DownColor?.Get() ?? ColorScale.Get(), CurrentScale);
                    else Texture.Draw(spriteBatch, OriginPosition + ShiftedPos, DownColor?.Get() ?? ColorScale.Get(), CurrentScale);
                }
            }
            else if (Over)
            {
                if (OverTexture != null) OverTexture.Draw(spriteBatch, OriginPosition, OverColor?.Get() ?? ColorScale.Get(), CurrentScale);
                else Texture.Draw(spriteBatch, OriginPosition, OverColor?.Get() ?? ColorScale.Get(), CurrentScale);
            }
            else
            {
                Texture.Draw(spriteBatch, OriginPosition, ColorScale.Get(), CurrentScale);
            }
        }

        public override void OnMoveMouse(Point point)
        {
            if (Disabled) return;
            if (Down)
            {
                var opp = BaseElement.CurrentPixelSize + BaseElement.OriginPosition;
                var newX = MathHelper.Clamp(OriginPosition.X + TextureOffsetToOrigin.X + point.X - Anchor.X, BaseElement.OriginPosition.X, opp.X - CurrentPixelSize.X);
                var newY = MathHelper.Clamp(OriginPosition.Y + TextureOffsetToOrigin.Y + point.Y - Anchor.Y, BaseElement.OriginPosition.Y, opp.Y - CurrentPixelSize.Y);
                ShiftedPos = new Coordinate(newX, newY) - OriginPosition - TextureOffsetToOrigin;
            }
            else if (OverTexture != null)
            {
                Over = IsOver(point);
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
                var potPosition = OriginPosition + ShiftedPos;
                ShiftedPos = new Coordinate();

                var relX = BaseElement.CurrentPixelSize.X == 0 ? 0
                    : (potPosition.X - BaseElement.OriginPosition.X + TextureOffsetToOrigin.X) / BaseElement.CurrentPixelSize.X;
                var relY = BaseElement.CurrentPixelSize.Y == 0 ? 0
                    : (potPosition.Y - BaseElement.OriginPosition.Y + TextureOffsetToOrigin.Y) / BaseElement.CurrentPixelSize.Y;
                PositionRelativeToBase = new Vector2(relX, relY);

                Function?.Invoke(new UICallbackPackage(GivenName, point, potPosition));
                if (TrueMove) OriginPosition = potPosition;
                Over = IsOver(point);
            }
        }
    }
}
