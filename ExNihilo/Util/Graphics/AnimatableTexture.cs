using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ExNihilo.Util.Graphics
{
    public class AnimatableTexture
    {
        private int _frameNow;
        private readonly int _timerID=-1;
        private readonly bool _animated;
        private readonly int _frameCount, _fps;
        private byte[] _alphaMask;

        public int Width { get; }
        public int Height => TextureStrip.Height;
        public readonly Texture2D TextureStrip;

        public AnimatableTexture(Texture2D texture, int frameCount=1, int framesPerSec=1)
        {
            _frameCount = frameCount;
            TextureStrip = texture;
            Width = TextureStrip.Width / _frameCount;
            _frameNow = 0;
            _fps = framesPerSec;
            if (frameCount > 1)
            {
                _timerID = UniversalTime.NewTimer(false, 1d / framesPerSec);
                UniversalTime.TurnOnTimer(_timerID);
                _animated = true;
            }
        }
        ~AnimatableTexture()
        {
            if (_timerID >= 0) UniversalTime.SellTimer(_timerID);
        }

        public static implicit operator AnimatableTexture(Texture2D t)
        {
            return new AnimatableTexture(t);
        }

        public static implicit operator Texture2D(AnimatableTexture t)
        {
            return t.TextureStrip;
        }

        public AnimatableTexture Copy()
        {
            return new AnimatableTexture(TextureStrip, _frameCount, _fps);
        }

        public void ResetFrame()
        {
            if (_timerID >= 0) UniversalTime.ResetTimer(_timerID);
            _frameNow = 0;
        }
        public void AdjustFPS(int newFPS)
        {
            if (_timerID >= 0) UniversalTime.RecycleTimer(_timerID, 1d / newFPS);
        }
        public void ToggleAnimation(bool on)
        {
            if (_timerID < 0) return;
            if (on) UniversalTime.TurnOnTimer(_timerID);
            else UniversalTime.TurnOffTimer(_timerID);
        }

        public byte[] GetAlphaMask()
        {
            if (_alphaMask is null)
            {
                var data = new Color[TextureStrip.Width * TextureStrip.Height];
                TextureStrip.GetData(data);
                _alphaMask = new byte[data.Length];
                for (int i = 0; i < data.Length; i++) _alphaMask[i] = data[i].A;
            }
            return _alphaMask;
        }

        public void Draw(SpriteBatch batch, Vector2 screenPos, ColorScale color, float scale, float rotation = 0,
            TextureUtilities.PositionType origin = TextureUtilities.PositionType.TopLeft, SpriteEffects effect = SpriteEffects.None)
        {
            var originVec = Vector2.Zero;
            if (origin != TextureUtilities.PositionType.TopLeft)
                originVec = (Vector2)TextureUtilities.GetOffset(origin, new Coordinate(Width, Height));

            if (_animated)
            {
                var frameShifts = UniversalTime.GetNumberOfFires(_timerID);
                if (frameShifts > 0) _frameNow = (_frameNow + frameShifts) % _frameCount;
                var rect = new Rectangle(Width * _frameNow, 0, Width, TextureStrip.Height);
                batch.Draw(TextureStrip, screenPos, rect, color, rotation, originVec, scale, effect, 0);
            }
            else
            {
                batch.Draw(TextureStrip, screenPos, null, color, rotation, originVec, scale, effect, 0);
            }
        }

        public void Draw(SpriteBatch batch, Coordinate screenPos, ColorScale color, float scale, float rotation = 0,
            TextureUtilities.PositionType origin = TextureUtilities.PositionType.TopLeft, SpriteEffects effect = SpriteEffects.None)
        {
            Draw(batch, (Vector2)screenPos, color, scale, rotation, origin, effect);
        }

        public void Draw(SpriteBatch batch, Vector2 screenPos, ColorScale color, Vector2 scale, float rotation = 0,
            TextureUtilities.PositionType origin = TextureUtilities.PositionType.TopLeft, SpriteEffects effect = SpriteEffects.None)
        {
            var originVec = Vector2.Zero;
            if (origin != TextureUtilities.PositionType.TopLeft)
                originVec = (Vector2)TextureUtilities.GetOffset(origin, new Coordinate(Width, Height));

            if (_animated)
            {
                var frameShifts = UniversalTime.GetNumberOfFires(_timerID);
                if (frameShifts > 0) _frameNow = (_frameNow + frameShifts) % _frameCount;
                var rect = new Rectangle(Width * _frameNow, 0, Width, TextureStrip.Height);
                batch.Draw(TextureStrip, screenPos, rect, color, rotation, originVec, scale, effect, 0);
            }
            else
            {
                batch.Draw(TextureStrip, screenPos, null, color, rotation, originVec, scale, effect, 0);
            }
        }

        public void Draw(SpriteBatch batch, Coordinate screenPos, ColorScale color, Vector2 scale, float rotation = 0,
            TextureUtilities.PositionType origin = TextureUtilities.PositionType.TopLeft, SpriteEffects effect = SpriteEffects.None)
        {
            Draw(batch, (Vector2)screenPos, color, scale, rotation, origin, effect);
        }
    }
}