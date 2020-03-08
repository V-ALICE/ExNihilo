using System;
using ExNihilo.Systems.Backend;
using ExNihilo.UI;
using ExNihilo.Util;
using ExNihilo.Util.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace ExNihilo.Menus
{
    public class MultiplayerMenu : Menu
    {
/********************************************************************
------->Menu Callbacks
********************************************************************/
        private void CloseMenu(UICallbackPackage package)
        {
            OnExit?.Invoke();
        }

        private void NoteAction(bool accepted)
        {
            _showingNote = false;
            if (accepted) _panelUI.OnMoveMouse(_lastMousePosition);
            else OnExit?.Invoke();
        }

/********************************************************************
------->Menu Functions
********************************************************************/
        private readonly UIPanel _panelUI;
        private readonly NoteMenu _note;
        private Point _lastMousePosition;
        private bool _showingNote;

        public MultiplayerMenu(GameContainer container, Action onExit) : base(container, onExit)
        {
            _lastMousePosition = new Point();

            _panelUI = new UIPanel("this.MenuKing", new Vector2(0.5f, 0.5f), Vector2.One, TextureUtilities.PositionType.Center);

            var backdrop = new UIElement("Backdrop", "UI/decor/Backdrop", new Vector2(0.5f, 0.5f), Color.White, _panelUI, TextureUtilities.PositionType.Center);
            var exitButton = new UIClickable("ExitButton", "UI/button/RedBulb", new Coordinate(-8, 8), ColorScale.White, backdrop, TextureUtilities.PositionType.Center, TextureUtilities.PositionType.TopRight);
            var exitButtonX = new UIElement("ExitButtonX", "UI/icon/No", new Coordinate(), ColorScale.White, exitButton, TextureUtilities.PositionType.Center, TextureUtilities.PositionType.Center);
            exitButton.RegisterCallback(CloseMenu);
            SetRulesAll(TextureLibrary.MediumScaleRuleSet, exitButton, exitButtonX);
            exitButton.SetExtraStates("UI/button/RedBulbDown", "UI/button/RedBulbOver");

            

            _panelUI.AddElements(backdrop, exitButton, exitButtonX);

            _note = new NoteMenu(container, "There's a familiar island off in\nthe distance. Call out?", NoteAction);
        }

        public override void Enter(Point point)
        {
            _lastMousePosition = point;
            _showingNote = true;
            _note.Enter(point);
        }

        public override void LoadContent(GraphicsDevice graphics, ContentManager content)
        {
            _panelUI.LoadContent(graphics, content);
            _note.LoadContent(graphics, content);
        }

        public override void OnResize(GraphicsDevice graphics, Coordinate gameWindow)
        {
            _panelUI.OnResize(graphics, gameWindow);
            _note.OnResize(graphics, gameWindow);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (_showingNote) _note.Draw(spriteBatch);
            else _panelUI.Draw(spriteBatch);
        }

        public override bool OnMoveMouse(Point point)
        {
            if (_showingNote) _note.OnMoveMouse(point);
            else _panelUI.OnMoveMouse(point);
            return false;
        }

        public override bool OnLeftClick(Point point)
        {
            if (_showingNote) return _note.OnLeftClick(point);
            return _panelUI.OnLeftClick(point);
        }

        public override void OnLeftRelease(Point point)
        {
            if (_showingNote) _note.OnLeftRelease(point);
            else _panelUI.OnLeftRelease(point);
        }

        public override void ReceiveInput(string input)
        {

        }
    }
}
