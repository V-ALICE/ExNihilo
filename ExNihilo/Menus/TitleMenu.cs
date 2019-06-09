using ExNihilo.Systems;
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
        private void SwapToTitleFromLoad(UICallbackPackage package)
        {
            if (_deleteMode)
            {
                ((UITogglable)_loadUI.GetElement("DeleteFileButton"))?.ForcePush();
            }
            _type = CurrentMenu.Title;
        }

        private void SwapToTitleFromOptions(UICallbackPackage package)
        {
            SaveHandler.SaveParameters();
            _type = CurrentMenu.Title;
        }

        private void ApplyMusicVolume(UICallbackPackage package)
        {
            AudioManager.MusicVolume = package.value[0];
            SaveHandler.Parameters.MusicVolume = package.value[0];
        }

        private void ApplyEffectVolume(UICallbackPackage package)
        {
            AudioManager.EffectVolume = package.value[0];
            SaveHandler.Parameters.EffectVolume = package.value[0];
        }

/********************************************************************
------->Loading Menu Callbacks
********************************************************************/
        private void ToggleLoadButtons()
        {
            if (_deleteMode)
            {
                if (!SaveHandler.HasSave(SaveHandler.FILE_1)) (_loadUI.GetElement(SaveHandler.FILE_1) as UIClickable)?.Disable(ColorScale.Grey);
                if (!SaveHandler.HasSave(SaveHandler.FILE_2)) (_loadUI.GetElement(SaveHandler.FILE_2) as UIClickable)?.Disable(ColorScale.Grey);
                if (!SaveHandler.HasSave(SaveHandler.FILE_3)) (_loadUI.GetElement(SaveHandler.FILE_3) as UIClickable)?.Disable(ColorScale.Grey);
            }
            else
            {
                if (!SaveHandler.HasSave(SaveHandler.FILE_1)) (_loadUI.GetElement(SaveHandler.FILE_1) as UIClickable)?.Enable();
                if (!SaveHandler.HasSave(SaveHandler.FILE_2)) (_loadUI.GetElement(SaveHandler.FILE_2) as UIClickable)?.Enable();
                if (!SaveHandler.HasSave(SaveHandler.FILE_3)) (_loadUI.GetElement(SaveHandler.FILE_3) as UIClickable)?.Enable();
            }
        }

        private void SelectLoad(UICallbackPackage package)
        {
            if (_deleteMode)
            {
                SaveHandler.DeleteSave(package.caller);
                UpdateLoadButtonText();
                ToggleLoadButtons();
            }
            else
            {
                if (SaveHandler.HasSave(package.caller))
                {
                    Container.Unpack(SaveHandler.GetSave(package.caller));
                    //Container.RequestSectorChange(GameContainer.SectorID.Loading);
                }
                else
                {
                    _type = CurrentMenu.NewGame;
                    slot = package.caller;
                }
            }
        }

        private void ToggleDeleteMode(UICallbackPackage package)
        {
            _deleteMode = package.value[0] > 0;
            ToggleLoadButtons();
        }

