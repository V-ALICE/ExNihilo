using System;
using ExNihilo.Systems;
using ExNihilo.Systems.Backend;
using ExNihilo.Systems.Bases;
using ExNihilo.UI;
using ExNihilo.UI.Bases;
using ExNihilo.Util;
using ExNihilo.Util.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace ExNihilo.Menus
{
    public abstract class Menu : IUI, IClickable, ITypable, ISavable
    {
        public enum MenuCommand
        {
            Up, Down, Left, Right, Select
        }

        protected GameContainer Container;

        protected Action OnExit;

        protected Menu(GameContainer container, Action onExit)
        {
            Container = container;
            OnExit = onExit;
        }

        public virtual void Enter(Point point)
        {
        }

        public virtual void BackOut()
        {
            OnExit?.Invoke();
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

        protected void RegisterAll(Action<UICallbackPackage> action, params UIClickable[] elements)
        {
            foreach (var element in elements) element.RegisterCallback(action);
        }
        protected void RegisterAll(Action<UICallbackPackage> action, params UIElement[] elements)
        {
            //Elements must actually be clickables or else this will just fail
            foreach (var element in elements) ((UIClickable)element).RegisterCallback(action);
        }

        protected void DisableAll(ColorScale color, params UIClickable[] elements)
        {
            foreach (var element in elements) element.Disable(color);
        }

        protected void SetExtrasAll(string downPath, string overPath, ColorScale down, ColorScale over, params UIClickable[] elements)
        {
            foreach (var element in elements) element.SetExtraStates(downPath, overPath, down, over);
        }

        protected void SetRulesAll(ScaleRuleSet rules, params UIElement[] elements)
        {
            foreach (var element in elements) element.SetRules(rules);
        }

        public virtual void Update()
        {
        }

        public abstract void LoadContent(GraphicsDevice graphics, ContentManager content);

        public abstract void OnResize(GraphicsDevice graphics, Coordinate gameWindow);

        public abstract void Draw(SpriteBatch spriteBatch);

        public abstract bool OnMoveMouse(Point point);

        public abstract bool OnLeftClick(Point point);

        public abstract void OnLeftRelease(Point point);
        public abstract void ReceiveInput(string input);
        public virtual void Backspace(int len)
        {
        }

        public virtual void Pack(PackedGame game)
        {
        }

        public virtual void Unpack(PackedGame game)
        {
        }
    }
}
