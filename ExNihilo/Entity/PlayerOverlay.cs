using ExNihilo.Systems;
using ExNihilo.Systems.Backend;
using ExNihilo.UI.Bases;
using ExNihilo.Util;
using ExNihilo.Util.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace ExNihilo.Entity
{
    public class PlayerOverlay : IUI
    {
        private readonly ScaleRuleSet _rules = TextureLibrary.HalfScaleRuleSet;
        private EntityContainer _entity;
        public readonly long ID;
        private readonly bool _useCustomPos;
        public string Name => _entity.Name;
        public float Scale { get; private set; }

        public Coordinate PlayerCenterScreen { get; private set; }
        public Vector2 PlayerCustomWorldPos;

        public PlayerOverlay(EntityContainer entity, bool centered, long id=0)
        {
            Scale = 1;
            PlayerCenterScreen = new Coordinate();
            _entity = entity;
            _useCustomPos = !centered;
            ID = id;
        }

        public void LoadContent(GraphicsDevice graphics, ContentManager content)
        {
        }

        public void OnResize(GraphicsDevice graphics, Coordinate gameWindow)
        {
            Scale = _rules.GetScale(gameWindow);
            PlayerCenterScreen = new Coordinate(gameWindow.X / 2, gameWindow.Y / 2) - TextureUtilities.GetOffset(TextureUtilities.PositionType.Center, _entity.Texture);
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            if (!_useCustomPos) _entity.Texture.Draw(spriteBatch, PlayerCenterScreen, ColorScale.White, Scale);
        }

        public void Draw(SpriteBatch spriteBatch, Vector2 worldPos, float curScale, float drawScale)
        {
            if (_useCustomPos)
            {
                //customPos will be the worldPos of the world that makes this overlay centered
                var pos = worldPos + curScale * PlayerCustomWorldPos;
                _entity.Texture.Draw(spriteBatch, pos, ColorScale.White, drawScale);
            }
            else _entity.Texture.Draw(spriteBatch, PlayerCenterScreen, ColorScale.White, Scale);
        }

        public void Push(Coordinate push)
        {
            _entity.Push(push);
        }
        public EntityTexture.State GetCurrentState()
        {
            return _entity.Entity.CurrentState;
        }
        public void Halt()
        {
            _entity.Push(new Coordinate());

        }

        public void ForceEntityRef(EntityContainer entity)
        {
            _entity = entity;
        }

        public object[] GetStandardUpdateArray(long id)
        {
            return new object[] { id, PlayerCustomWorldPos.X, PlayerCustomWorldPos.Y, GetCurrentState() };
        }
        public void ForceValues(float x, float y, sbyte type)
        {
            PlayerCustomWorldPos.X = x;
            PlayerCustomWorldPos.Y = y;
            _entity.Entity.SetState((EntityTexture.State) type);
        }
    }
}
