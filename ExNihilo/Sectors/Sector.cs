using System.Threading;
using ExNihilo.Input.Commands;
using ExNihilo.UI;
using ExNihilo.Util;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace ExNihilo.Sectors
{
    public abstract class Sector : IUI, IClickable
    {
        protected CommandHandler handler;
        protected Thread loadingThread;

        public abstract void OnResize(GraphicsDevice graphicsDevice, Coordinate window);
        public abstract void Initialize();
        public abstract void LoadContent(GraphicsDevice graphicsDevice, ContentManager content);
        public abstract void Update();
        protected abstract void DrawDebugInfo();
        public abstract void Draw(SpriteBatch spriteBatch, bool drawDebugInfo);
        public void Draw(SpriteBatch spriteBatch)
        {
            Draw(spriteBatch, false);
        }
        public abstract void OnExit();

        public abstract void OnMoveMouse(Point point);
        public abstract void OnLeftClick(Point point);
        public abstract void OnLeftRelease();
    }
}
