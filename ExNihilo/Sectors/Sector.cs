using ExNihilo.Input.Commands;
using ExNihilo.Menus;
using ExNihilo.Systems;
using ExNihilo.Systems.Backend;
using ExNihilo.Systems.Bases;
using ExNihilo.UI.Bases;
using ExNihilo.Util;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace ExNihilo.Sectors
{
    public abstract class Sector : IUI, IClickable, ISavable, IPlayer
    {
        protected GameContainer Container;

        protected Coordinate CurrentPush;
        protected int CurrentPushMult;

        protected Sector(GameContainer container)
        {
            Container = container;
            CurrentPush = new Coordinate();
            CurrentPushMult = 1;
        }

        public abstract void OnResize(GraphicsDevice graphicsDevice, Coordinate gameWindow);

        public virtual void Enter(Point point, Coordinate gameWindow)
        {
        }
        public virtual void Leave(GameContainer.SectorID newSector)
        {

        }

        public abstract void Initialize();
        public abstract void LoadContent(GraphicsDevice graphicsDevice, ContentManager content);
        public abstract void Update();
        protected abstract void DrawDebugInfo(SpriteBatch spriteBatch);
        public abstract void Draw(SpriteBatch spriteBatch, bool drawDebugInfo);
        public void Draw(SpriteBatch spriteBatch)
        {
            Draw(spriteBatch, false);
        }

        public virtual void OnExit()
        {
        }

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

        public void PushX(int x)
        {
            CurrentPush.X += x;
        }

        public void PushY(int y)
        {
            CurrentPush.Y += y;
        }

        public void PushMult(int mult)
        {
            CurrentPushMult = mult;
        }

        public virtual void Touch()
        {
        }

        public virtual void ToggleTabMenu()
        {

        }
    }
}
