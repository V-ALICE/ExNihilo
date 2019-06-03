using System.Collections.Generic;
using ExNihilo.UI;
using ExNihilo.Util;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace ExNihilo.Menus
{
    public class TitleMenu : Menu
    {
        private readonly List<InteractiveUIElement> _test;
        public TitleMenu()
        {
            _test = new List<InteractiveUIElement>
            {
                new InteractiveUIElement("UI/BigButton", new Vector2(0.5f, 0.25f), "UI/SmallButtonPressed"),
                new InteractiveUIElement("UI/BigButton", new Vector2(0.5f, 0.5f), "UI/SmallButtonPressed"),
                new InteractiveUIElement("UI/BigButton", new Vector2(0.5f, 0.75f), "UI/SmallButtonPressed")
            };
        }

        public override void Enter()
        {
            
        }

        public override bool TryBackOut()
        {
            return true;
        }

        protected override void MenuDown()
        {
            
        }

        protected override void MenuUp()
        {
            
        }

        protected override void MenuLeft()
        {
            
        }

        protected override void MenuRight()
        {
            
        }

        protected override void Select()
        {
            
        }

        public override void Update()
        {

        }

        public override void LoadContent(GraphicsDevice graphics, ContentManager content)
        {
            foreach (var button in _test) button.LoadContent(graphics, content);
        }

        public override void OnResize(GraphicsDevice graphics, Coordinate window)
        {
            foreach (var button in _test) button.OnResize(graphics, window);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            foreach (var button in _test) button.Draw(spriteBatch);
        }

        public override void OnMoveMouse(Point point)
        {
            foreach (var button in _test) button.OnMoveMouse(point);
        }

        public override void OnLeftClick(Point point)
        {
            foreach (var button in _test) button.OnLeftClick(point);
        }

        public override void OnLeftRelease()
        {
            foreach (var button in _test) button.OnLeftRelease();
        }
    }
}
