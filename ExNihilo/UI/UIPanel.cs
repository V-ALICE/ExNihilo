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
        protected Dictionary<string, UIElement> Set;
        protected Vector2 BaseSizeRel;
        protected bool IsRelativeToWindow, VerticalLocked, HorizontalLocked, King;

        public UIPanel(string name, Vector2 relPos, Coordinate absoluteSize, UIPanel superior, PositionType anchorPoint) : 
            base(name, "null", relPos, superior, anchorPoint)
        {
            Set = new Dictionary<string, UIElement>();
            CurrentPixelSize = absoluteSize;
            IsRelativeToWindow = false;
        }

        public UIPanel(string name, Vector2 relPos, Vector2 relSize, UIPanel superior, PositionType anchorPoint) : 
            base(name, "null", relPos, superior, anchorPoint)
        {
            Set = new Dictionary<string, UIElement>();
            BaseSizeRel = relSize;
            if (MathD.IsClose(relSize.X, 0)) HorizontalLocked = true;
            if (MathD.IsClose(relSize.Y, 0)) VerticalLocked = true;
            IsRelativeToWindow = true;
        }

        public UIPanel(string name, Coordinate pixelSize, Coordinate absoluteSize, UIElement superior, PositionType anchorPoint, 
            PositionType superAnchorPoint) : base(name, "null", pixelSize, superior, anchorPoint, superAnchorPoint)
        {
            Set = new Dictionary<string, UIElement>();
            CurrentPixelSize = absoluteSize;
            IsRelativeToWindow = false;
        }

        public UIPanel(string name, Vector2 relPos, Coordinate absoluteSize, PositionType anchorPoint) : base(name, relPos, anchorPoint)
        {
            Set = new Dictionary<string, UIElement>();
            CurrentPixelSize = absoluteSize;
            IsRelativeToWindow = false;
            King = true;
        }
        public UIPanel(string name, Vector2 relPos, Vector2 relSize, PositionType anchorPoint) : base(name, relPos, anchorPoint)
        {
            Set = new Dictionary<string, UIElement>();
            BaseSizeRel = relSize;
            if (MathD.IsClose(relSize.X, 0)) HorizontalLocked = true;
            if (MathD.IsClose(relSize.Y, 0)) VerticalLocked = true;
            IsRelativeToWindow = true;
            King = true;
        }

        public override void ReinterpretScale(Coordinate window)
        {
            //OnResize calls each element's OnResize which will handle ReinterpretScale calls
            //No one should be calling this anyway
        }

        public void AddElements(params UIElement[] elements)
        {
            foreach (var e in elements)
            {
                if (Set.ContainsKey(e.GivenName)) Set[e.GivenName] = e;
                else Set.Add(e.GivenName, e);
            }
        }

        public override void LoadContent(GraphicsDevice graphics, ContentManager content)
        {
            base.LoadContent(graphics, content);
            foreach (var item in Set.Values) item.LoadContent(graphics, content);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (!Loaded) return;
            foreach (var item in Set.Values) item.Draw(spriteBatch);

            //LineDrawer.DrawSquare(spriteBatch, Pos, BaseSize.X, BaseSize.Y, Activated ? Color.White : Color.Black, 5);
        }

        public override void OnResize(GraphicsDevice graphics, Coordinate gameWindow)
        {
            if (!Loaded) return;

            if (!WasResized(gameWindow))
            {
                if (King)
                {
                    if (VerticalLocked) BaseSizeRel.X = MathHelper.Clamp(BaseSizeRel.X, 1f / gameWindow.X, 1.0f);
                    if (HorizontalLocked) BaseSizeRel.Y = MathHelper.Clamp(BaseSizeRel.Y, 1f / gameWindow.Y, 1.0f);

                    CurrentPixelSize = new Coordinate(gameWindow * BaseSizeRel);
                    TextureOffsetToOrigin = GetOffset(AnchorType, CurrentPixelSize);

                    LastResizeWindow = gameWindow;
                    if (AbsoluteOffset)
                    {
                        //Position of king is relative to the origin of the window in scaled pixels
                        var scaledOffset = new Coordinate(PixelOffsetFromBase.X, PixelOffsetFromBase.Y);
                        OriginPosition = scaledOffset - TextureOffsetToOrigin;
                    }
                    else
                    {
                        //Position of king is relative to the space of the screen
                        OriginPosition = gameWindow * PositionRelativeToBase - TextureOffsetToOrigin;
                    }
                }
                else if (IsRelativeToWindow)
                {
                    if (VerticalLocked) BaseSizeRel.X = MathHelper.Clamp(BaseSizeRel.X, 1f / BaseElement.CurrentPixelSize.X, 1.0f);
                    if (HorizontalLocked) BaseSizeRel.Y = MathHelper.Clamp(BaseSizeRel.Y, 1f / BaseElement.CurrentPixelSize.Y, 1.0f);

                    CurrentPixelSize = new Coordinate(BaseElement.CurrentPixelSize * BaseSizeRel);
                    TextureOffsetToOrigin = GetOffset(AnchorType, CurrentPixelSize);
                    base.OnResize(graphics, gameWindow);
                }
                else base.OnResize(graphics, gameWindow);
            }

            foreach (var item in Set.Values) item.OnResize(graphics, gameWindow);
        }

        public override void OnMoveMouse(Point point)
        {
            foreach (var item in Set.Values)
            {
                if (item is UIClickable click) click.OnMoveMouse(point);
            }
        }

        public override bool OnLeftClick(Point point)
        {
            foreach (var item in Set.Values)
            {
                if (item is UIClickable click)
                {
                    if (click.OnLeftClick(point)) return true;
                }
            }
            //return base.OnLeftClick(point);
            return false;
        }

        public override void OnLeftRelease(Point point)
        {
            //base.OnLeftRelease(point);
            foreach (var item in Set.Values)
            {
                if (item is UIClickable click) click.OnLeftRelease(point);
            }
        }

        public override void Enable(ColorScale c)
        {
            foreach (var item in Set.Values)
            {
                if (item is UIClickable click) click.Enable(c);
            }
        }

        public override void Disable(ColorScale c)
        {
            foreach (var item in Set.Values)
            {
                if (item is UIClickable click) click.Disable(c);
            }
        }

        public UIElement GetElement(string title)
        {
            if (GivenName == title) return this;
            return Set.ContainsKey(title) ? Set[title] : null;
        }
    }
}
