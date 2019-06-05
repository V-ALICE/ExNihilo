using System.Collections.Generic;
using ExNihilo.Util;
using ExNihilo.Util.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace ExNihilo.UI
{
    public class UIPanel : UIClickable
    {
        protected List<UIElement> Set;
        protected Vector2 BaseSizeRel;
        protected bool IsRelativeToWindow, VerticalLocked, HorizontalLocked;

        public UIPanel(Vector2 relPos, Coordinate absoluteSize,PositionType t = PositionType.Center, bool pixelOffset = false) : 
            base("null", relPos, "", t, 1, pixelOffset)
        {
            Set = new List<UIElement>();
            BaseSize = absoluteSize;
            IsRelativeToWindow = false;
        }

        public UIPanel(Vector2 relPos, Vector2 relSize, PositionType t = PositionType.Center, bool pixelOffset = false) :
            base("null", relPos, "", t, 1, pixelOffset)
        {
            Set = new List<UIElement>();
            BaseSizeRel = relSize;
            if (MathD.IsClose(relSize.X, 0)) HorizontalLocked = true;
            if (MathD.IsClose(relSize.Y, 0)) VerticalLocked = true;
            IsRelativeToWindow = true;
        }

        public void AddElements(params UIElement[] elements)
        {
            Set.AddRange(elements);
        }

        public override void LoadContent(GraphicsDevice graphics, ContentManager content)
        {
            base.LoadContent(graphics, content);
            foreach (var item in Set) item.LoadContent(graphics, content);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (!Loaded) return;
            base.Draw(spriteBatch, Color.White);
            foreach (var item in Set) item.Draw(spriteBatch, Color.White);

            LineDrawer.DrawSquare(spriteBatch, Pos, BaseSize.X, BaseSize.Y, Activated ? Color.White : Color.Black, 5);
        }

        public override void Draw(SpriteBatch spriteBatch, Color color)
        {
            if (!Loaded) return;
            base.Draw(spriteBatch, color);
            foreach (var item in Set) item.Draw(spriteBatch, color);

            LineDrawer.DrawSquare(spriteBatch, Pos, BaseSize.X, BaseSize.Y, Activated ? Color.White : Color.Black, 5);
        }

        public override void OnResize(GraphicsDevice graphics, Coordinate window, Vector2 origin)
        {
            if (!Loaded) return;
            if (IsRelativeToWindow)
            {
                if (VerticalLocked)   BaseSizeRel.X = MathHelper.Clamp(BaseSizeRel.X, 1f / window.X, 1.0f);
                if (HorizontalLocked) BaseSizeRel.Y = MathHelper.Clamp(BaseSizeRel.Y, 1f / window.Y, 1.0f);

                BaseSize = new Coordinate(window * BaseSizeRel);
                TextureOffset = GetOffset();
            }
            base.OnResize(graphics, window, origin);
            foreach (var item in Set) item.OnResize(graphics, BaseSize, Pos);
        }

        public override void OnMoveMouse(Point point)
        {
            foreach (var item in Set)
            {
                if (item is UIClickable click) click.OnMoveMouse(point);
            }
        }

        public override void OnLeftClick(Point point)
        {
            base.OnLeftClick(point);
            foreach (var item in Set)
            {
                if (item is UIClickable click) click.OnLeftClick(point);
            }
        }

        public override void OnLeftRelease()
        {
            base.OnLeftRelease();
            foreach (var item in Set)
            {
                if (item is UIClickable click) click.OnLeftRelease();
            }
        }
    }
}
