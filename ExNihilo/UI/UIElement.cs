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

        protected PositionType Type;
        protected Texture2D Texture;
        protected Vector2 Pos;
        protected Coordinate BaseSize;
        protected Vector2 PosRel, TextureOffset;
        protected ScaleRuleSet ScaleRules;
        protected float CurrentScale;
        protected readonly string TexturePath;
        protected readonly bool AbsoluteOffset;
        protected bool Loaded;

        public UIElement(string path, Vector2 relPos, PositionType t = PositionType.Center, bool pixelOffset = false)
        {
            ExceptionCheck.AssertCondition(pixelOffset || (relPos.X >= 0 && relPos.X <= 1.0 && relPos.Y >= 0 && relPos.Y <= 1.0));
            TexturePath = path;
            PosRel = Utilities.Copy(relPos);
            CurrentScale = 1;
            Type = t;
            AbsoluteOffset = pixelOffset;
        }

        public virtual void SetRules(ScaleRuleSet rules)
        {
            ScaleRules = rules;
        }

        public virtual void ReinterpretScale(Coordinate window)
        {
            CurrentScale = ScaleRules.GetScale(window);
            BaseSize = new Coordinate((int)(CurrentScale * Texture.Width), (int)(CurrentScale * Texture.Height));
            ReinterpretOffset();
        }

        public virtual void LoadContent(GraphicsDevice graphics, ContentManager content)
        {
            Loaded = true;
            ScaleRules = UILibrary.DefaultScaleRuleSet;
            Texture = UILibrary.TextureLookUp[TexturePath];
            if (BaseSize is null) BaseSize = new Coordinate((int) (CurrentScale*Texture.Width), (int) (CurrentScale*Texture.Height));
            ReinterpretOffset();
        }

        protected void ReinterpretOffset()
        {
            switch (Type)
            {
                case PositionType.TopLeft:
                    TextureOffset =  new Vector2(0, 0);
                    break;
                case PositionType.TopRight:
                    TextureOffset =  new Vector2(BaseSize.X, 0);
                    break;
                case PositionType.BottomLeft:
                    TextureOffset =  new Vector2(0, BaseSize.Y);
                    break;
                case PositionType.BottomRight:
                    TextureOffset =  new Vector2(BaseSize.X, BaseSize.Y);
                    break;
                case PositionType.CenterTop:
                    TextureOffset =  new Vector2(BaseSize.X / 2, 0);
                    break;
                case PositionType.CenterBottom:
                    TextureOffset =  new Vector2(BaseSize.X / 2, BaseSize.Y);
                    break;
                case PositionType.CenterLeft:
                    TextureOffset =  new Vector2(0, BaseSize.Y / 2);
                    break;
                case PositionType.CenterRight:
                    TextureOffset =  new Vector2(BaseSize.X, BaseSize.Y / 2);
                    break;
                case PositionType.Center:
                    TextureOffset =  new Vector2(BaseSize.X / 2, BaseSize.Y / 2);
                    break;
                default:
                    TextureOffset = new Vector2();
                    break;
            }
        }
        public virtual void OnResize(GraphicsDevice graphics, Coordinate gameWindow, Coordinate subWindow, Vector2 origin)
        {
            if (!Loaded) return;
            ReinterpretScale(gameWindow);
            Pos = AbsoluteOffset ? origin + PosRel - TextureOffset : origin + subWindow * PosRel - TextureOffset;
        }

        public virtual void Draw(SpriteBatch spriteBatch)
        {
            if (!Loaded) return;
            spriteBatch.Draw(Texture, Pos, null, Color.White, 0, Vector2.Zero, CurrentScale, SpriteEffects.None, 0);
        }

    }
}
