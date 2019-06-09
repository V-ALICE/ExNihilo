using System;
using ExNihilo.Util;
using ExNihilo.Util.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace ExNihilo.UI
{
    public class UIExtendable : UIClickable
    {
        //TODO: someway to allow these to be extendable backwards (not just down and right)
        protected Vector2 RelativeScalar, MaxiumScalar, MinimumScalar;
        protected Coordinate MaximumSize, BaseTextureSize;
        protected readonly bool HorizontalLocked, VerticalLocked;

        public UIExtendable(string name, string path, Vector2 relPos, ColorScale color, UIPanel superior, TextureUtilities.PositionType anchorPoint, Coordinate maxSize, 
            bool allowHorizontalChange = true, bool allowVerticalChange = true) :
            base(name, path, relPos, color, superior, anchorPoint)
        {
            RelativeScalar = new Vector2(1,1);
            MaximumSize = maxSize;
            HorizontalLocked = !allowHorizontalChange;
            VerticalLocked = !allowVerticalChange;
        }
        public UIExtendable(string name, string path, Coordinate pixelOffset, ColorScale color, UIElement superior, TextureUtilities.PositionType anchorPoint, 
            TextureUtilities.PositionType superAnchorPoint, Coordinate maxSize, bool allowHorizontalChange = true, bool allowVerticalChange = true) : 
            base(name, path, pixelOffset, color, superior, anchorPoint, superAnchorPoint)
        {
            RelativeScalar = new Vector2(1,1);
            MaximumSize = maxSize;
            HorizontalLocked = !allowHorizontalChange;
            VerticalLocked = !allowVerticalChange;
        }

        public void ForceValue(Vector2 relativeScalar)
        {
            RelativeScalar = relativeScalar;
            RelativeScalar.X = MathHelper.Clamp(RelativeScalar.X, HorizontalLocked ? 1 : 0, 1);
            RelativeScalar.Y = MathHelper.Clamp(RelativeScalar.Y, VerticalLocked ? 1 : 0, 1);
        }

        public override void ReinterpretScale(Coordinate window)
        {
            if (!Loaded) return;
            CurrentScale = ScaleRules.GetScale(window);
            CurrentPixelSize = new Coordinate((int)(CurrentScale * MaximumSize.X), (int)(CurrentScale * MaximumSize.Y));
            TextureOffsetToOrigin = TextureUtilities.GetOffset(AnchorType, CurrentPixelSize);
        }

        public override void LoadContent(GraphicsDevice graphics, ContentManager content)
        {
            base.LoadContent(graphics, content);
            CurrentPixelSize = new Coordinate((int)(CurrentScale * MaximumSize.X), (int)(CurrentScale * MaximumSize.Y));
            BaseTextureSize = new Coordinate((int)(CurrentScale * Texture.Width), (int)(CurrentScale * Texture.Height));
            MaxiumScalar = new Vector2((float)MaximumSize.X / BaseTextureSize.X, (float)MaximumSize.Y / BaseTextureSize.Y);
            MinimumScalar = new Vector2(HorizontalLocked ? 1 : 0, VerticalLocked ? 1 : 0);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (!Loaded) return;
            Texture.Draw(spriteBatch, OriginPosition, ColorScale, CurrentScale * RelativeScalar * MaxiumScalar);
        }

        protected void ReinterpretScalar(Point point)
        {
            var diff = new Vector2(point.X - OriginPosition.X, point.Y - OriginPosition.Y);
            var scal = new Vector2(CurrentScale*BaseTextureSize.X, CurrentScale*BaseTextureSize.Y);
            RelativeScalar = diff / scal / MaxiumScalar;
            RelativeScalar.X = MathHelper.Clamp(RelativeScalar.X, HorizontalLocked ? 1 : 0, 1);
            RelativeScalar.Y = MathHelper.Clamp(RelativeScalar.Y, VerticalLocked ? 1 : 0, 1);
        }

        public override bool IsOver(Point mousePos)
        {
            if (TexturePath == "null") return false;
            int buttonX = (int)(Math.Round(mousePos.X - OriginPosition.X) / CurrentScale);
            int buttonY = (int)(Math.Round(mousePos.Y - OriginPosition.Y) / CurrentScale);
            return buttonX >= 0 && buttonY >= 0 && buttonX < MaximumSize.X && buttonY < MaximumSize.Y;
        }

        public override void OnMoveMouse(Point point)
        {
            if (Down) ReinterpretScalar(point);
        }

        public override bool OnLeftClick(Point point)
        {
            if (Disabled) return false;
            if (IsOver(point))
            {
                Down = true;
                ReinterpretScalar(point);
                return true;
            }

            Down = false;
            return false;
        }

        public override void OnLeftRelease(Point point)
        {
            if (Down) Function?.Invoke(new UICallbackPackage(GivenName, point, OriginPosition, RelativeScalar.X, RelativeScalar.Y));
            Down = false;
        }

    }
}
