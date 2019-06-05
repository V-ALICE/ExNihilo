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
            var horizontal = new UIPanel(new Vector2(0.5f, 0.5f), new Vector2(0.75f, 0f), UIElement.PositionType.CenterTop);
            horizontal.AddElements(new UIMovable("UI/BigButtonUp", new Vector2(0f, 0.5f), "UI/BigButtonDown", true));
            var vertical = new UIPanel(new Vector2(0.5f, 0.5f), new Vector2(0f, 0.75f), UIElement.PositionType.CenterLeft);
            vertical.AddElements(new UIMovable("UI/BigButtonUp", new Vector2(0.5f, 0f), "UI/BigButtonDown", true));

            _test = new UIPanel(new Vector2(0.5f, 0.5f), new Vector2(0.75f, 0.75f));
            _test.AddElements(new UIMovable("UI/BigButtonUp", new Vector2(0.5f, 0.5f), "UI/BigButtonDown"));
            _test.AddElements(horizontal, vertical);
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
