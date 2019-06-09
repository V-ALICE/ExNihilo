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
            _titleUI = new UIPanel("this.MenuKing", new Vector2(0.5f, 0.5f), Vector2.One, TextureUtilities.PositionType.Center);
            _optionsUI = new UIPanel("this.MenuKing", new Vector2(0.5f, 0.5f), Vector2.One, TextureUtilities.PositionType.Center);
            _loadUI = new UIPanel("this.MenuKing", new Vector2(0.5f, 0.5f), Vector2.One, TextureUtilities.PositionType.Center);

            // Title Menu setup
            var titlePanel = new UIPanel("TitleButtonPanel", new Vector2(0.5f, 1), new Vector2(0, 0.5f), TextureUtilities.PositionType.CenterBottom);
            var playButton = new UIClickable("PlayButton", "UI/BigButton", new Vector2(0, 0), ColorScale.White, titlePanel, TextureUtilities.PositionType.CenterTop, "UI/BigButtonDown", "UI/BigButtonOver");
            var optionsButton = new UIClickable("OptionsButton", "UI/BigButton", new Coordinate(0, 10), ColorScale.White, playButton, TextureUtilities.PositionType.CenterTop,
                TextureUtilities.PositionType.CenterBottom, "UI/BigButtonDown", "UI/BigButtonOver");
            var exitButton = new UIClickable("ExitButton", "UI/BigButton", new Coordinate(0, 10), ColorScale.White, optionsButton, TextureUtilities.PositionType.CenterTop,
                TextureUtilities.PositionType.CenterBottom, "UI/BigButtonDown", "UI/BigButtonOver");
            var playButtonText = new UIText("TitleButtonText", new Coordinate(), "Play Game", ColorScale.Black, playButton,
                TextureUtilities.PositionType.Center, TextureUtilities.PositionType.Center);
            var optionsButtonText = new UIText("OptionsButtonText", new Coordinate(), "Options", ColorScale.Black, optionsButton,
                TextureUtilities.PositionType.Center, TextureUtilities.PositionType.Center);
            var exitButtonText = new UIText("ExitButtonText", new Coordinate(), "Exit", ColorScale.Black, exitButton,
                TextureUtilities.PositionType.Center, TextureUtilities.PositionType.Center);
            var titleDisplay = new UIElement("Title", "UI/Title", new Vector2(0.5f, 0.25f), ColorScale.GetFromGlobal("Random"), _titleUI, TextureUtilities.PositionType.Center);

            playButton.RegisterCallback(SwapToLoad);
            optionsButton.RegisterCallback(SwapToOptions);
            exitButton.RegisterCallback(ExitGame);
            titleDisplay.SetRules(UILibrary.HalfScaleRuleSet);

            titlePanel.AddElements(playButton, optionsButton, exitButton, playButtonText, optionsButtonText, exitButtonText);
            _titleUI.AddElements(titleDisplay, titlePanel);

            // Option Menu setup
            var backButton = new UIClickable("BackButton", "UI/SmallButton", new Coordinate(10, -10), ColorScale.White, _optionsUI, TextureUtilities.PositionType.BottomLeft,
                TextureUtilities.PositionType.BottomLeft, "UI/SmallButtonDown", "UI/SmallButtonOver");
            var backButtonText = new UIText("BackButtonText", new Coordinate(), "Back", ColorScale.Black, backButton,
                TextureUtilities.PositionType.Center, TextureUtilities.PositionType.Center);
            var effectVolumeBar = new UIElement("EffectVolumeBar", "UI/SmallFillBar", new Vector2(0.45f, 0.2f), ColorScale.White, _optionsUI, TextureUtilities.PositionType.TopRight);
            var musicVolumeBar = new UIElement("MusicVolumeBar", "UI/SmallFillBar", new Vector2(0.55f, 0.2f), ColorScale.White, _optionsUI, TextureUtilities.PositionType.TopLeft);
            var effectVolumeBarFill = new UIExtendable("EffectVolumeBarFill", "UI/BarFillRed", new Coordinate(18, 6), ColorScale.White, effectVolumeBar,
                TextureUtilities.PositionType.TopLeft, TextureUtilities.PositionType.TopLeft, new Coordinate(240, 28), true, false);
            var musicVolumeBarFill = new UIExtendable("MusicVolumeBarFill", "UI/BarFillBlue", new Coordinate(18, 6), ColorScale.White, musicVolumeBar,
                TextureUtilities.PositionType.TopLeft, TextureUtilities.PositionType.TopLeft, new Coordinate(240, 28), true, false);
            var effectVolumeBarText = new UIText("EffectVolumeBarText", new Coordinate(2, -4), "Effect Volume", ColorScale.White,
                effectVolumeBarFill, TextureUtilities.PositionType.BottomLeft, TextureUtilities.PositionType.TopLeft);
            var musicVolumeBarText = new UIText("MusicVolumeBarText", new Coordinate(2, -4), "Music Volume", ColorScale.White,
                musicVolumeBarFill, TextureUtilities.PositionType.BottomLeft, TextureUtilities.PositionType.TopLeft);

            var radioSet = new UIPanel("RadioSet", new Vector2(0.45f, 0.55f), new Coordinate(50, 50), _optionsUI, TextureUtilities.PositionType.TopRight);
            var radioButton1 = new UITogglable("RadioButton1", "UI/GreenBulb", new Coordinate(), ColorScale.White, radioSet, TextureUtilities.PositionType.Center, TextureUtilities.PositionType.TopLeft, "UI/GreenBulbDown", "UI/GreenBulbOver");
            var radioButton2 = new UITogglable("RadioButton2", "UI/BlueBulb", new Coordinate(), ColorScale.White, radioSet, TextureUtilities.PositionType.Center, TextureUtilities.PositionType.TopRight, "UI/BlueBulbDown", "UI/BlueBulbOver");
            var radioButton3 = new UITogglable("RadioButton3", "UI/RedBulb", new Coordinate(), ColorScale.White, radioSet, TextureUtilities.PositionType.Center, TextureUtilities.PositionType.BottomLeft, "UI/RedBulbDown", "UI/RedBulbOver");
            var radioButton4 = new UITogglable("RadioButton4", "UI/BlackBulb", new Coordinate(), ColorScale.White, radioSet, TextureUtilities.PositionType.Center, TextureUtilities.PositionType.BottomRight, "UI/BlackBulbDown", "UI/BlackBulbOver");
            var radioButton5 = new UITogglable("RadioButton5", "UI/RadioUnselected", new Coordinate(), ColorScale.White, radioSet, TextureUtilities.PositionType.Center, TextureUtilities.PositionType.Center, "UI/RadioSelected");

            var moveTest = new UIMovable("MoveTest", "UI/SmallEntryBox", new Vector2(0.75f, 0.75f), ColorScale.White, _optionsUI, TextureUtilities.PositionType.Center, "", "", true);

            backButton.RegisterCallback(SwapToTitle);
            effectVolumeBarFill.RegisterCallback(ApplyEffectVolume);
            musicVolumeBarFill.RegisterCallback(ApplyMusicVolume);
            effectVolumeBarFill.ForceValue(new Vector2(0.5f, 1));
            musicVolumeBarFill.ForceValue(new Vector2(0.5f, 1));

            radioSet.AddElements(radioButton1, radioButton2, radioButton3, radioButton4, radioButton5);
            _optionsUI.AddElements(backButton, backButtonText, effectVolumeBar, musicVolumeBar, effectVolumeBarFill, musicVolumeBarFill, effectVolumeBarText, musicVolumeBarText, radioSet, moveTest);

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
