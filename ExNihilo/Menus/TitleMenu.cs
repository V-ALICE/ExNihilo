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
        private readonly UIPanel _test;
        public TitleMenu()
        {
            _test = new UIPanel(new Vector2(0.5f, 0.5f), Vector2.One);
            _test.AddElement(new UIPanel(new Vector2(0,0), new Vector2(0.49f, 0.49f), false, UIElement.PositionType.TopLeft));
            _test.AddElement(new UIPanel(new Vector2(0,1), new Vector2(0.49f, 0.49f), false, UIElement.PositionType.BottomLeft));
            _test.AddElement(new UIPanel(new Vector2(1,0), new Vector2(0.49f, 0.49f), false, UIElement.PositionType.TopRight));
            _test.AddElement(new UIPanel(new Vector2(1,1), new Vector2(0.49f, 0.49f), false, UIElement.PositionType.BottomRight));
            _test.AddElement(new UIPanel(new Vector2(0.5f,0.5f), new Vector2(0.49f, 0.49f)));

            _test.AddElement(new UIClickable("UI/BigButtonUp", new Vector2(0.5f, 0.5f), "UI/BigButtonDown"));
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
            _test.LoadContent(graphics, content);
        }

        public override void OnResize(GraphicsDevice graphics, Coordinate window, Vector2 origin)
        {
            _test.OnResize(graphics, window, origin);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            _test.Draw(spriteBatch);
        }

        public override void OnMoveMouse(Point point)
        {
            _test.OnMoveMouse(point);
        }

        public override void OnLeftClick(Point point)
        {
            _test.OnLeftClick(point);
        }

        public override void OnLeftRelease()
        {
            _test.OnLeftRelease();
        }
    }
}
