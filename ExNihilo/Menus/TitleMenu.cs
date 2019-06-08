using System;
using ExNihilo.UI;
using ExNihilo.UI.Bases;
using ExNihilo.Util;
using ExNihilo.Util.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace ExNihilo.Menus
{
    public class TitleMenu : Menu
    {
/********************************************************************
------->Title Menu Callbacks
********************************************************************/
        private void SwapToLoad(string s)
        {
            _type = CurrentMenu.Play;
        }

        private void SwapToOptions(string s)
        {
            _type = CurrentMenu.Options;
        }

        private void ExitGame(string s)
        {
            Container.ExitGame();
        }

/********************************************************************
------->Options Menu Callbacks
********************************************************************/
        private void SwapToTitle(string s)
        {
            _type = CurrentMenu.Title;
        }

/********************************************************************
------->Loading Menu Callbacks
********************************************************************/

/********************************************************************
------->Menu Functions
********************************************************************/

        private enum CurrentMenu
        {
            Title, Options, Play
        }

        private CurrentMenu _type;
        private readonly UIPanel _titleUI, _optionsUI, _loadUI;

        private void DoThing(string s)
        {
            Container.Console.ForceMessage("", "<" + s + " pressed>");
        }

        public TitleMenu(GameContainer container) : base(container)
        {
            _titleUI = new UIPanel("this.MenuKing", new Vector2(0.5f, 0.5f), Vector2.One, UIElement.PositionType.Center);
            _optionsUI = new UIPanel("this.MenuKing", new Vector2(0.5f, 0.5f), Vector2.One, UIElement.PositionType.Center);
            _loadUI = new UIPanel("this.MenuKing", new Vector2(0.5f, 0.5f), Vector2.One, UIElement.PositionType.Center);

            // Title Menu setup
            var titlePanel = new UIPanel("TitleButtonPanel", new Vector2(0.5f, 1), new Vector2(0, 0.5f), UIElement.PositionType.CenterBottom);
            var playButton = new UIClickable("PlayButton", "UI/BigButtonUp", new Vector2(0, 0), titlePanel, UIElement.PositionType.CenterTop, "UI/BigButtonDown");
            var optionsButton = new UIClickable("OptionsButton", "UI/BigButtonUp", new Coordinate(0, 10), playButton, UIElement.PositionType.CenterTop, UIElement.PositionType.CenterBottom, "UI/BigButtonDown");
            var exitButton = new UIClickable("ExitButton", "UI/BigButtonUp", new Coordinate(0, 10), optionsButton, UIElement.PositionType.CenterTop, UIElement.PositionType.CenterBottom, "UI/BigButtonDown");
            var playButtonText = new UIText("TitleButtonText", new Coordinate(), "Play Game", new ColorScale[0], playButton, UIElement.PositionType.Center, UIElement.PositionType.Center);
            var optionsButtonText = new UIText("OptionsButtonText", new Coordinate(), "Options", new ColorScale[0], optionsButton, UIElement.PositionType.Center, UIElement.PositionType.Center);
            var exitButtonText = new UIText("ExitButtonText", new Coordinate(), "Exit", new ColorScale[0], exitButton, UIElement.PositionType.Center, UIElement.PositionType.Center);
            var titleDisplay = new UIElement("Title", "UI/Title", new Vector2(0.5f, 0.25f), _titleUI, UIElement.PositionType.Center);

            playButton.RegisterCallback(SwapToLoad);
            optionsButton.RegisterCallback(SwapToOptions);
            exitButton.RegisterCallback(ExitGame);
            titleDisplay.SetRules(UILibrary.HalfScaleRuleSet);
            titleDisplay.SetColorScale(ColorScale.GetFromGlobal("Random"));

            titlePanel.AddElements(playButton, optionsButton, exitButton, playButtonText, optionsButtonText, exitButtonText);
            _titleUI.AddElements(titleDisplay, titlePanel);

            // Option Menu setup
            var backButton = new UIClickable("BackButton", "UI/BigButtonUp", new Coordinate(10, -10), _optionsUI, UIElement.PositionType.BottomLeft, UIElement.PositionType.BottomLeft, "UI/BigButtonDown");
            var backButtonText = new UIText("BackButtonText", new Coordinate(), "Back", new ColorScale[0], backButton, UIElement.PositionType.Center, UIElement.PositionType.Center);

            backButton.RegisterCallback(SwapToTitle);

            _optionsUI.AddElements(backButton, backButtonText);

            // Option Menu setup

            _loadUI.AddElements(backButton, backButtonText);
        }

        public override void Enter()
        {
            _type = CurrentMenu.Title;
        }

        public override void LoadContent(GraphicsDevice graphics, ContentManager content)
        {
            _titleUI.LoadContent(graphics, content);
            _optionsUI.LoadContent(graphics, content);
            _loadUI.LoadContent(graphics, content);
        }

        public override void OnResize(GraphicsDevice graphics, Coordinate gameWindow)
        {
            _titleUI.OnResize(graphics, gameWindow);
            _optionsUI.OnResize(graphics, gameWindow);
            _loadUI.OnResize(graphics, gameWindow);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            switch (_type)
            {
                case CurrentMenu.Title:
                    _titleUI.Draw(spriteBatch);
                    break;
                case CurrentMenu.Options:
                    _optionsUI.Draw(spriteBatch);
                    break;
                case CurrentMenu.Play:
                    _loadUI.Draw(spriteBatch);
                    break;
            }
        }

        public override void OnMoveMouse(Point point)
        {
            switch (_type)
            {
                case CurrentMenu.Title:
                    _titleUI.OnMoveMouse(point);
                    break;
                case CurrentMenu.Options:
                    _optionsUI.OnMoveMouse(point);
                    break;
                case CurrentMenu.Play:
                    _loadUI.OnMoveMouse(point);
                    break;
            }
        }

        public override bool OnLeftClick(Point point)
        {
            switch (_type)
            {
                case CurrentMenu.Title:
                    return _titleUI.OnLeftClick(point);
                case CurrentMenu.Options:
                    return _optionsUI.OnLeftClick(point);
                case CurrentMenu.Play:
                    return _loadUI.OnLeftClick(point);
            }

            return false;
        }

        public override void OnLeftRelease()
        {
            switch (_type)
            {
                case CurrentMenu.Title:
                    _titleUI.OnLeftRelease();
                    break;
                case CurrentMenu.Options:
                    _optionsUI.OnLeftRelease();
                    break;
                case CurrentMenu.Play:
                    _loadUI.OnLeftRelease();
                    break;
            }
        }

    }
}
