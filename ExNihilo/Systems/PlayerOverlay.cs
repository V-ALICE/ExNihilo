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
        private readonly ScaleRuleSet _rules = TextureLibrary.HalfScaleRuleSet;
        private AnimatableTexture _texture;
        private float _currentScale;
        private readonly string _texturePath;
        private readonly int _frames, _fps;

        public Vector2 PlayerCenterScreen { get; private set; }

        public PlayerOverlay(string texturePath, int frames=1, int fps=1)
        {
            _frames = frames;
            _fps = fps;
            _texturePath = texturePath;
            _currentScale = 1;
            PlayerCenterScreen = new Vector2();
        }

        public void LoadContent(GraphicsDevice graphics, ContentManager content)
        {
            _texture = new AnimatableTexture(TextureLibrary.Lookup(_texturePath), _frames, _fps);
        }

        public void OnResize(GraphicsDevice graphics, Coordinate gameWindow)
        {
            _currentScale = _rules.GetScale(gameWindow);
            PlayerCenterScreen = new Vector2(gameWindow.X / 2, gameWindow.Y / 2) - TextureUtilities.GetOffset(TextureUtilities.PositionType.Center, _texture);
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            _texture.Draw(spriteBatch, PlayerCenterScreen, ColorScale.White, _currentScale);
        }

        public Coordinate GetCurrentDimensions()
        {
            return new Coordinate(_currentScale*_texture.Width, _currentScale*_texture.Height);
        }

        public void ForceTexture(AnimatableTexture texture)
        {
            _texture = texture;
        }
    }
}
