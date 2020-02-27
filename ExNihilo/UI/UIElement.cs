using ExNihilo.Systems;
using ExNihilo.Systems.Backend;
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
        protected TextureUtilities.PositionType AnchorType, SuperAnchorType;
        protected AnimatableTexture Texture;
        protected ColorScale ColorScale;
        protected Vector2 PositionRelativeToBase;
        protected Coordinate PixelOffsetFromBase, LastResizeWindow, TextureOffsetToOrigin;
        protected ScaleRuleSet ScaleRules;
        protected UIElement BaseElement;
        protected string TexturePath;
        protected bool AbsoluteOffset, Loaded, DontDrawThis;

        public float CurrentScale { get; protected set; }
        public string GivenName { get; protected set; }
        public Coordinate OriginPosition { get; protected set; }
        public Coordinate CurrentPixelSize { get; protected set; }

        public UIElement(string name, string path, Vector2 relPos, ColorScale color, UIPanel superior, TextureUtilities.PositionType anchorPoint)
        {
            TexturePath = path;
            PositionRelativeToBase = Utilities.Copy(relPos);
            CurrentScale = 1;
            AnchorType = anchorPoint;
            AbsoluteOffset = false;
            BaseElement = superior;
            GivenName = name;
            ColorScale = color;
        }

        public UIElement(string name, string path, Coordinate pixelOffset, ColorScale color, UIElement superior, TextureUtilities.PositionType anchorPoint, TextureUtilities.PositionType superAnchorType)
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
            ColorScale = color;
        }

        protected UIElement(string name, Vector2 relPos, TextureUtilities.PositionType anchorPoint)
        {
            //King element. Panel that encompasses everything else
            TexturePath = "null";
            PositionRelativeToBase = Utilities.Copy(relPos);
            CurrentScale = 1;
            AnchorType = anchorPoint;
            AbsoluteOffset = false;
            GivenName = name;
            ColorScale = Color.White;
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
            TextureOffsetToOrigin = TextureUtilities.GetOffset(AnchorType, CurrentPixelSize);
        }

        public virtual void LoadContent(GraphicsDevice graphics, ContentManager content)
        {
            Loaded = true;
            if (ScaleRules is null) ScaleRules = TextureLibrary.DefaultScaleRuleSet;
            Texture = TextureLibrary.Lookup(TexturePath);
            if (CurrentPixelSize.Origin()) CurrentPixelSize = new Coordinate((int) (CurrentScale*Texture.Width), (int) (CurrentScale*Texture.Height));
            TextureOffsetToOrigin = TextureUtilities.GetOffset(AnchorType, CurrentPixelSize);
            LastResizeWindow = new Coordinate();
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
                var superOffset = TextureUtilities.GetOffset(SuperAnchorType, BaseElement.CurrentPixelSize);
                OriginPosition = scaledOffset + BaseElement.OriginPosition - TextureOffsetToOrigin + superOffset;
            }
            else  
            {
                //Position of this element is relative to the space of its base panel
                OriginPosition = (Coordinate)(BaseElement.CurrentPixelSize * PositionRelativeToBase) + BaseElement.OriginPosition - TextureOffsetToOrigin;
            }
        }

        public virtual void Draw(SpriteBatch spriteBatch, Coordinate rightDownOffset)
        {
            if (!Loaded || DontDrawThis) return;
            Texture.Draw(spriteBatch, OriginPosition + rightDownOffset, ColorScale?.Get() ?? Color.White, CurrentScale);
        }

        public virtual void Draw(SpriteBatch spriteBatch)
        {
            if (!Loaded || DontDrawThis) return;
            Texture.Draw(spriteBatch, OriginPosition, ColorScale?.Get() ?? Color.White, CurrentScale);
        }

        public void ChangeColor(ColorScale color)
        {
            ColorScale = color;
        }

        public void ResetTextureFrame()
        {
            Texture.ResetFrame();
        }

        public void SetNullTexture()
        {
            Texture = TextureLibrary.Lookup("null");
        }

        public virtual void ChangeTexture(AnimatableTexture texture)
        {
            TexturePath = "NotNull";
            Texture = texture.Copy();
            CurrentPixelSize = new Coordinate((int)(CurrentScale * Texture.Width), (int)(CurrentScale * Texture.Height));
            TextureOffsetToOrigin = TextureUtilities.GetOffset(AnchorType, CurrentPixelSize);

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

        public virtual void ChangeTexture(string path)
        {
            TexturePath = path;
            Texture = TextureLibrary.Lookup(TexturePath);
            CurrentPixelSize = new Coordinate((int)(CurrentScale * Texture.Width), (int)(CurrentScale * Texture.Height));
            TextureOffsetToOrigin = TextureUtilities.GetOffset(AnchorType, CurrentPixelSize);

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

        public virtual void AllowDraw(bool t)
        {
            DontDrawThis = !t;
        }

    }
}
