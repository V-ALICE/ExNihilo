using ExNihilo.UI;
using ExNihilo.UI.Bases;
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

        protected GameContainer Container;
        protected UIPanel _menuUI;
        protected bool SubMode;
        protected int Selection, SubSelection;

        protected Menu(GameContainer container)
        {
            _menuUI = new UIPanel("this.MenuKing", new Vector2(0.5f, 0.5f), Vector2.One, UIElement.PositionType.Center);
            Container = container;
        }

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

        public virtual void Update()
        {

        }

        public virtual void LoadContent(GraphicsDevice graphics, ContentManager content)
        {
            _menuUI.LoadContent(graphics, content);
        }

        public virtual void OnResize(GraphicsDevice graphics, Coordinate gameWindow)
        {
            _menuUI.OnResize(graphics, gameWindow);
        }

        public virtual void Draw(SpriteBatch spriteBatch)
        {
            _menuUI.Draw(spriteBatch);
        }

        public virtual void OnMoveMouse(Point point)
        {
            _menuUI.OnMoveMouse(point);
        }

        public virtual bool OnLeftClick(Point point)
        {
            return _menuUI.OnLeftClick(point);
        }

        public virtual void OnLeftRelease()
        {
            _menuUI.OnLeftRelease();
        }
    }
}
