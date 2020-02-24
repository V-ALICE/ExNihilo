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
        private float _currentScale;

        public Coordinate PlayerCenterScreen { get; private set; }

        public PlayerOverlay(EntityContainer entity)
        {
            _currentScale = 1;
            PlayerCenterScreen = new Coordinate();
            _entity = entity;
        }

        public void LoadContent(GraphicsDevice graphics, ContentManager content)
        {
        }

        public void OnResize(GraphicsDevice graphics, Coordinate gameWindow)
        {
            _currentScale = _rules.GetScale(gameWindow);
            PlayerCenterScreen = new Coordinate(gameWindow.X / 2, gameWindow.Y / 2) - TextureUtilities.GetOffset(TextureUtilities.PositionType.Center, _entity.Texture);
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            _entity.Texture.Draw(spriteBatch, PlayerCenterScreen, ColorScale.White, _currentScale);
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

        public void ForceTexture(EntityContainer entity)
        {
            _entity = entity;
        }
    }
}
