using System.Threading;
using ExNihilo.Input.Commands;
using ExNihilo.UI.Bases;
using ExNihilo.Util;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace ExNihilo.Sectors
{
    public abstract class Sector : IUI, IClickable
    {
        protected CommandHandler Handler;
        protected Thread LoadingThread;

        public abstract void OnResize(GraphicsDevice graphicsDevice, Coordinate window, Vector2 origin);
        public abstract void Initialize();
        public abstract void LoadContent(GraphicsDevice graphicsDevice, ContentManager content);
        public abstract void Update();
        protected abstract void DrawDebugInfo();
        public abstract void Draw(SpriteBatch spriteBatch, bool drawDebugInfo);
        public void Draw(SpriteBatch spriteBatch)
        {
            Draw(spriteBatch, false);
        }

        public virtual void OnExit()
        {
            LoadingThread?.Abort();
            LoadingThread?.Join();
        }

        public abstract void OnMoveMouse(Point point);
        public abstract void OnLeftClick(Point point);
        public abstract void OnLeftRelease();
    }
}
