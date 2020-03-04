using System.Diagnostics.Eventing.Reader;
using System.Threading.Tasks;
using ExNihilo.Input.Controllers;
using ExNihilo.Systems;
using ExNihilo.Systems.Backend;
using ExNihilo.Systems.Game;
using ExNihilo.UI;
using ExNihilo.Util;
using ExNihilo.Util.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using PositionType = ExNihilo.Util.Graphics.TextureUtilities.PositionType;

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
            _loadUI.OnMoveMouse(_lastMousePosition);
        }

        private void SwapToOptions(UICallbackPackage package)
        {
            _type = CurrentMenu.Options;
            _optionsUI.OnMoveMouse(_lastMousePosition);
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
            _deleteMode = false;
            _type = CurrentMenu.Title;
            _titleUI.OnMoveMouse(_lastMousePosition);
        }

        private void SwapToTitleFromOptions(UICallbackPackage package)
        {
            SaveHandler.SaveParameters();
            _type = CurrentMenu.Title;
            _titleUI.OnMoveMouse(_lastMousePosition);
        }

        private void ApplyMusicVolume(UICallbackPackage package)
        {
            AudioManager.MusicVolume = package.Value[0];
            SaveHandler.Parameters.MusicVolume = package.Value[0];
        }

        private void ApplyEffectVolume(UICallbackPackage package)
        {
            AudioManager.EffectVolume = package.Value[0];
            SaveHandler.Parameters.EffectVolume = package.Value[0];
        }

        private void ChangeParticleStyle(UICallbackPackage package)
        {
            if (package.Value[0] < 0) return;
            switch (package.Caller)
            {
                case "t1":
                    SaveHandler.Parameters.ParticleType = 0;
                    ParticleBackdrop.Clear();
                    break;
                case "t2":
                    SaveHandler.Parameters.ParticleType = 1;
                    ParticleBackdrop.ChangeStyle(ParticleBackdrop.Mode.SlowDots);
                    break;
                case "t3":
                    SaveHandler.Parameters.ParticleType = 2;
                    ParticleBackdrop.ChangeStyle(ParticleBackdrop.Mode.Rainy);
                    break;
                case "t4":
                    SaveHandler.Parameters.ParticleType = 3;
                    ParticleBackdrop.ChangeStyle(ParticleBackdrop.Mode.Windy);
                    break;
                case "t5":
                    SaveHandler.Parameters.ParticleType = 4;
                    ParticleBackdrop.ChangeStyle(ParticleBackdrop.Mode.Embers);
                    break;
            }
        }

        private void ChangeParticleColor(UICallbackPackage package)
        {
            if (package.Value[0] < 0) return;
            switch (package.Caller)
            {
                case "c1":
                    SaveHandler.Parameters.ParticleColor = 0;
                    ParticleBackdrop.ChangeColor();
                    break;
                case "c2":
                    SaveHandler.Parameters.ParticleColor = 1;
                    ParticleBackdrop.ChangeColor(ColorScale.White);
                    break;
                case "c3":
                    SaveHandler.Parameters.ParticleColor = 2;
                    ParticleBackdrop.ChangeColor(ColorScale.GetFromGlobal("Pulse"));
                    break;
                case "c4":
                    SaveHandler.Parameters.ParticleColor = 3;
                    ParticleBackdrop.ChangeColor(ColorScale.GetFromGlobal("Rainbow"));
                    break;
                case "c5":
                    SaveHandler.Parameters.ParticleColor = 4;
                    ParticleBackdrop.ChangeColor(ColorScale.GetFromGlobal("Ember"));
                    break;
            }
        }

