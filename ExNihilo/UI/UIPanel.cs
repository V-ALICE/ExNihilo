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
        protected List<UIElement> set;
        protected Vector2 baseSizeRel;
        protected bool isRelativeToWindow;

        public UIPanel(Vector2 relPos, Coordinate absoluteSize, bool absolute=false, PositionType t = PositionType.Center) : 
            base("null", relPos, "", absolute, 1, t)
        {
            set = new List<UIElement>();
            baseSize = absoluteSize;
            isRelativeToWindow = false;
        }

        public UIPanel(Vector2 relPos, Vector2 relSize, bool absolute = false, PositionType t = PositionType.Center) : 
            base("null", relPos, "", absolute, 1, t)
        {
            set = new List<UIElement>();
            baseSizeRel = relSize;
            isRelativeToWindow = true;
        }

        public void AddElement(UIElement element)
        {
            set.Add(element);
        }

        public override void LoadContent(GraphicsDevice graphics, ContentManager content)
        {
            base.LoadContent(graphics, content);
            foreach (var item in set) item.LoadContent(graphics, content);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (!loaded) return;
            base.Draw(spriteBatch, Color.White);
            foreach (var item in set) item.Draw(spriteBatch, Color.White);

            LineDrawer.DrawSquare(spriteBatch, pos, baseSize.X, baseSize.Y, Activated ? Color.White : Color.Black, 5);
        }

        public override void Draw(SpriteBatch spriteBatch, Color color)
        {
            if (!loaded) return;
            base.Draw(spriteBatch, color);
            foreach (var item in set) item.Draw(spriteBatch, color);

            LineDrawer.DrawSquare(spriteBatch, pos, baseSize.X, baseSize.Y, Activated ? Color.White : Color.Black, 5);
        }

        public override void OnResize(GraphicsDevice graphics, Coordinate window, Vector2 origin)
        {
            if (isRelativeToWindow) baseSize = new Coordinate(window * baseSizeRel);
            base.OnResize(graphics, window, origin);
            foreach (var item in set) item.OnResize(graphics, baseSize, pos);
        }

        public override void OnMoveMouse(Point point)
        {
            foreach (var item in set)
            {
                if (item is UIClickable click) click.OnMoveMouse(point);
            }
        }

        public override void OnLeftClick(Point point)
        {
            base.OnLeftClick(point);
            foreach (var item in set)
            {
                if (item is UIClickable click) click.OnLeftClick(point);
            }
        }

        public override void OnLeftRelease()
        {
            base.OnLeftRelease();
            foreach (var item in set)
            {
                if (item is UIClickable click) click.OnLeftRelease();
            }
        }
    }
}
