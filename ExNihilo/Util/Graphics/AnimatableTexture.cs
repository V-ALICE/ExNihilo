using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ExNihilo.Util.Graphics
{
    public class AnimatableTexture
    {
        private int _frameNow;
        private readonly int _timerID;
        private readonly Texture2D _textureStrip;
        private readonly bool _animated;
        private readonly int _frameCount;
        private byte[] _alphaMask;

        public int Width { get; }
        public int Height => _textureStrip.Height;

        public AnimatableTexture(Texture2D texture, int frameCount=1, int framesPerSec=1)
        {
            _frameCount = frameCount;
            _textureStrip = texture;
            Width = _textureStrip.Width / _frameCount;
            _frameNow = 0;
            if (frameCount > 1)
            {
                _timerID = UniversalTime.NewTimer(false, 1d / framesPerSec);
                UniversalTime.TurnOnTimer(_timerID);
                _animated = true;
            }
        }
        ~AnimatableTexture()
        {
            UniversalTime.SellTimer(_timerID);
            _textureStrip.Dispose();
        }

        public void AdjustFPS(int newFPS)
        {
            UniversalTime.RecycleTimer(_timerID, 1d / newFPS);
        }
        public void ToggleAnimation(bool on)
        {
            if (on) UniversalTime.TurnOnTimer(_timerID);
            else UniversalTime.TurnOffTimer(_timerID);
        }

        public byte[] GetAlphaMask()
        {
            if (_alphaMask is null)
            {
                var data = new Color[_textureStrip.Width * _textureStrip.Height];
                _textureStrip.GetData(data);
                _alphaMask = new byte[data.Length];
                for (int i = 0; i < data.Length; i++) _alphaMask[i] = data[i].A;
            }
            return _alphaMask;
        }

        public void Draw(SpriteBatch batch, Vector2 screenPos, ColorScale color, float scale, float rotation = 0, TextureUtilities.PositionType origin = TextureUtilities.PositionType.TopLeft, SpriteEffects effect =SpriteEffects.None)
        {
            var originVec = Vector2.Zero;
            if (origin != TextureUtilities.PositionType.TopLeft) originVec = TextureUtilities.GetOffset(origin, new Coordinate(Width, _textureStrip.Height));

            if (_animated)
            {
                var frameShifts = UniversalTime.GetNumberOfFires(_timerID);
                if (frameShifts > 0) _frameNow = (_frameNow + frameShifts) % _frameCount;
                var rect = new Rectangle(Width * _frameNow, 0, Width, _textureStrip.Height);
                batch.Draw(_textureStrip, screenPos, rect, color, rotation, originVec, scale, effect, 0);
            }
            else
            {
                batch.Draw(_textureStrip, screenPos, null, color, rotation, originVec, scale, effect, 0);
            }
        }

        public void Draw(SpriteBatch batch, Vector2 screenPos, ColorScale color, Vector2 scale, float rotation = 0, TextureUtilities.PositionType origin = TextureUtilities.PositionType.TopLeft, SpriteEffects effect = SpriteEffects.None)
        {
            var originVec = Vector2.Zero;
            if (origin != TextureUtilities.PositionType.TopLeft) originVec = TextureUtilities.GetOffset(origin, new Coordinate(Width, _textureStrip.Height));

            if (_animated)
            {
                var frameShifts = UniversalTime.GetNumberOfFires(_timerID);
                if (frameShifts > 0) _frameNow = (_frameNow + frameShifts) % _frameCount;
                var rect = new Rectangle(Width * _frameNow, 0, Width, _textureStrip.Height);
                batch.Draw(_textureStrip, screenPos, rect, color, rotation, originVec, scale, effect, 0);
            }
            else
            {
                batch.Draw(_textureStrip, screenPos, null, color, rotation, originVec, scale, effect, 0);
            }
        }
    }
}