using ExNihilo.UI;
using ExNihilo.Util;
using ExNihilo.Util.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace ExNihilo.Menus
{
    public class NoteMenu : Menu
    {
/********************************************************************
------->Menu Callbacks
********************************************************************/
        private void CloseMenu(UICallbackPackage package)
        {
            Dead = true;
        }

        private void ConfirmMenu(UICallbackPackage package)
        {
            Confirmed = true;
        }

/********************************************************************
------->Menu Functions
********************************************************************/
        private readonly UIPanel _panelUI;
        public bool Confirmed;

        public NoteMenu(GameContainer container, string note, bool disableConfirm) : base(container)
        {
            _panelUI = new UIPanel("this.MenuKing", new Vector2(0.5f, 0.5f), Vector2.One, TextureUtilities.PositionType.Center);

            var textBox = new UIElement("NoteTextBox", "UI/field/SmallEntryBox", new Vector2(0.5f, 0.5f), ColorScale.White, _panelUI, TextureUtilities.PositionType.Center);
            var text = new UIText("Note", new Coordinate(14, 18), note, ColorScale.Black, textBox, TextureUtilities.PositionType.TopLeft, TextureUtilities.PositionType.TopLeft, true);
            var confirmButton = new UIClickable("ConfirmButton", "UI/button/GreenBulb", new Coordinate(-6, -20), ColorScale.White, textBox, TextureUtilities.PositionType.TopRight, TextureUtilities.PositionType.BottomRight, true);
            var confirmButtonIcon = new UIElement("ConfirmButtonIcon", "UI/icon/Yes", new Coordinate(), ColorScale.Ghost, confirmButton, TextureUtilities.PositionType.Center, TextureUtilities.PositionType.Center);
            var cancelButton = new UIClickable("CancelButton", "UI/button/RedBulb", new Coordinate(-2, 0), ColorScale.White, confirmButton, TextureUtilities.PositionType.TopRight, TextureUtilities.PositionType.TopLeft);
            var cancelButtonIcon = new UIElement("CancelButtonIcon", "UI/icon/No", new Coordinate(), ColorScale.Ghost, cancelButton, TextureUtilities.PositionType.Center, TextureUtilities.PositionType.Center);

            confirmButton.SetExtraStates("UI/button/GreenBulbDown", "UI/button/GreenBulbOver");
            cancelButton.SetExtraStates("UI/button/RedBulbDown", "UI/button/RedBulbOver");
            confirmButton.RegisterCallback(ConfirmMenu);
            cancelButton.RegisterCallback(CloseMenu);
            if (disableConfirm) confirmButton.Disable(ColorScale.Grey);

            _panelUI.AddElements(textBox, text, confirmButton, confirmButtonIcon, cancelButton, cancelButtonIcon);
        }

        public override void Enter(Point point)
        {
            Dead = false;
            Confirmed = false;
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