/********************************************************************
------->Loading Menu Callbacks
********************************************************************/
        private void ToggleLoadButtons()
        {
            var button1 = _loadUI.GetElement(SaveHandler.FILE_1) as UIClickable;
            var button2 = _loadUI.GetElement(SaveHandler.FILE_2) as UIClickable;
            var button3 = _loadUI.GetElement(SaveHandler.FILE_3) as UIClickable;
            if (_deleteMode)
            {
                if (SaveHandler.HasSave(SaveHandler.FILE_1)) button1?.ChangeColor(Color.Red);
                else button1?.Disable(ColorScale.Grey);
                if (SaveHandler.HasSave(SaveHandler.FILE_2)) button2?.ChangeColor(Color.Red);
                else button2?.Disable(ColorScale.Grey);
                if (SaveHandler.HasSave(SaveHandler.FILE_3)) button3?.ChangeColor(Color.Red);
                else button3?.Disable(ColorScale.Grey);
            }
            else
            {
                button1?.ChangeColor(ColorScale.White);
                button2?.ChangeColor(ColorScale.White);
                button3?.ChangeColor(ColorScale.White);
                if (!SaveHandler.HasSave(SaveHandler.FILE_1)) button1?.Enable();
                if (!SaveHandler.HasSave(SaveHandler.FILE_2)) button2?.Enable();
                if (!SaveHandler.HasSave(SaveHandler.FILE_3)) button3?.Enable();
            }
        }

        private async void LoadGame(string caller)
        {
            Container.RequestSectorChange(GameContainer.SectorID.Loading);
            var save = SaveHandler.GetSave(caller, true);
            var success = await Task.Run(() => Container.Unpack(save));
            if (!success)
            {
                GameContainer.Console.ForceMessage("<error>", "Failed to load save", Color.DarkRed, Color.White);
                Container.RequestSectorChange(GameContainer.SectorID.Outerworld);
            }
            else if (save.InVoid)
            {
                Container.StartNewGame(save.Floor, null); //How is this going to work for multiplayer
                AudioManager.PlaySong("Void", true);
            }
            else
            {
                Container.RequestSectorChange(GameContainer.SectorID.Outerworld);
                AudioManager.PlaySong("Outerworld", true);
            }
        }

        private void SelectLoad(UICallbackPackage package)
        {
            if (_deleteMode)
            {
                //TODO: make a popup warning for trying to delete a file(?)
                SaveHandler.DeleteSave(package.Caller);
                UpdateLoadButtonText();
                ToggleLoadButtons();
            }
            else
            {
                if (SaveHandler.HasSave(package.Caller))
                { 
                    LoadGame(package.Caller);
                }
                else
                {
                    _type = CurrentMenu.NewGame;
                    _slot = package.Caller;
                }
            }
        }

        private void ToggleDeleteMode(UICallbackPackage package)
        {
            _deleteMode = !_deleteMode;
            var delText = _loadUI.GetElement("DeleteFileButtonText") as UIText;
            delText?.SetText(_deleteMode ? "Cancel" : "Delete File", ColorScale.Black);
            ToggleLoadButtons();
        }

/********************************************************************
------->New Game Menu Callbacks
********************************************************************/
        private void CancelNewGame(UICallbackPackage package)
        {
            _type = CurrentMenu.Play;
            _loadUI.OnMoveMouse(_lastMousePosition);
        }

        private void PrepForTextEntry(UICallbackPackage package)
        {
            _textEntryMode = package.Value[0] > 0;
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
            var file1Text = SaveHandler.HasSave(SaveHandler.FILE_1) ? SaveHandler.GetSave(SaveHandler.FILE_1, false).TitleCard : "No File";
            var file2Text = SaveHandler.HasSave(SaveHandler.FILE_2) ? SaveHandler.GetSave(SaveHandler.FILE_2, false).TitleCard : "No File";
            var file3Text = SaveHandler.HasSave(SaveHandler.FILE_3) ? SaveHandler.GetSave(SaveHandler.FILE_3, false).TitleCard : "No File";
            (_loadUI.GetElement(SaveHandler.FILE_1 + "Text") as UIText)?.SetText(file1Text);
            (_loadUI.GetElement(SaveHandler.FILE_2 + "Text") as UIText)?.SetText(file2Text);
            (_loadUI.GetElement(SaveHandler.FILE_3 + "Text") as UIText)?.SetText(file3Text);
        }

        private void CreateNewGame(UICallbackPackage package)
        {
            if (!(_newGameUI.GetElement("NewGameInputBoxText") is UIText text) || text.Text.Length == 0) return;
            var newGame = new PackedGame(Container, text.Text);
            SaveHandler.Save(_slot, newGame);
            UpdateLoadButtonText();
            _type = CurrentMenu.Play;
            _loadUI.OnMoveMouse(_lastMousePosition);
            _slot = "";
            _deleteMode = false;
            (_newGameUI.GetElement("NewGameInputBoxText") as UIText)?.SetText("New File");
        }

