using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ExNihilo.Util.Graphics
{
    public class AnimatedTexture
    {
        private int _frameNow;
        private readonly int _timerID;
        private readonly Texture2D _textureStrip;
        private readonly bool _animated;

        public readonly int FrameCount;

        public AnimatedTexture(Texture2D texture, int frameCount, int framesPerSec)
        {
            ExceptionCheck.AssertCondition(framesPerSec > 0, "framesPerSec="+framesPerSec);
            ExceptionCheck.AssertCondition(frameCount > 0, "frameCount=" + frameCount);

            FrameCount = frameCount;
            _textureStrip = texture;
            _frameNow = 0;
            _animated = frameCount == 1;
            _timerID = UniversalTime.NewTimer(false, 1d / framesPerSec);
            UniversalTime.TurnOnTimer(_timerID);
        }

        ~AnimatedTexture()
        {
            UniversalTime.SellTimer(_timerID);
        }

        public void UpdateFrame()
        {
            if (!_animated) return;
            var frameShifts = UniversalTime.GetNumberOfFires(_timerID);
            if (frameShifts > 0) _frameNow = (_frameNow + frameShifts) % FrameCount;
        }

        public void AdjustFPS(int newFPS)
        {
            ExceptionCheck.AssertCondition(newFPS > 0, "newFPS=" + newFPS);
            UniversalTime.RecycleTimer(_timerID, 1d / newFPS);
        }

        public void ToggleAnimation(bool on)
        {
            if (on) UniversalTime.TurnOnTimer(_timerID);
            else UniversalTime.TurnOffTimer(_timerID);
        }

        private Rectangle GetRect()
        {
            var frameWidth = _textureStrip.Width / FrameCount;
            return new Rectangle(frameWidth * _frameNow, 0, frameWidth, _textureStrip.Height);
        }

        public void Draw(SpriteBatch batch, Vector2 screenPos, Color color, float rotation, float size)
        {
            var adjustedOrigin = new Vector2(0, _textureStrip.Height);
            batch.Draw(_textureStrip, screenPos, GetRect(), Color.White, rotation, adjustedOrigin, size, SpriteEffects.None, 0.5f);
        }

        public void Draw(SpriteBatch batch, Vector2 screenPos)
        {
            var adjustedOrigin = new Vector2(0, _textureStrip.Height);
            batch.Draw(_textureStrip, screenPos, GetRect(), Color.White, 0, adjustedOrigin, 1f, SpriteEffects.None, 0.5f);
        }
    }
}