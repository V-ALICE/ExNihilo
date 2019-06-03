using ExNihilo.UI;
using ExNihilo.Util;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace ExNihilo.Menus
{
    public abstract class Menu : IUI, IClickable
    {
        public enum MenuCommand
        {
            Up, Down, Left, Right, Select
        }

        protected bool SubMode;
        protected int Selection, SubSelection;

        public abstract void Enter();
        public abstract bool TryBackOut();

        protected abstract void MenuDown();
        protected abstract void MenuUp();
        protected abstract void MenuLeft();
        protected abstract void MenuRight();
        protected abstract void Select();
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

        public abstract void Update();

        public abstract void LoadContent(GraphicsDevice graphics, ContentManager content);

        public abstract void OnResize(GraphicsDevice graphics, Coordinate window);

        public abstract void Draw(SpriteBatch spriteBatch);

        public abstract void OnMoveMouse(Point point);
        public abstract void OnLeftClick(Point point);
        public abstract void OnLeftRelease();
    }
}
