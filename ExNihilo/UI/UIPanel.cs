using System;
using System.Collections.Generic;
using System.Linq;
using ExNihilo.Util;
using ExNihilo.Util.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace ExNihilo.UI
{
    public class UIPanel : UIClickable
    {
        protected UIPanel Tooltip;
        protected List<UIElement> Set;
        protected Vector2 BaseSizeRel, TooltipOffset;
        protected Coordinate BasePixelSize;
        protected Point LastMousePos;
        protected bool IsRelativeToSuperior, VerticalLocked, HorizontalLocked, King;

        public UIPanel(string name, Vector2 relPos, Coordinate absoluteSize, UIPanel superior, TextureUtilities.PositionType anchorPoint) : 
            base(name, "null", relPos, Color.White, superior, anchorPoint)
        {
            //Relative offset absolute size
            Set = new List<UIElement>();
            BasePixelSize = absoluteSize;
            IsRelativeToSuperior = false;
        }

        public UIPanel(string name, Coordinate pixelOffset, Vector2 relSize, UIPanel superior, TextureUtilities.PositionType anchorPoint,
            TextureUtilities.PositionType superAnchorPoint) : base(name, "null", pixelOffset, Color.White, superior, anchorPoint, superAnchorPoint)
        {
            //Absolute offset relative size
            Set = new List<UIElement>();
            BaseSizeRel = relSize;
            if (MathD.IsClose(relSize.X, 0)) HorizontalLocked = true;
            if (MathD.IsClose(relSize.Y, 0)) VerticalLocked = true;
            IsRelativeToSuperior = true;
        }

        public UIPanel(string name, Vector2 relPos, Vector2 relSize, UIPanel superior, TextureUtilities.PositionType anchorPoint) : 
            base(name, "null", relPos, Color.White, superior, anchorPoint)
        {
            //Relative offset relative size
            Set = new List<UIElement>();
            BaseSizeRel = relSize;
            if (MathD.IsClose(relSize.X, 0)) HorizontalLocked = true;
            if (MathD.IsClose(relSize.Y, 0)) VerticalLocked = true;
            IsRelativeToSuperior = true;
        }

        public UIPanel(string name, Coordinate pixelOffset, Coordinate absoluteSize, UIElement superior, TextureUtilities.PositionType anchorPoint, 
            TextureUtilities.PositionType superAnchorPoint) : base(name, "null", pixelOffset, Color.White, superior, anchorPoint, superAnchorPoint)
        {
            //Absolute offset absolute size
            Set = new List<UIElement>();
            BasePixelSize = absoluteSize;
            IsRelativeToSuperior = false;
        }

        public UIPanel(string name, Vector2 relPos, Coordinate absoluteSize, TextureUtilities.PositionType anchorPoint) : base(name, relPos, anchorPoint)
        {
            //Relative offset absolute size King
            Set = new List<UIElement>();
            BasePixelSize = absoluteSize;
            IsRelativeToSuperior = false;
            King = true;
        }
        public UIPanel(string name, Vector2 relPos, Vector2 relSize, TextureUtilities.PositionType anchorPoint) : base(name, relPos, anchorPoint)
        {
            //Relative offset relative size King
            Set = new List<UIElement>();
            BaseSizeRel = relSize;
            if (MathD.IsClose(relSize.X, 0)) HorizontalLocked = true;
            if (MathD.IsClose(relSize.Y, 0)) VerticalLocked = true;
            IsRelativeToSuperior = true;
            King = true;
        }

        public virtual void AddElements(params UIElement[] elements)
        {
            Set.AddRange(elements);
        }

        public virtual void AddTooltip(Coordinate size, Vector2 offset, params UIElement[] elements)
        {
            Tooltip = new UIPanel(GivenName+"Tooltip", new Coordinate(), size, this, TextureUtilities.PositionType.TopLeft, TextureUtilities.PositionType.TopLeft);
            Tooltip.AddElements(elements);
            TooltipOffset = Utilities.Copy(offset);
        }

        public override void ReinterpretScale(Coordinate window)
        {
            //only for non relative sized panels
            if (!Loaded) return;
            CurrentScale = ScaleRules.GetScale(window);
            CurrentPixelSize = new Coordinate((int)(CurrentScale * BasePixelSize.X), (int)(CurrentScale * BasePixelSize.Y));
            TextureOffsetToOrigin = TextureUtilities.GetOffset(AnchorType, CurrentPixelSize);
        }

        public override void LoadContent(GraphicsDevice graphics, ContentManager content)
        {
            base.LoadContent(graphics, content);
            Tooltip?.LoadContent(graphics, content);
            foreach (var item in Set) item.LoadContent(graphics, content);
        }

        public override void Draw(SpriteBatch spriteBatch, Coordinate rightDownOffset)
        {
            if (!Loaded || DontDrawThis) return;
            foreach (var item in Set) item.Draw(spriteBatch, rightDownOffset);
            if (King) DrawFinal(spriteBatch);

            if (D.Bug) LineDrawer.DrawSquare(spriteBatch, (Vector2)(OriginPosition + rightDownOffset), CurrentPixelSize.X, CurrentPixelSize.Y, Color.White, 5);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (!Loaded) return;
            foreach (var item in Set) item.Draw(spriteBatch);
            if (King) DrawFinal(spriteBatch);

            if (D.Bug) LineDrawer.DrawSquare(spriteBatch, (Vector2)OriginPosition, CurrentPixelSize.X, CurrentPixelSize.Y, Color.White, 5);
        }

        public void DrawFinal(SpriteBatch spriteBatch)
        {
            foreach (var item in Set)
            {
                if (item is UIPanel p) p.DrawFinal(spriteBatch);
                else if (item is UIMovable m) m.DrawFinal(spriteBatch);
            }
            if (Over) Tooltip?.Draw(spriteBatch, new Coordinate(LastMousePos.X - OriginPosition.X, LastMousePos.Y - OriginPosition.Y) + (Coordinate)(CurrentScale * TooltipOffset));
        }

        public override void OnResize(GraphicsDevice graphics, Coordinate gameWindow)
        {
            if (!Loaded || WasResized(gameWindow)) return;

            if (King)
            {
                if (VerticalLocked) BaseSizeRel.X = MathHelper.Clamp(BaseSizeRel.X, 1f / gameWindow.X, 1.0f);
                if (HorizontalLocked) BaseSizeRel.Y = MathHelper.Clamp(BaseSizeRel.Y, 1f / gameWindow.Y, 1.0f);

                CurrentPixelSize = new Coordinate(gameWindow * BaseSizeRel);
                TextureOffsetToOrigin = TextureUtilities.GetOffset(AnchorType, CurrentPixelSize);

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
                    OriginPosition = (Coordinate)(gameWindow * PositionRelativeToBase) - TextureOffsetToOrigin;
                }
            }
            else if (IsRelativeToSuperior)
            {
                if (VerticalLocked) BaseSizeRel.X = MathHelper.Clamp(BaseSizeRel.X, 1f / BaseElement.CurrentPixelSize.X, 1.0f);
                if (HorizontalLocked) BaseSizeRel.Y = MathHelper.Clamp(BaseSizeRel.Y, 1f / BaseElement.CurrentPixelSize.Y, 1.0f);

                CurrentPixelSize = new Coordinate(BaseElement.CurrentPixelSize * BaseSizeRel);
                TextureOffsetToOrigin = TextureUtilities.GetOffset(AnchorType, CurrentPixelSize);
                LastResizeWindow = gameWindow;
                if (!BaseElement.WasResized(gameWindow)) BaseElement.OnResize(graphics, gameWindow);

                if (AbsoluteOffset)
                {
                    //Position of this element is relative to the origin of its base element in scaled pixels
                    var scaledOffset = new Coordinate(
                        (int)(BaseElement.CurrentScale * PixelOffsetFromBase.X),
                        (int)(BaseElement.CurrentScale * PixelOffsetFromBase.Y));
                    var superOffset = TextureUtilities.GetOffset(SuperAnchorType, BaseElement.CurrentPixelSize);
                    OriginPosition = scaledOffset + BaseElement.OriginPosition - TextureOffsetToOrigin + superOffset;
                }
                else
                {
                    //Position of this element is relative to the space of its base panel
                    OriginPosition = (Coordinate)(BaseElement.CurrentPixelSize * PositionRelativeToBase) + BaseElement.OriginPosition - TextureOffsetToOrigin;
                }
            }
            else base.OnResize(graphics, gameWindow);

            Tooltip?.OnResize(graphics, gameWindow);
            foreach (var item in Set) item.OnResize(graphics, gameWindow);
        }

        public override bool IsOver(Point mousePos)
        {
            if (Disabled) return false;
            int buttonX = (int)((mousePos.X - OriginPosition.X) / CurrentScale);
            int buttonY = (int)((mousePos.Y - OriginPosition.Y) / CurrentScale);
            return buttonX >= 0 && buttonY >= 0 && buttonX < CurrentPixelSize.X / CurrentScale && buttonY < CurrentPixelSize.Y / CurrentScale;
        }

        public override bool OnMoveMouse(Point point)
        {
            var found = false;
            for (int i = Set.Count - 1; i >= 0; i--)
            {
                if (Set[i] is UIClickable click)
                {
                    if (found) click.ForceNotOver();
                    else if (click.OnMoveMouse(point)) found = true;
                }
            }
            if (!Disabled && Tooltip != null)
            {
                LastMousePos = point;
                Over = IsOver(point);
                return Over;
            }

            return false;
        }

        public override bool OnLeftClick(Point point)
        {
            for (int i = Set.Count - 1; i >= 0; i--)
            {
                if (Set[i] is UIClickable click)
                {
                    if (click.OnLeftClick(point)) return true;
                }
            }

            Down = IsOver(point); //don't return this because the top level panel will steal it every time
            return false;
        }

        public override void OnLeftRelease(Point point)
        {
            if (!Disabled) base.OnLeftRelease(point);
            foreach (var item in Set)
            {
                if (item is UIClickable click) click.OnLeftRelease(point);
            }
        }

        public override void Enable()
        {
            foreach (var item in Set)
            {
                if (item is UIClickable click) click.Enable();
            }
            Tooltip?.Enable();
            Disabled = false;
        }

        public override void Disable(ColorScale c)
        {
            foreach (var item in Set)
            {
                if (item is UIClickable click) click.Disable(c);
            }
            Tooltip?.Disable(c);
            Disabled = true;
        }

        public virtual UIElement GetElement(string title)
        {
            if (GivenName == title) return this;

            var top = Set.FirstOrDefault(s => s.GivenName == title);
            if (top != null) return top;

            foreach (var element in Set)
            {
                if (element is UIPanel panel)
                {
                    var rtrn = panel.GetElement(title);
                    if (rtrn != null) return rtrn;
                }
            }

            return Tooltip?.GetElement(title);
        }

    }
}
