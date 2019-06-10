using ExNihilo.Systems.Bases;
using ExNihilo.UI.Bases;
using ExNihilo.Util;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace ExNihilo.Menus
{
    public abstract class Menu : IUI, IClickable, ITypable
    {
        public enum MenuCommand
        {
            Up, Down, Left, Right, Select
        }

        protected GameContainer Container;

        protected Menu(GameContainer container)
        {
            Container = container;
        }

        public abstract void Enter();

        public virtual bool BackOut()
        {
            return true;
        }

        protected virtual void MenuDown()
        {
        }

        protected virtual void MenuUp()
        {
        }

        protected virtual void MenuLeft()
        {
        }

        protected virtual void MenuRight()
        {
        }

        protected virtual void Select()
        {
        }

        public void ReceiveCommand(MenuCommand command)
        {
            switch (command)
            {
                case MenuCommand.Up:
                    MenuUp();
                    break;
                case MenuCommand.Down:
                    MenuDown();
                    break;
                case MenuCommand.Left:
                    MenuLeft();
                    break;
                case MenuCommand.Right:
                    MenuRight();
                    break;
                case MenuCommand.Select:
                    Select();
                    break;
            }
        }

        public virtual void Update()
        {
        }

        public abstract void LoadContent(GraphicsDevice graphics, ContentManager content);

        public abstract void OnResize(GraphicsDevice graphics, Coordinate gameWindow);

        public abstract void Draw(SpriteBatch spriteBatch);

        public abstract void OnMoveMouse(Point point);

        public abstract bool OnLeftClick(Point point);

        public abstract void OnLeftRelease(Point point);
        public abstract void ReceiveInput(string input);
        public virtual void Backspace(int len)
        {
        }
    }
}
