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
        private void SwapToLoad(UICallbackPackage package)
        {
            _type = CurrentMenu.Play;
        }

        private void SwapToOptions(UICallbackPackage package)
        {
            _type = CurrentMenu.Options;
        }

        private void ExitGame(UICallbackPackage package)
        {
            Container.ExitGame();
        }

/********************************************************************
------->Options Menu Callbacks
********************************************************************/
        private void SwapToTitle(UICallbackPackage package)
        {
            _type = CurrentMenu.Title;
        }

        private void ApplyMusicVolume(UICallbackPackage package)
        {
            AudioManager.MusicVolume = package.value[0];
        }

        private void ApplyEffectVolume(UICallbackPackage package)
        {
            AudioManager.EffectVolume = package.value[0];
        }

/********************************************************************
------->Loading Menu Callbacks
********************************************************************/
        private void SelectLoad(UICallbackPackage package)
        {
            DoThing(package.caller, package.value[0]);
        }

/********************************************************************
------->Menu Functions
********************************************************************/

        private enum CurrentMenu
        {
            Title, Options, Play
        }

        private CurrentMenu _type;
        private readonly UIPanel _titleUI, _optionsUI, _loadUI;

        private void DoThing(string caller, float value)
        {
            Container.Console.ForceMessage("", "<" + caller + " pressed with value " + value + ">");
        }

        public TitleMenu(GameContainer container) : base(container)
        {
            _titleUI = new UIPanel("this.MenuKing", new Vector2(0.5f, 0.5f), Vector2.One, UIElement.PositionType.Center);
            _optionsUI = new UIPanel("this.MenuKing", new Vector2(0.5f, 0.5f), Vector2.One, UIElement.PositionType.Center);
            _loadUI = new UIPanel("this.MenuKing", new Vector2(0.5f, 0.5f), Vector2.One, UIElement.PositionType.Center);

            // Title Menu setup
            var titlePanel = new UIPanel("TitleButtonPanel", new Vector2(0.5f, 1), new Vector2(0, 0.5f), UIElement.PositionType.CenterBottom);
            var playButton = new UIClickable("PlayButton", "UI/BigButton", new Vector2(0, 0), titlePanel, UIElement.PositionType.CenterTop, "UI/BigButtonDown");
            var optionsButton = new UIClickable("OptionsButton", "UI/BigButton", new Coordinate(0, 10), playButton, UIElement.PositionType.CenterTop,
                UIElement.PositionType.CenterBottom, "UI/BigButtonDown");
            var exitButton = new UIClickable("ExitButton", "UI/BigButton", new Coordinate(0, 10), optionsButton, UIElement.PositionType.CenterTop,
                UIElement.PositionType.CenterBottom, "UI/BigButtonDown");
            var playButtonText = new UIText("TitleButtonText", new Coordinate(), "Play Game", new ColorScale[] {Color.Black}, playButton,
                UIElement.PositionType.Center, UIElement.PositionType.Center);
            var optionsButtonText = new UIText("OptionsButtonText", new Coordinate(), "Options", new ColorScale[] {Color.Black}, optionsButton,
                UIElement.PositionType.Center, UIElement.PositionType.Center);
            var exitButtonText = new UIText("ExitButtonText", new Coordinate(), "Exit", new ColorScale[] {Color.Black}, exitButton,
                UIElement.PositionType.Center, UIElement.PositionType.Center);
            var titleDisplay = new UIElement("Title", "UI/Title", new Vector2(0.5f, 0.25f), _titleUI, UIElement.PositionType.Center);

            playButton.RegisterCallback(SwapToLoad);
            optionsButton.RegisterCallback(SwapToOptions);
            exitButton.RegisterCallback(ExitGame);
            titleDisplay.SetRules(UILibrary.HalfScaleRuleSet);
            titleDisplay.SetColorScale(ColorScale.GetFromGlobal("Random"));

            titlePanel.AddElements(playButton, optionsButton, exitButton, playButtonText, optionsButtonText, exitButtonText);
            _titleUI.AddElements(titleDisplay, titlePanel);

            // Option Menu setup
            var backButton = new UIClickable("BackButton", "UI/SmallButton", new Coordinate(10, -10), _optionsUI, UIElement.PositionType.BottomLeft,
                UIElement.PositionType.BottomLeft, "UI/SmallButtonDown");
            var backButtonText = new UIText("BackButtonText", new Coordinate(), "Back", new ColorScale[] {Color.Black}, backButton,
                UIElement.PositionType.Center, UIElement.PositionType.Center);
            var effectVolumeBar = new UIElement("EffectVolumeBar", "UI/SmallFillBar", new Vector2(0.45f, 0.2f), _optionsUI, UIElement.PositionType.TopRight);
            var musicVolumeBar = new UIElement("MusicVolumeBar", "UI/SmallFillBar", new Vector2(0.55f, 0.2f), _optionsUI, UIElement.PositionType.TopLeft);
            var effectVolumeBarFill = new UIExtendable("EffectVolumeBarFill", "UI/BarFillRed", new Coordinate(18, 6), effectVolumeBar,
                UIElement.PositionType.TopLeft, UIElement.PositionType.TopLeft, new Coordinate(240, 28), true, false);
            var musicVolumeBarFill = new UIExtendable("MusicVolumeBarFill", "UI/BarFillBlue", new Coordinate(18, 6), musicVolumeBar,
                UIElement.PositionType.TopLeft, UIElement.PositionType.TopLeft, new Coordinate(240, 28), true, false);
            var effectVolumeBarText = new UIText("EffectVolumeBarText", new Coordinate(2, -4), "Effect Volume", new ColorScale[] {Color.White},
                effectVolumeBarFill, UIElement.PositionType.BottomLeft, UIElement.PositionType.TopLeft);
            var musicVolumeBarText = new UIText("MusicVolumeBarText", new Coordinate(2, -4), "Music Volume", new ColorScale[] { Color.White },
                musicVolumeBarFill, UIElement.PositionType.BottomLeft, UIElement.PositionType.TopLeft);

            backButton.RegisterCallback(SwapToTitle);
            effectVolumeBarFill.RegisterCallback(ApplyEffectVolume);
            musicVolumeBarFill.RegisterCallback(ApplyMusicVolume);
            effectVolumeBarFill.ForceValue(new Vector2(0.5f, 1));
            musicVolumeBarFill.ForceValue(new Vector2(0.5f, 1));

            _optionsUI.AddElements(backButton, backButtonText, effectVolumeBar, musicVolumeBar, effectVolumeBarFill, musicVolumeBarFill, effectVolumeBarText, musicVolumeBarText);

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

        public override void OnLeftRelease(Point point)
        {
            switch (_type)
            {
                case CurrentMenu.Title:
                    _titleUI.OnLeftRelease(point);
                    break;
                case CurrentMenu.Options:
                    _optionsUI.OnLeftRelease(point);
                    break;
                case CurrentMenu.Play:
                    _loadUI.OnLeftRelease(point);
                    break;
            }
        }

    }
}
