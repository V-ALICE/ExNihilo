using ExNihilo.Systems;
using ExNihilo.Systems.Backend;
using ExNihilo.Systems.Backend.Network;
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
        private readonly ScaleRuleSet _rules = TextureLibrary.DefaultScaleRuleSet;
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
            if (!_useCustomPos) _entity.Texture.Draw(spriteBatch, PlayerCenterScreen, ColorScale.Grey, Scale);
        }

        public void Draw(SpriteBatch spriteBatch, Vector2 pos, float drawScale)
        {
            if (_useCustomPos)
            {
                //customPos will be the worldPos of the world that makes this overlay centered
                _entity.Texture.Draw(spriteBatch, pos, ColorScale.Grey, drawScale);
            }
            else _entity.Texture.Draw(spriteBatch, PlayerCenterScreen, ColorScale.Grey, Scale);
        }

        public void Push(Coordinate push)
        {
            _entity.Push(push);
        }
        public EntityTexture.State GetCurrentState()
        {
            return _entity.Entity.CurrentState;
        }
        public AnimatableTexture GetStateTexture(EntityTexture.State state)
        {
            return _entity.Entity.GetTexture(state);
        }
        public void Halt()
        {
            _entity.Push(new Coordinate());

        }

        public Coordinate GetCurrentPixelSize()
        {
            return new Coordinate(Scale * _entity.Texture.Width, Scale * _entity.Texture.Height);
        }

        public void ForceEntityRef(EntityContainer entity)
        {
            _entity = entity;
        }

        public StandardUpdate GetStandardUpdate(long id)
        {
            return new StandardUpdate(id, PlayerCustomWorldPos.X, PlayerCustomWorldPos.Y, (sbyte) GetCurrentState());
        }
        public void ForceValues(float x, float y, sbyte type)
        {
            PlayerCustomWorldPos.X = x;
            PlayerCustomWorldPos.Y = y;
            _entity.Entity.SetState((EntityTexture.State) type);
        }
    }
}
