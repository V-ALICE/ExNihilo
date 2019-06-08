using ExNihilo.UI.Bases;
using ExNihilo.Util;
using ExNihilo.Util.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace ExNihilo.UI
{
    public class UIElement : IUI, IScalable
    {
        public enum PositionType
        {
            TopLeft, TopRight,
            BottomLeft, BottomRight,
            CenterTop, CenterBottom,
            CenterLeft, CenterRight,
            Center,
        }

        protected PositionType AnchorType, SuperAnchorType;
        protected Texture2D Texture;
        protected ColorScale ColorScale;
        protected Vector2 PositionRelativeToBase, TextureOffsetToOrigin;
        protected Coordinate PixelOffsetFromBase, LastResizeWindow;
        protected ScaleRuleSet ScaleRules;
        protected UIElement BaseElement;
        protected float CurrentScale;
        protected readonly string TexturePath;
        protected bool AbsoluteOffset, Loaded;

        public string GivenName { get; protected set; }
        public Vector2 OriginPosition { get; protected set; }
        public Coordinate CurrentPixelSize { get; protected set; }

        public UIElement(string name, string path, Vector2 relPos, UIPanel superior, PositionType anchorPoint)
        {
            ExceptionCheck.AssertCondition(relPos.X >= 0 && relPos.X <= 1.0 && relPos.Y >= 0 && relPos.Y <= 1.0);
            TexturePath = path;
            PositionRelativeToBase = Utilities.Copy(relPos);
            CurrentScale = 1;
            AnchorType = anchorPoint;
            AbsoluteOffset = false;
            BaseElement = superior;
            GivenName = name;
        }

        public UIElement(string name, string path, Coordinate pixelOffset, UIElement superior, PositionType anchorPoint, PositionType superAnchorType)
        {
            //Pixel offset is always relative to the top 
            TexturePath = path;
            PixelOffsetFromBase = pixelOffset.Copy();
            CurrentScale = 1;
            AnchorType = anchorPoint;
            SuperAnchorType = superAnchorType;
            AbsoluteOffset = true;
            BaseElement = superior;
            GivenName = name;
        }

        protected UIElement(string name, Vector2 relPos, PositionType anchorPoint)
        {
            //King element. Panel that encompasses everything else
            ExceptionCheck.AssertCondition(relPos.X >= 0 && relPos.X <= 1.0 && relPos.Y >= 0 && relPos.Y <= 1.0);
            TexturePath = "null";
            PositionRelativeToBase = Utilities.Copy(relPos);
            CurrentScale = 1;
            AnchorType = anchorPoint;
            AbsoluteOffset = false;
            GivenName = name;
        }

        public void SetColorScale(ColorScale scale)
        {
            ColorScale = scale;
        }

        public void SetRules(ScaleRuleSet rules)
        {
            ScaleRules = rules;
        }

        public virtual void ReinterpretScale(Coordinate window)
        {
            if (!Loaded) return;
            CurrentScale = ScaleRules.GetScale(window);
            CurrentPixelSize = new Coordinate((int)(CurrentScale * Texture.Width), (int)(CurrentScale * Texture.Height));
            TextureOffsetToOrigin = GetOffset(AnchorType, CurrentPixelSize);
        }

        public virtual void LoadContent(GraphicsDevice graphics, ContentManager content)
        {
            Loaded = true;
            if (ScaleRules is null) ScaleRules = UILibrary.DefaultScaleRuleSet;
            Texture = UILibrary.TextureLookUp[TexturePath];
            if (CurrentPixelSize is null) CurrentPixelSize = new Coordinate((int) (CurrentScale*Texture.Width), (int) (CurrentScale*Texture.Height));
            TextureOffsetToOrigin = GetOffset(AnchorType, CurrentPixelSize);
            LastResizeWindow = new Coordinate();
        }

        protected static Vector2 GetOffset(PositionType anchorType, Coordinate pixelSize)
        {
            Vector2 textureOffsetToOrigin;
            switch (anchorType)
            {
                case PositionType.TopLeft:
                    textureOffsetToOrigin =  new Vector2(0, 0);
                    break;
                case PositionType.TopRight:
                    textureOffsetToOrigin =  new Vector2(pixelSize.X, 0);
                    break;
                case PositionType.BottomLeft:
                    textureOffsetToOrigin =  new Vector2(0, pixelSize.Y);
                    break;
                case PositionType.BottomRight:
                    textureOffsetToOrigin =  new Vector2(pixelSize.X, pixelSize.Y);
                    break;
                case PositionType.CenterTop:
                    textureOffsetToOrigin =  new Vector2(pixelSize.X / 2, 0);
                    break;
                case PositionType.CenterBottom:
                    textureOffsetToOrigin =  new Vector2(pixelSize.X / 2, pixelSize.Y);
                    break;
                case PositionType.CenterLeft:
                    textureOffsetToOrigin =  new Vector2(0, pixelSize.Y / 2);
                    break;
                case PositionType.CenterRight:
                    textureOffsetToOrigin =  new Vector2(pixelSize.X, pixelSize.Y / 2);
                    break;
                case PositionType.Center:
                    textureOffsetToOrigin =  new Vector2(pixelSize.X / 2, pixelSize.Y / 2);
                    break;
                default:
                    textureOffsetToOrigin = new Vector2();
                    break;
            }

            return textureOffsetToOrigin;
        }

        public bool WasResized(Coordinate gameWindow)
        {
            return LastResizeWindow.Equals(gameWindow);
        }

        public virtual void OnResize(GraphicsDevice graphics, Coordinate gameWindow)
        {
            if (!Loaded || WasResized(gameWindow)) return;

            ReinterpretScale(gameWindow);
            LastResizeWindow = gameWindow;
            if (!BaseElement.WasResized(gameWindow)) BaseElement.OnResize(graphics, gameWindow);

            if (AbsoluteOffset)
            {
                //Position of this element is relative to the origin of its base element in scaled pixels
                var scaledOffset = new Coordinate(
                    (int) (BaseElement.CurrentScale * PixelOffsetFromBase.X), 
                    (int) (BaseElement.CurrentScale * PixelOffsetFromBase.Y));
                var superOffset = GetOffset(SuperAnchorType, BaseElement.CurrentPixelSize);
                OriginPosition = BaseElement.OriginPosition + scaledOffset - TextureOffsetToOrigin + superOffset;
            }
            else  
            {
                //Position of this element is relative to the space of its base panel
                OriginPosition = BaseElement.OriginPosition + BaseElement.CurrentPixelSize * PositionRelativeToBase - TextureOffsetToOrigin;
            }
        }

        public virtual void Draw(SpriteBatch spriteBatch)
        {
            if (!Loaded) return;
            spriteBatch.Draw(Texture, OriginPosition, null, ColorScale?.Get() ?? Color.White, 0, Vector2.Zero, CurrentScale, SpriteEffects.None, 0);
        }
        public void Draw(SpriteBatch spriteBatch, ColorScale c)
        {
            if (!Loaded) return;
            spriteBatch.Draw(Texture, OriginPosition, null, c, 0, Vector2.Zero, CurrentScale, SpriteEffects.None, 0);
        }

    }
}