/********************************************************************
------->New Game Menu Callbacks
********************************************************************/
        private void CancelNewGame(UICallbackPackage package)
        {
            _type = CurrentMenu.Play;
        }

        private void PrepForTextEntry(UICallbackPackage package)
        {
            _textEntryMode = package.value[0] > 0;
            if (_textEntryMode && TypingKeyboard.Lock(this))
            {
                
            }
            else
            {
                _textEntryMode = false;
                TypingKeyboard.Unlock(this);
            }
        }

        private void UpdateLoadButtonText()
        {
            var file1Text = SaveHandler.HasSave(SaveHandler.FILE_1) ? SaveHandler.GetSave(SaveHandler.FILE_1).TitleCard : "No File";
            var file2Text = SaveHandler.HasSave(SaveHandler.FILE_2) ? SaveHandler.GetSave(SaveHandler.FILE_2).TitleCard : "No File";
            var file3Text = SaveHandler.HasSave(SaveHandler.FILE_3) ? SaveHandler.GetSave(SaveHandler.FILE_3).TitleCard : "No File";
            (_loadUI.GetElement(SaveHandler.FILE_1 + "Text") as UIText)?.SetText(file1Text);
            (_loadUI.GetElement(SaveHandler.FILE_2 + "Text") as UIText)?.SetText(file2Text);
            (_loadUI.GetElement(SaveHandler.FILE_3 + "Text") as UIText)?.SetText(file3Text);
        }

        private void CreateNewGame(UICallbackPackage package)
        {
            if (!(_newGameUI.GetElement("NewGameInputBoxText") is UIText text) || text.Text.Length == 0) return;
            var newGame = new PackedGame(text.Text);
            SaveHandler.Save(slot, newGame);
            UpdateLoadButtonText();
            _type = CurrentMenu.Play;
            slot = "";
            (_newGameUI.GetElement("NewGameInputBoxText") as UIText)?.SetText(""); //temp until creating a new game closes the menu
        }

        /********************************************************************
        ------->Menu Functions
        ********************************************************************/
        private enum CurrentMenu
        {
            Title, Options, Play, NewGame
        }

        private CurrentMenu _type;
        private string slot;
        private bool _deleteMode, _textEntryMode;
        private readonly UIPanel _titleUI, _optionsUI, _loadUI, _newGameUI;
        private const int MAX_NEWGAME_TEXT_SIZE = 15;

        private void DoThing(string caller, float value)
        {
            Container.Console.ForceMessage("<Console>", caller + " pressed with value " + value);
        }

        private UIPanel ActivePanel()
        {
            switch (_type)
            {
                case CurrentMenu.Title:
                    return _titleUI;
                case CurrentMenu.Options:
                    return _optionsUI;
                case CurrentMenu.Play:
                    return _loadUI;
                case CurrentMenu.NewGame:
                    return _newGameUI;
                default: return null;
            }
        }

        public TitleMenu(GameContainer container) : base(container)
        {
            _titleUI = new UIPanel("this.MenuKing", new Vector2(0.5f, 0.5f), Vector2.One, TextureUtilities.PositionType.Center);
            _optionsUI = new UIPanel("this.MenuKing", new Vector2(0.5f, 0.5f), Vector2.One, TextureUtilities.PositionType.Center);
            _loadUI = new UIPanel("this.MenuKing", new Vector2(0.5f, 0.5f), Vector2.One, TextureUtilities.PositionType.Center);
            _newGameUI = new UIPanel("this.MenuKing", new Vector2(0.5f, 0.5f), Vector2.One, TextureUtilities.PositionType.Center);

            // Title Menu setup
            var titlePanel = new UIPanel("TitleButtonPanel", new Vector2(0.5f, 1), new Vector2(0, 0.5f), TextureUtilities.PositionType.CenterBottom);
            var playButton = new UIClickable("PlayButton", "UI/BigButton", new Vector2(0, 0), ColorScale.White, titlePanel, TextureUtilities.PositionType.CenterTop, "UI/BigButtonDown", "UI/BigButtonOver");
            var optionsButton = new UIClickable("OptionsButton", "UI/BigButton", new Coordinate(0, 10), ColorScale.White, playButton, TextureUtilities.PositionType.CenterTop, TextureUtilities.PositionType.CenterBottom, "UI/BigButtonDown", "UI/BigButtonOver");
            var exitButton = new UIClickable("ExitButton", "UI/BigButton", new Coordinate(0, 10), ColorScale.White, optionsButton, TextureUtilities.PositionType.CenterTop, TextureUtilities.PositionType.CenterBottom, "UI/BigButtonDown", "UI/BigButtonOver");
            var playButtonText = new UIText("TitleButtonText", new Coordinate(), "Play Game", ColorScale.Black, playButton, TextureUtilities.PositionType.Center, TextureUtilities.PositionType.Center);
            var optionsButtonText = new UIText("OptionsButtonText", new Coordinate(), "Options", ColorScale.Black, optionsButton, TextureUtilities.PositionType.Center, TextureUtilities.PositionType.Center);
            var exitButtonText = new UIText("ExitButtonText", new Coordinate(), "Exit", ColorScale.Black, exitButton, TextureUtilities.PositionType.Center, TextureUtilities.PositionType.Center);
            var titleDisplay = new UIElement("Title", "UI/Title", new Vector2(0.5f, 0.25f), ColorScale.GetFromGlobal("Random"), _titleUI, TextureUtilities.PositionType.Center);

            playButton.RegisterCallback(SwapToLoad);
            optionsButton.RegisterCallback(SwapToOptions);
            exitButton.RegisterCallback(ExitGame);
            titleDisplay.SetRules(UILibrary.HalfScaleRuleSet);

            titlePanel.AddElements(playButton, optionsButton, exitButton, playButtonText, optionsButtonText, exitButtonText);
            _titleUI.AddElements(titleDisplay, titlePanel);

            // Option Menu setup
            var backButton = new UIClickable("BackButton", "UI/SmallButton", new Coordinate(10, -10), ColorScale.White, _optionsUI, TextureUtilities.PositionType.BottomLeft, TextureUtilities.PositionType.BottomLeft, "UI/SmallButtonDown", "UI/SmallButtonOver");
            var backButtonText = new UIText("BackButtonText", new Coordinate(), "Back", ColorScale.Black, backButton, TextureUtilities.PositionType.Center, TextureUtilities.PositionType.Center);
            var effectVolumeBar = new UIElement("EffectVolumeBar", "UI/SmallFillBar", new Vector2(0.45f, 0.2f), ColorScale.White, _optionsUI, TextureUtilities.PositionType.TopRight);
            var musicVolumeBar = new UIElement("MusicVolumeBar", "UI/SmallFillBar", new Vector2(0.55f, 0.2f), ColorScale.White, _optionsUI, TextureUtilities.PositionType.TopLeft);
            var effectVolumeBarFill = new UIExtendable("EffectVolumeBarFill", "UI/BarFillRed", new Coordinate(18, 6), ColorScale.White, effectVolumeBar, TextureUtilities.PositionType.TopLeft, TextureUtilities.PositionType.TopLeft, new Coordinate(240, 28), true, false);
            var musicVolumeBarFill = new UIExtendable("MusicVolumeBarFill", "UI/BarFillBlue", new Coordinate(18, 6), ColorScale.White, musicVolumeBar, TextureUtilities.PositionType.TopLeft, TextureUtilities.PositionType.TopLeft, new Coordinate(240, 28), true, false);
            var effectVolumeBarText = new UIText("EffectVolumeBarText", new Coordinate(2, -4), "Effect Volume", ColorScale.White, effectVolumeBarFill, TextureUtilities.PositionType.BottomLeft, TextureUtilities.PositionType.TopLeft);
            var musicVolumeBarText = new UIText("MusicVolumeBarText", new Coordinate(2, -4), "Music Volume", ColorScale.White, musicVolumeBarFill, TextureUtilities.PositionType.BottomLeft, TextureUtilities.PositionType.TopLeft);

            //var radioSet = new UIPanel("RadioSet", new Vector2(0.45f, 0.55f), new Coordinate(50, 50), _optionsUI, TextureUtilities.PositionType.TopRight);
            //var radioButton1 = new UITogglable("RadioButton1", "UI/GreenBulb", new Coordinate(), ColorScale.White, radioSet, TextureUtilities.PositionType.Center, TextureUtilities.PositionType.TopLeft, "UI/GreenBulbDown", "UI/GreenBulbOver");
            //var radioButton2 = new UITogglable("RadioButton2", "UI/BlueBulb", new Coordinate(), ColorScale.White, radioSet, TextureUtilities.PositionType.Center, TextureUtilities.PositionType.TopRight, "UI/BlueBulbDown", "UI/BlueBulbOver");
            //var radioButton3 = new UITogglable("RadioButton3", "UI/RedBulb", new Coordinate(), ColorScale.White, radioSet, TextureUtilities.PositionType.Center, TextureUtilities.PositionType.BottomLeft, "UI/RedBulbDown", "UI/RedBulbOver");
            //var radioButton4 = new UITogglable("RadioButton4", "UI/BlackBulb", new Coordinate(), ColorScale.White, radioSet, TextureUtilities.PositionType.Center, TextureUtilities.PositionType.BottomRight, "UI/BlackBulbDown", "UI/BlackBulbOver");
            //var radioButton5 = new UITogglable("RadioButton5", "UI/RadioUnselected", new Coordinate(), ColorScale.White, radioSet, TextureUtilities.PositionType.Center, TextureUtilities.PositionType.Center, "UI/RadioSelected");
            //var moveTest = new UIMovable("MoveTest", "UI/SmallEntryBox", new Vector2(0.75f, 0.75f), ColorScale.White, _optionsUI, TextureUtilities.PositionType.Center, "", "", true);
            //radioSet.AddElements(radioButton1, radioButton2, radioButton3, radioButton4, radioButton5);
            //_optionsUI.AddElements(radioSet, moveTest);

            backButton.RegisterCallback(SwapToTitleFromOptions);
            effectVolumeBarFill.RegisterCallback(ApplyEffectVolume);
            musicVolumeBarFill.RegisterCallback(ApplyMusicVolume);
            effectVolumeBarFill.ForceValue(new Vector2(AudioManager.EffectVolume, 1));
            musicVolumeBarFill.ForceValue(new Vector2(AudioManager.MusicVolume, 1));

            _optionsUI.AddElements(backButton, backButtonText, effectVolumeBar, musicVolumeBar, effectVolumeBarFill, musicVolumeBarFill, effectVolumeBarText, musicVolumeBarText);

            // Load Menu setup
            var loadPanel = new UIPanel("LoadFilePanel", new Vector2(0.5f, 1), new Vector2(0, 0.5f), TextureUtilities.PositionType.CenterBottom);
            var loadFile1 = new UIClickable(SaveHandler.FILE_1, "UI/BigButton", new Vector2(0, 0), ColorScale.White, loadPanel, TextureUtilities.PositionType.CenterTop, "UI/BigButtonDown", "UI/BigButtonOver");
            var loadFile2 = new UIClickable(SaveHandler.FILE_2, "UI/BigButton", new Coordinate(0, 10), ColorScale.White, loadFile1, TextureUtilities.PositionType.CenterTop, TextureUtilities.PositionType.CenterBottom, "UI/BigButtonDown", "UI/BigButtonOver");
            var loadFile3 = new UIClickable(SaveHandler.FILE_3, "UI/BigButton", new Coordinate(0, 10), ColorScale.White, loadFile2, TextureUtilities.PositionType.CenterTop, TextureUtilities.PositionType.CenterBottom, "UI/BigButtonDown", "UI/BigButtonOver");
            var loadFile1Text = new UIText(SaveHandler.FILE_1 + "Text", new Coordinate(), "", ColorScale.Black, loadFile1, TextureUtilities.PositionType.Center, TextureUtilities.PositionType.Center);
            var loadFile2Text = new UIText(SaveHandler.FILE_2 + "Text", new Coordinate(), "", ColorScale.Black, loadFile2, TextureUtilities.PositionType.Center, TextureUtilities.PositionType.Center);
            var loadFile3Text = new UIText(SaveHandler.FILE_3 + "Text", new Coordinate(), "", ColorScale.Black, loadFile3, TextureUtilities.PositionType.Center, TextureUtilities.PositionType.Center);
            var deleteFileButton = new UITogglable("DeleteFileButton", "UI/SmallButton", new Coordinate(-10, -10), ColorScale.White, _loadUI, TextureUtilities.PositionType.BottomRight, TextureUtilities.PositionType.BottomRight, "UI/SmallButtonDown", "UI/SmallButtonOver");
            var deleteFileButtonText = new UIText("DeleteFileButtonText", new Coordinate(), "Delete File", ColorScale.Black, deleteFileButton, TextureUtilities.PositionType.Center, TextureUtilities.PositionType.Center);
            var backButton2 = new UIClickable("BackButton", "UI/SmallButton", new Coordinate(10, -10), ColorScale.White, _loadUI, TextureUtilities.PositionType.BottomLeft, TextureUtilities.PositionType.BottomLeft, "UI/SmallButtonDown", "UI/SmallButtonOver");
            var backButtonText2 = new UIText("BackButtonText", new Coordinate(), "Back", ColorScale.Black, backButton2, TextureUtilities.PositionType.Center, TextureUtilities.PositionType.Center);

            loadFile1.RegisterCallback(SelectLoad);
            loadFile2.RegisterCallback(SelectLoad);
            loadFile3.RegisterCallback(SelectLoad);
            deleteFileButton.RegisterCallback(ToggleDeleteMode);
            backButton2.RegisterCallback(SwapToTitleFromLoad);

            loadPanel.AddElements(loadFile1, loadFile2, loadFile3, loadFile1Text, loadFile2Text, loadFile3Text);
            _loadUI.AddElements(backButton2, backButtonText2, titleDisplay, deleteFileButton, deleteFileButtonText, loadPanel);

            //New Game Menu setup
            var cancelButton = new UIClickable("BackButton", "UI/SmallButton", new Coordinate(10, -10), ColorScale.White, _newGameUI, TextureUtilities.PositionType.BottomLeft, TextureUtilities.PositionType.BottomLeft, "UI/SmallButtonDown", "UI/SmallButtonOver");
            var cancelButtonText = new UIText("BackButtonText", new Coordinate(), "Cancel", ColorScale.Black, cancelButton, TextureUtilities.PositionType.Center, TextureUtilities.PositionType.Center);
            var confirmButton = new UIClickable("ConfirmButton", "UI/SmallButton", new Coordinate(-10, -10), ColorScale.White, _newGameUI, TextureUtilities.PositionType.BottomRight, TextureUtilities.PositionType.BottomRight, "UI/SmallButtonDown", "UI/SmallButtonOver");
            var confirmButtonText = new UIText("ConfirmButtonText", new Coordinate(), "Confirm", ColorScale.Black, confirmButton, TextureUtilities.PositionType.Center, TextureUtilities.PositionType.Center);
            var inputBox = new UITogglable("NewGameInputBox", "UI/SmallEntryBox", new Vector2(0.1f, 0.1f), ColorScale.White, _newGameUI, TextureUtilities.PositionType.TopLeft, "UI/SmallEntryBox", "", true);
            var inputBoxText = new UIText("NewGameInputBoxText", new Coordinate(20, 20), "", ColorScale.Black, inputBox, TextureUtilities.PositionType.TopLeft, TextureUtilities.PositionType.TopLeft);

            cancelButton.RegisterCallback(CancelNewGame);
            inputBox.RegisterCallback(PrepForTextEntry);
            confirmButton.RegisterCallback(CreateNewGame);
            inputBoxText.SetRules(UILibrary.DoubleScaleRuleSet);

            _newGameUI.AddElements(cancelButton, cancelButtonText, inputBox, inputBoxText, confirmButton, confirmButtonText);
        }

        public override void Enter()
        {
            _type = CurrentMenu.Title;
            (_newGameUI.GetElement("NewGameInputBoxText") as UIText)?.SetText("");
            UpdateLoadButtonText();
        }

        public override bool BackOut()
        {
            if (_textEntryMode)
            {
                (_newGameUI.GetElement("NewGameInputBox") as UITogglable)?.ForcePush();
                return false;
            }
            switch (_type)
            {
                case CurrentMenu.NewGame:
                    _type = CurrentMenu.Play;
                    return false;
                case CurrentMenu.Play:
                case CurrentMenu.Options:
                    _type = CurrentMenu.Title;
                    return false;
                default:
                    return true;
            }
        }

        public override void LoadContent(GraphicsDevice graphics, ContentManager content)
        {
            _titleUI.LoadContent(graphics, content);
            _optionsUI.LoadContent(graphics, content);
            _loadUI.LoadContent(graphics, content);
            _newGameUI.LoadContent(graphics, content);
        }

        public override void OnResize(GraphicsDevice graphics, Coordinate gameWindow)
        {
            _titleUI.OnResize(graphics, gameWindow);
            _optionsUI.OnResize(graphics, gameWindow);
            _loadUI.OnResize(graphics, gameWindow);
            _newGameUI.OnResize(graphics, gameWindow);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            ActivePanel().Draw(spriteBatch);
        }

        public override void OnMoveMouse(Point point)
        {
            ActivePanel().OnMoveMouse(point);
        }

        public override bool OnLeftClick(Point point)
        {
            return ActivePanel().OnLeftClick(point);
        }

        public override void OnLeftRelease(Point point)
        {
            ActivePanel().OnLeftRelease(point);
        }

        public override void Backspace(int len)
        {
            if (!_textEntryMode) return;

            if (_newGameUI.GetElement("NewGameInputBoxText") is UIText text)
            {
                if (text.Text.Length >= len) text.SetText(text.Text.Substring(0, text.Text.Length - len));
            }
        }

        public override void ReceiveInput(string input)
        {
            if (!_textEntryMode) return;

            if (_newGameUI.GetElement("NewGameInputBoxText") is UIText text)
            {
                text.SetText(Utilities.Clamp(text.Text + input, MAX_NEWGAME_TEXT_SIZE));
            }
        }
    }
}