/********************************************************************
------->Menu Functions
********************************************************************/
        private enum CurrentMenu
        {
            Title, Options, Play, NewGame
        }

        private CurrentMenu _type;
        private string _slot;
        private bool _deleteMode, _textEntryMode;
        private readonly UIPanel _titleUI, _optionsUI, _loadUI, _newGameUI;
        private Point _lastMousePosition;
        private const int MAX_NEWGAME_TEXT_SIZE = 15;


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
            _lastMousePosition = new Point();

            _titleUI = new UIPanel("this.MenuKing", new Vector2(0.5f, 0.5f), Vector2.One, PositionType.Center);
            _optionsUI = new UIPanel("this.MenuKing", new Vector2(0.5f, 0.5f), Vector2.One, PositionType.Center);
            _loadUI = new UIPanel("this.MenuKing", new Vector2(0.5f, 0.5f), Vector2.One, PositionType.Center);
            _newGameUI = new UIPanel("this.MenuKing", new Vector2(0.5f, 0.5f), Vector2.One, PositionType.Center);

            // Title Menu setup
            var titlePanel = new UIPanel("TitleButtonPanel", new Vector2(0.5f, 1), new Vector2(0, 0.5f), PositionType.CenterBottom);
            var playButton = new UIClickable("PlayButton", "UI/button/BigButton", new Vector2(0, 0), ColorScale.White, titlePanel, PositionType.CenterTop);
            var optionsButton = new UIClickable("OptionsButton", "UI/button/BigButton", new Coordinate(0, 10), ColorScale.White, playButton, PositionType.CenterTop, PositionType.CenterBottom);
            var exitButton = new UIClickable("ExitButton", "UI/button/BigButton", new Coordinate(0, 10), ColorScale.White, optionsButton, PositionType.CenterTop, PositionType.CenterBottom);
            var playButtonText = new UIText("TitleButtonText", new Coordinate(), "Play Game", ColorScale.Black, playButton, PositionType.Center, PositionType.Center);
            var optionsButtonText = new UIText("OptionsButtonText", new Coordinate(), "Options", ColorScale.Black, optionsButton, PositionType.Center, PositionType.Center);
            var exitButtonText = new UIText("ExitButtonText", new Coordinate(), "Exit", ColorScale.Black, exitButton, PositionType.Center, PositionType.Center);
            var titleDisplay = new UIElement("Title", "UI/decor/Title", new Vector2(0.5f, 0.25f), ColorScale.GetFromGlobal("Rainbow"), _titleUI, PositionType.Center);

            playButton.RegisterCallback(SwapToLoad);
            optionsButton.RegisterCallback(SwapToOptions);
            exitButton.RegisterCallback(ExitGame);
            titleDisplay.SetRules(TextureLibrary.HalfScaleRuleSet);
            SetExtrasAll("UI/button/BigButtonDown", "UI/button/BigButtonOver", null, null, playButton, optionsButton, exitButton);

            titlePanel.AddElements(playButton, optionsButton, exitButton, playButtonText, optionsButtonText, exitButtonText);
            _titleUI.AddElements(titleDisplay, titlePanel);

            // Option Menu setup
            var backButton = new UIClickable("BackButton", "UI/button/SmallButton", new Coordinate(10, -10), ColorScale.White, _optionsUI, PositionType.BottomLeft, PositionType.BottomLeft);
            var backButtonText = new UIText("BackButtonText", new Coordinate(), "Back", ColorScale.Black, backButton, PositionType.Center, PositionType.Center);
            var effectVolumeBar = new UIElement("EffectVolumeBar", "UI/field/SmallFillBar", new Vector2(0.45f, 0.2f), ColorScale.White, _optionsUI, PositionType.TopRight);
            var musicVolumeBar = new UIElement("MusicVolumeBar", "UI/field/SmallFillBar", new Vector2(0.55f, 0.2f), ColorScale.White, _optionsUI, PositionType.TopLeft);
            var effectVolumeBarFill = new UIExtendable("EffectVolumeBarFill", "UI/fill/BarFillRed", new Coordinate(18, 6), ColorScale.White, effectVolumeBar, PositionType.TopLeft, PositionType.TopLeft, new Coordinate(240, 28), true, false);
            var musicVolumeBarFill = new UIExtendable("MusicVolumeBarFill", "UI/fill/BarFillBlue", new Coordinate(18, 6), ColorScale.White, musicVolumeBar, PositionType.TopLeft, PositionType.TopLeft, new Coordinate(240, 28), true, false);
            var effectVolumeBarText = new UIText("EffectVolumeBarText", new Coordinate(2, -4), "Effect Volume", ColorScale.Grey, effectVolumeBarFill, PositionType.BottomLeft, PositionType.TopLeft);
            var musicVolumeBarText = new UIText("MusicVolumeBarText", new Coordinate(2, -4), "Music Volume", ColorScale.Grey, musicVolumeBarFill, PositionType.BottomLeft, PositionType.TopLeft);
            var particleStylePanel = new UIRadioSet("ParticleStylePanel", new Coordinate(0, 150), new Coordinate(25, 100), effectVolumeBar, PositionType.TopRight, PositionType.Center);
            var particleColorPanel = new UIRadioSet("ParticleColorPanel", new Coordinate(0, 150), new Coordinate(25, 100), effectVolumeBar, PositionType.TopLeft, PositionType.Center);
            var noParticles = new UITogglable("t1", "UI/button/RadioUnselected", Vector2.Zero, ColorScale.White, particleStylePanel, PositionType.Center, SaveHandler.Parameters.ParticleType == 0);
            var randomParticles = new UITogglable("t2", "UI/button/RadioUnselected", new Vector2(0, 0.25f), ColorScale.White, particleStylePanel, PositionType.Center, SaveHandler.Parameters.ParticleType == 1);
            var rainParticles = new UITogglable("t3", "UI/button/RadioUnselected", new Vector2(0, 0.5f), ColorScale.White, particleStylePanel, PositionType.Center, SaveHandler.Parameters.ParticleType == 2);
            var windParticles = new UITogglable("t4", "UI/button/RadioUnselected", new Vector2(0, 0.75f), ColorScale.White, particleStylePanel, PositionType.Center, SaveHandler.Parameters.ParticleType == 3);
            var emberParticles = new UITogglable("t5", "UI/button/RadioUnselected", new Vector2(0, 1), ColorScale.White, particleStylePanel, PositionType.Center, SaveHandler.Parameters.ParticleType == 4);
            var defaultColor = new UITogglable("c1", "UI/button/RadioUnselected", new Vector2(1, 0), ColorScale.White, particleColorPanel, PositionType.Center, SaveHandler.Parameters.ParticleColor == 0);
            var whiteColor = new UITogglable("c2", "UI/button/RadioUnselected", new Vector2(1, 0.25f), ColorScale.White, particleColorPanel, PositionType.Center, SaveHandler.Parameters.ParticleColor == 1);
            var pulseColor = new UITogglable("c3", "UI/button/RadioUnselected", new Vector2(1, 0.5f), ColorScale.White, particleColorPanel, PositionType.Center, SaveHandler.Parameters.ParticleColor == 2);
            var techniColor = new UITogglable("c4", "UI/button/RadioUnselected", new Vector2(1, 0.75f), ColorScale.White, particleColorPanel, PositionType.Center, SaveHandler.Parameters.ParticleColor == 3);
            var moodyColor = new UITogglable("c5", "UI/button/RadioUnselected", new Vector2(1, 1), ColorScale.White, particleColorPanel, PositionType.Center, SaveHandler.Parameters.ParticleColor == 4);
            var particleStyleText = new UIText("ParticleStyleText", new Coordinate(0, -10), "Style", ColorScale.Grey, noParticles, PositionType.BottomRight, PositionType.TopLeft);
            var particleColorText = new UIText("ParticleColorText", new Coordinate(0, -10), "Color", ColorScale.Grey, defaultColor, PositionType.BottomLeft, PositionType.TopRight);
            var particleText = new UIText("ParticleText", new Coordinate(0, -50), "@u17@Particle Settings", ColorScale.Grey, particleStylePanel, PositionType.CenterBottom, PositionType.TopRight);
            var noParticlesText = new UIText("NoParticlesText", new Coordinate(-5, 0), "None", ColorScale.White, noParticles, PositionType.CenterRight, PositionType.CenterLeft);
            var randomParticlesText = new UIText("RandomParticlesText", new Coordinate(-5, 0), "Points", ColorScale.White, randomParticles, PositionType.CenterRight, PositionType.CenterLeft);
            var rainParticlesText = new UIText("RainParticlesText", new Coordinate(-5, 0), "Rainy", new ColorScale(Color.SkyBlue), rainParticles, PositionType.CenterRight, PositionType.CenterLeft);
            var windParticlesText = new UIText("WindParticlesText", new Coordinate(-5, 0), "Windy", new ColorScale(Color.ForestGreen), windParticles, PositionType.CenterRight, PositionType.CenterLeft);
            var emberwParticlesText = new UIText("EmberParticlesText", new Coordinate(-5, 0), "Embers", ColorScale.GetFromGlobal("Ember"), emberParticles, PositionType.CenterRight, PositionType.CenterLeft);
            var defaultColorText = new UIText("DefaultColorText", new Coordinate(5, 0), "Style Default", ColorScale.White, defaultColor, PositionType.CenterLeft, PositionType.CenterRight);
            var whiteColorText = new UIText("WhiteColorText", new Coordinate(5, 0), "White", ColorScale.White, whiteColor, PositionType.CenterLeft, PositionType.CenterRight);
            var pulseColorText = new UIText("PulseColorText", new Coordinate(5, 0), "Pulse", ColorScale.GetFromGlobal("Pulse"), pulseColor, PositionType.CenterLeft, PositionType.CenterRight);
            var techniColorText = new UIText("TechniColorText", new Coordinate(5, 0), "Technicolor", ColorScale.GetFromGlobal("Rainbow"), techniColor, PositionType.CenterLeft, PositionType.CenterRight);
            var moodyColorText = new UIText("MoodyColorText", new Coordinate(5, 0), "Moody", ColorScale.GetFromGlobal("Random"), moodyColor, PositionType.CenterLeft, PositionType.CenterRight);

            backButton.RegisterCallback(SwapToTitleFromOptions);
            effectVolumeBarFill.RegisterCallback(ApplyEffectVolume);
            musicVolumeBarFill.RegisterCallback(ApplyMusicVolume);
            effectVolumeBarFill.ForceValue(new Vector2(AudioManager.EffectVolume, 1));
            musicVolumeBarFill.ForceValue(new Vector2(AudioManager.MusicVolume, 1));
            RegisterAll(ChangeParticleStyle, noParticles, rainParticles, windParticles, randomParticles, emberParticles);
            RegisterAll(ChangeParticleColor, defaultColor, whiteColor, pulseColor, techniColor, moodyColor);
            backButton.SetExtraStates("UI/button/SmallButtonDown", "UI/button/SmallButtonOver");
            SetExtrasAll("UI/button/RadioSelected", "", null, null, noParticles, rainParticles, windParticles, randomParticles, emberParticles, defaultColor, whiteColor, pulseColor, techniColor, moodyColor);

            particleStylePanel.AddElements(noParticles, rainParticles, windParticles, randomParticles, emberParticles, noParticlesText, rainParticlesText, windParticlesText, randomParticlesText, emberwParticlesText, particleStyleText, particleText);
            particleColorPanel.AddElements(defaultColor, pulseColor, whiteColor, techniColor, moodyColor, defaultColorText, whiteColorText, pulseColorText, techniColorText, moodyColorText, particleColorText);
            _optionsUI.AddElements(backButton, backButtonText, effectVolumeBar, musicVolumeBar, effectVolumeBarFill, musicVolumeBarFill, effectVolumeBarText, musicVolumeBarText, particleStylePanel, particleColorPanel);

            // Load Menu setup
            var loadPanel = new UIPanel("LoadFilePanel", new Vector2(0.5f, 1), new Vector2(0, 0.5f), PositionType.CenterBottom);
            var loadFile1 = new UIClickable(SaveHandler.FILE_1, "UI/button/BigButton", new Vector2(0, 0), ColorScale.White, loadPanel, PositionType.CenterTop, true);
            var loadFile2 = new UIClickable(SaveHandler.FILE_2, "UI/button/BigButton", new Coordinate(0, 10), ColorScale.White, loadFile1, PositionType.CenterTop, PositionType.CenterBottom, true);
            var loadFile3 = new UIClickable(SaveHandler.FILE_3, "UI/button/BigButton", new Coordinate(0, 10), ColorScale.White, loadFile2, PositionType.CenterTop, PositionType.CenterBottom, true);
            var loadFile1Text = new UIText(SaveHandler.FILE_1 + "Text", new Coordinate(), "", ColorScale.Black, loadFile1, PositionType.Center, PositionType.Center);
            var loadFile2Text = new UIText(SaveHandler.FILE_2 + "Text", new Coordinate(), "", ColorScale.Black, loadFile2, PositionType.Center, PositionType.Center);
            var loadFile3Text = new UIText(SaveHandler.FILE_3 + "Text", new Coordinate(), "", ColorScale.Black, loadFile3, PositionType.Center, PositionType.Center);
            var deleteFileButton = new UIClickable("DeleteFileButton", "UI/button/SmallButton", new Coordinate(-10, -10), ColorScale.White, _loadUI, PositionType.BottomRight, PositionType.BottomRight);
            var deleteFileButtonText = new UIText("DeleteFileButtonText", new Coordinate(), "Delete File", ColorScale.Black, deleteFileButton, PositionType.Center, PositionType.Center);
            var backButton2 = new UIClickable("BackButton", "UI/button/SmallButton", new Coordinate(10, -10), ColorScale.White, _loadUI, PositionType.BottomLeft, PositionType.BottomLeft);
            var backButtonText2 = new UIText("BackButtonText", new Coordinate(), "Back", ColorScale.Black, backButton2, PositionType.Center, PositionType.Center);

            deleteFileButton.RegisterCallback(ToggleDeleteMode);
            backButton2.RegisterCallback(SwapToTitleFromLoad);
            RegisterAll(SelectLoad, loadFile1, loadFile2, loadFile3);
            SetExtrasAll("UI/button/BigButtonDown", "UI/button/BigButtonOver", null, null, loadFile1, loadFile2, loadFile3);
            backButton2.SetExtraStates("UI/button/SmallButtonDown", "UI/button/SmallButtonOver");
            deleteFileButton.SetExtraStates("UI/button/SmallButtonDown", "UI/button/SmallButtonOver");

            loadPanel.AddElements(loadFile1, loadFile2, loadFile3, loadFile1Text, loadFile2Text, loadFile3Text);
            _loadUI.AddElements(backButton2, backButtonText2, deleteFileButton, deleteFileButtonText, loadPanel, titleDisplay);

            //New Game Menu setup
            var cancelButton = new UIClickable("BackButton", "UI/button/SmallButton", new Coordinate(10, -10), ColorScale.White, _newGameUI, PositionType.BottomLeft, PositionType.BottomLeft);
            var cancelButtonText = new UIText("BackButtonText", new Coordinate(), "Cancel", ColorScale.Black, cancelButton, PositionType.Center, PositionType.Center);
            var confirmButton = new UIClickable("ConfirmButton", "UI/button/SmallButton", new Coordinate(-10, -10), ColorScale.White, _newGameUI, PositionType.BottomRight, PositionType.BottomRight);
            var confirmButtonText = new UIText("ConfirmButtonText", new Coordinate(), "Confirm", ColorScale.Black, confirmButton, PositionType.Center, PositionType.Center);
            var inputBox = new UITogglable("NewGameInputBox", "UI/field/SmallEntryBox", new Vector2(0.1f, 0.1f), ColorScale.Ghost, _newGameUI, PositionType.TopLeft, false, true);
            var inputBoxText = new UIText("NewGameInputBoxText", new Coordinate(20, 20), "New File", ColorScale.Black, inputBox, PositionType.TopLeft, PositionType.TopLeft);

            cancelButton.RegisterCallback(CancelNewGame);
            inputBox.RegisterCallback(PrepForTextEntry);
            confirmButton.RegisterCallback(CreateNewGame);
            inputBoxText.SetRules(TextureLibrary.DoubleScaleRuleSet);
            inputBox.SetExtraStates("", "", ColorScale.White);
            SetExtrasAll("UI/button/SmallButtonDown", "UI/button/SmallButtonOver", null, null, cancelButton, confirmButton);

            _newGameUI.AddElements(cancelButton, cancelButtonText, inputBox, inputBoxText, confirmButton, confirmButtonText);
        }

        public override void Enter(Point point)
        {
            _lastMousePosition = point;
            _titleUI.OnMoveMouse(point);
            Dead = false;
            _type = CurrentMenu.Title;
            (_newGameUI.GetElement("NewGameInputBoxText") as UIText)?.SetText("New File");
            UpdateLoadButtonText();
        }

        public override void BackOut()
        {
            if (_textEntryMode)
            {
                (_newGameUI.GetElement("NewGameInputBox") as UITogglable)?.ForcePush(false);
                Dead = false;
            }
            switch (_type)
            {
                case CurrentMenu.NewGame:
                    _type = CurrentMenu.Play;
                    Dead = false;
                    break;
                case CurrentMenu.Play:
                case CurrentMenu.Options:
                    _type = CurrentMenu.Title;
                    Dead = false;
                    break;
                default:
                    Dead = true;
                    break;
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

        public override bool OnMoveMouse(Point point)
        {
            ActivePanel().OnMoveMouse(point);
            _lastMousePosition = point;
            return false;
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
