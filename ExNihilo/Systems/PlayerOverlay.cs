using ExNihilo.UI.Bases;
using ExNihilo.Util;
using ExNihilo.Util.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace ExNihilo.Systems
{
    public class PlayerOverlay : IUI
    {
        private Vector2  _playerCenterScreen;
        private readonly ScaleRuleSet _rules = TextureLibrary.HalfScaleRuleSet;
        private float _currentScale;
        private AnimatableTexture _texture;
        private readonly string _texturePath;
        private readonly int _frames, _fps;
        private Coordinate _textureSize;

        public PlayerOverlay(string texturePath, int frames=1, int fps=1)
        {
            _frames = frames;
            _fps = fps;
            _texturePath = texturePath;
            _currentScale = 1;
            _playerCenterScreen = new Vector2();
            _textureSize = new Coordinate();
        }

        public void LoadContent(GraphicsDevice graphics, ContentManager content)
        {
            _texture = new AnimatableTexture(TextureLibrary.Lookup(_texturePath), _frames, _fps);
            _textureSize = new Coordinate(_texture.Width, _texture.Height);
        }

        public void OnResize(GraphicsDevice graphics, Coordinate gameWindow)
        {
            _currentScale = _rules.GetScale(gameWindow);
            _playerCenterScreen = new Vector2(gameWindow.X / 2, gameWindow.Y / 2) - TextureUtilities.GetOffset(TextureUtilities.PositionType.Center, _textureSize);
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            _texture.Draw(spriteBatch, _playerCenterScreen, ColorScale.White, _currentScale);
        }
    }
}
