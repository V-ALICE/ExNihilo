using ExNihilo.UI;
using ExNihilo.Util;
using ExNihilo.Util.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace ExNihilo.Menus
{
    public class DivineMenu : Menu
    {
/********************************************************************
------->Menu Callbacks
********************************************************************/
        private void CloseMenu(UICallbackPackage package)
        {
            Dead = true;
        }

/********************************************************************
------->Menu Functions
********************************************************************/
        private readonly UIPanel _panelUI;

        public DivineMenu(GameContainer container) : base(container)
        {
            _panelUI = new UIPanel("this.MenuKing", new Vector2(0.5f, 0.5f), Vector2.One, TextureUtilities.PositionType.Center);

            var backdrop = new UIElement("Backdrop", "UI/decor/Backdrop", new Vector2(0.5f, 0.5f), Color.White, _panelUI, TextureUtilities.PositionType.Center);
            var backButton = new UIClickable("BackButton", "UI/button/SmallButton", new Coordinate(14, -14), ColorScale.White, backdrop, TextureUtilities.PositionType.BottomLeft, TextureUtilities.PositionType.BottomLeft);
            var backButtonText = new UIText("BackButtonText", new Coordinate(), "Back", ColorScale.Black, backButton, TextureUtilities.PositionType.Center, TextureUtilities.PositionType.Center);

            backButton.RegisterCallback(CloseMenu);
            backButton.SetExtraStates("UI/button/SmallButtonDown", "UI/button/SmallButtonOver");

            _panelUI.AddElements(backdrop, backButton, backButtonText);
        }

        public override void Enter(Point point)
        {
            Dead = false;
            _panelUI.OnMoveMouse(point);
        }

        public override void LoadContent(GraphicsDevice graphics, ContentManager content)
        {
            _panelUI.LoadContent(graphics, content);
        }

        public override void OnResize(GraphicsDevice graphics, Coordinate gameWindow)
        {
            _panelUI.OnResize(graphics, gameWindow);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            _panelUI.Draw(spriteBatch);
        }

        public override void OnMoveMouse(Point point)
        {
            _panelUI.OnMoveMouse(point);
        }

        public override bool OnLeftClick(Point point)
        {
            return _panelUI.OnLeftClick(point);
        }

        public override void OnLeftRelease(Point point)
        {
            _panelUI.OnLeftRelease(point);
        }

        public override void ReceiveInput(string input)
        {

        }
    }
}
