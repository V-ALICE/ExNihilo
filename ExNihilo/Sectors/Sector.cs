using System.Threading;
using ExNihilo.Input.Commands;
using ExNihilo.Menus;
using ExNihilo.Systems;
using ExNihilo.Systems.Bases;
using ExNihilo.UI.Bases;
using ExNihilo.Util;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace ExNihilo.Sectors
{
    public abstract class Sector : IUI, IClickable, ISavable
    {
        protected GameContainer Container;
        protected CommandHandler Handler;
        protected Thread LoadingThread;

        protected Sector(GameContainer container)
        {
            Container = container;
        }

        public abstract void OnResize(GraphicsDevice graphicsDevice, Coordinate gameWindow);
        public abstract void Enter();
        public abstract void Initialize();
        public abstract void LoadContent(GraphicsDevice graphicsDevice, ContentManager content);
        public abstract void Update();
        protected abstract void DrawDebugInfo();
        public abstract void Draw(SpriteBatch spriteBatch, bool drawDebugInfo);
        public void Draw(SpriteBatch spriteBatch)
        {
            Draw(spriteBatch, false);
        }

        public void OnExit()
        {
            LoadingThread?.Abort();
            LoadingThread?.Join();
            Exit();
        }
        protected abstract void Exit();

        public virtual void BackOut()
        {
            RequestSectorChange(GameContainer.SectorID.MainMenu);
        }
        public virtual void ReceiveCommand(Menu.MenuCommand command)
        {
        }

        public void RequestSectorChange(GameContainer.SectorID sector)
        {
            Container.RequestSectorChange(sector);
        }

        public abstract void OnMoveMouse(Point point);
        public abstract bool OnLeftClick(Point point);
        public abstract void OnLeftRelease(Point point);

        public abstract void Pack(PackedGame game);
        public abstract void Unpack(PackedGame game);
    }
}
