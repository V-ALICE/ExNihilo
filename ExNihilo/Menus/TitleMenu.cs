using System.Threading.Tasks;
using ExNihilo.Input.Controllers;
using ExNihilo.Systems;
using ExNihilo.UI;
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
            if (_deleteMode)
            {
                ((UITogglable)_loadUI.GetElement("DeleteFileButton"))?.ForcePush(false);
            }
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

        private async void LoadGame(string caller)
        {
            Container.RequestSectorChange(GameContainer.SectorID.Loading);
            var success = await Task.Run(() => Container.Unpack(SaveHandler.GetSave(caller)));
            Container.RequestSectorChange(success ? GameContainer.SectorID.Outerworld : GameContainer.SectorID.MainMenu);
        }

        private void SelectLoad(UICallbackPackage package)
        {
            if (_deleteMode)
            {
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
            _deleteMode = package.Value[0] > 0;
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
            SaveHandler.Save(_slot, newGame);
            UpdateLoadButtonText();
            _type = CurrentMenu.Play;
            _loadUI.OnMoveMouse(_lastMousePosition);
            _slot = "";
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
        private string _slot;
        private bool _deleteMode, _textEntryMode;
        private readonly UIPanel _titleUI, _optionsUI, _loadUI, _newGameUI;
        private Point _lastMousePosition;
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
            _lastMousePosition = new Point();

            _titleUI = new UIPanel("this.MenuKing", new Vector2(0.5f, 0.5f), Vector2.One, TextureUtilities.PositionType.Center);
            _optionsUI = new UIPanel("this.MenuKing", new Vector2(0.5f, 0.5f), Vector2.One, TextureUtilities.PositionType.Center);
            _loadUI = new UIPanel("this.MenuKing", new Vector2(0.5f, 0.5f), Vector2.One, TextureUtilities.PositionType.Center);
            _newGameUI = new UIPanel("this.MenuKing", new Vector2(0.5f, 0.5f), Vector2.One, TextureUtilities.PositionType.Center);

            // Title Menu setup
            var titlePanel = new UIPanel("TitleButtonPanel", new Vector2(0.5f, 1), new Vector2(0, 0.5f), TextureUtilities.PositionType.CenterBottom);
            var playButton = new UIClickable("PlayButton", "UI/button/BigButton", new Vector2(0, 0), ColorScale.White, titlePanel, TextureUtilities.PositionType.CenterTop);
            var optionsButton = new UIClickable("OptionsButton", "UI/button/BigButton", new Coordinate(0, 10), ColorScale.White, playButton, TextureUtilities.PositionType.CenterTop, TextureUtilities.PositionType.CenterBottom);
            var exitButton = new UIClickable("ExitButton", "UI/button/BigButton", new Coordinate(0, 10), ColorScale.White, optionsButton, TextureUtilities.PositionType.CenterTop, TextureUtilities.PositionType.CenterBottom);
            var playButtonText = new UIText("TitleButtonText", new Coordinate(), "Play Game", ColorScale.Black, playButton, TextureUtilities.PositionType.Center, TextureUtilities.PositionType.Center);
            var optionsButtonText = new UIText("OptionsButtonText", new Coordinate(), "Options", ColorScale.Black, optionsButton, TextureUtilities.PositionType.Center, TextureUtilities.PositionType.Center);
            var exitButtonText = new UIText("ExitButtonText", new Coordinate(), "Exit", ColorScale.Black, exitButton, TextureUtilities.PositionType.Center, TextureUtilities.PositionType.Center);
            var titleDisplay = new UIElement("Title", "UI/decor/Title", new Vector2(0.5f, 0.25f), ColorScale.GetFromGlobal("Rainbow"), _titleUI, TextureUtilities.PositionType.Center);

            playButton.RegisterCallback(SwapToLoad);
            optionsButton.RegisterCallback(SwapToOptions);
            exitButton.RegisterCallback(ExitGame);
            titleDisplay.SetRules(TextureLibrary.HalfScaleRuleSet);
            playButton.SetExtraStates("UI/button/BigButtonDown", "UI/button/BigButtonOver");
            optionsButton.SetExtraStates("UI/button/BigButtonDown", "UI/button/BigButtonOver");
            exitButton.SetExtraStates("UI/button/BigButtonDown", "UI/button/BigButtonOver");

            titlePanel.AddElements(playButton, optionsButton, exitButton, playButtonText, optionsButtonText, exitButtonText);
            _titleUI.AddElements(titleDisplay, titlePanel);

            // Option Menu setup
            var backButton = new UIClickable("BackButton", "UI/button/SmallButton", new Coordinate(10, -10), ColorScale.White, _optionsUI, TextureUtilities.PositionType.BottomLeft, TextureUtilities.PositionType.BottomLeft);
            var backButtonText = new UIText("BackButtonText", new Coordinate(), "Back", ColorScale.Black, backButton, TextureUtilities.PositionType.Center, TextureUtilities.PositionType.Center);
            var effectVolumeBar = new UIElement("EffectVolumeBar", "UI/field/SmallFillBar", new Vector2(0.45f, 0.2f), ColorScale.White, _optionsUI, TextureUtilities.PositionType.TopRight);
            var musicVolumeBar = new UIElement("MusicVolumeBar", "UI/field/SmallFillBar", new Vector2(0.55f, 0.2f), ColorScale.White, _optionsUI, TextureUtilities.PositionType.TopLeft);
            var effectVolumeBarFill = new UIExtendable("EffectVolumeBarFill", "UI/fill/BarFillRed", new Coordinate(18, 6), ColorScale.White, effectVolumeBar, TextureUtilities.PositionType.TopLeft, TextureUtilities.PositionType.TopLeft, new Coordinate(240, 28), true, false);
            var musicVolumeBarFill = new UIExtendable("MusicVolumeBarFill", "UI/fill/BarFillBlue", new Coordinate(18, 6), ColorScale.White, musicVolumeBar, TextureUtilities.PositionType.TopLeft, TextureUtilities.PositionType.TopLeft, new Coordinate(240, 28), true, false);
            var effectVolumeBarText = new UIText("EffectVolumeBarText", new Coordinate(2, -4), "Effect Volume", ColorScale.Grey, effectVolumeBarFill, TextureUtilities.PositionType.BottomLeft, TextureUtilities.PositionType.TopLeft);
            var musicVolumeBarText = new UIText("MusicVolumeBarText", new Coordinate(2, -4), "Music Volume", ColorScale.Grey, musicVolumeBarFill, TextureUtilities.PositionType.BottomLeft, TextureUtilities.PositionType.TopLeft);
            var particleStylePanel = new UIRadioSet("ParticleStylePanel", new Coordinate(0, 150), new Coordinate(25, 100), effectVolumeBar, TextureUtilities.PositionType.TopRight, TextureUtilities.PositionType.Center);
            var particleColorPanel = new UIRadioSet("ParticleColorPanel", new Coordinate(0, 150), new Coordinate(25, 100), effectVolumeBar, TextureUtilities.PositionType.TopLeft, TextureUtilities.PositionType.Center);
            var noParticles = new UITogglable("t1", "UI/button/RadioUnselected", Vector2.Zero, ColorScale.White, particleStylePanel, TextureUtilities.PositionType.Center, SaveHandler.Parameters.ParticleType == 0);
            var randomParticles = new UITogglable("t2", "UI/button/RadioUnselected", new Vector2(0, 0.25f), ColorScale.White, particleStylePanel, TextureUtilities.PositionType.Center, SaveHandler.Parameters.ParticleType == 1);
            var rainParticles = new UITogglable("t3", "UI/button/RadioUnselected", new Vector2(0, 0.5f), ColorScale.White, particleStylePanel, TextureUtilities.PositionType.Center, SaveHandler.Parameters.ParticleType == 2);
            var windParticles = new UITogglable("t4", "UI/button/RadioUnselected", new Vector2(0, 0.75f), ColorScale.White, particleStylePanel, TextureUtilities.PositionType.Center, SaveHandler.Parameters.ParticleType == 3);
            var emberParticles = new UITogglable("t5", "UI/button/RadioUnselected", new Vector2(0, 1), ColorScale.White, particleStylePanel, TextureUtilities.PositionType.Center, SaveHandler.Parameters.ParticleType == 4);
            var defaultColor = new UITogglable("c1", "UI/button/RadioUnselected", new Vector2(1, 0), ColorScale.White, particleColorPanel, TextureUtilities.PositionType.Center, SaveHandler.Parameters.ParticleColor == 0);
            var whiteColor = new UITogglable("c2", "UI/button/RadioUnselected", new Vector2(1, 0.25f), ColorScale.White, particleColorPanel, TextureUtilities.PositionType.Center, SaveHandler.Parameters.ParticleColor == 1);
            var pulseColor = new UITogglable("c3", "UI/button/RadioUnselected", new Vector2(1, 0.5f), ColorScale.White, particleColorPanel, TextureUtilities.PositionType.Center, SaveHandler.Parameters.ParticleColor == 2);
            var techniColor = new UITogglable("c4", "UI/button/RadioUnselected", new Vector2(1, 0.75f), ColorScale.White, particleColorPanel, TextureUtilities.PositionType.Center, SaveHandler.Parameters.ParticleColor == 3);
            var moodyColor = new UITogglable("c5", "UI/button/RadioUnselected", new Vector2(1, 1), ColorScale.White, particleColorPanel, TextureUtilities.PositionType.Center, SaveHandler.Parameters.ParticleColor == 4);
            var particleStyleText = new UIText("ParticleStyleText", new Coordinate(0, -10), "Style", ColorScale.Grey, noParticles, TextureUtilities.PositionType.BottomRight, TextureUtilities.PositionType.TopLeft);
            var particleColorText = new UIText("ParticleColorText", new Coordinate(0, -10), "Color", ColorScale.Grey, defaultColor, TextureUtilities.PositionType.BottomLeft, TextureUtilities.PositionType.TopRight);
            var particleText = new UIText("ParticleText", new Coordinate(0, -50), "@u17@Particle Settings", ColorScale.Grey, particleStylePanel, TextureUtilities.PositionType.CenterBottom, TextureUtilities.PositionType.TopRight);
            var noParticlesText = new UIText("NoParticlesText", new Coordinate(-5, 0), "None", ColorScale.White, noParticles, TextureUtilities.PositionType.CenterRight, TextureUtilities.PositionType.CenterLeft);
            var randomParticlesText = new UIText("RandomParticlesText", new Coordinate(-5, 0), "Points", ColorScale.White, randomParticles, TextureUtilities.PositionType.CenterRight, TextureUtilities.PositionType.CenterLeft);
            var rainParticlesText = new UIText("RainParticlesText", new Coordinate(-5, 0), "Rainy", new ColorScale(Color.SkyBlue), rainParticles, TextureUtilities.PositionType.CenterRight, TextureUtilities.PositionType.CenterLeft);
            var windParticlesText = new UIText("WindParticlesText", new Coordinate(-5, 0), "Windy", new ColorScale(Color.ForestGreen), windParticles, TextureUtilities.PositionType.CenterRight, TextureUtilities.PositionType.CenterLeft);
            var emberwParticlesText = new UIText("EmberParticlesText", new Coordinate(-5, 0), "Embers", ColorScale.GetFromGlobal("Ember"), emberParticles, TextureUtilities.PositionType.CenterRight, TextureUtilities.PositionType.CenterLeft);
            var defaultColorText = new UIText("DefaultColorText", new Coordinate(5, 0), "Default", ColorScale.White, defaultColor, TextureUtilities.PositionType.CenterLeft, TextureUtilities.PositionType.CenterRight);
            var whiteColorText = new UIText("WhiteColorText", new Coordinate(5, 0), "White", ColorScale.White, whiteColor, TextureUtilities.PositionType.CenterLeft, TextureUtilities.PositionType.CenterRight);
            var pulseColorText = new UIText("PulseColorText", new Coordinate(5, 0), "Pulse", ColorScale.GetFromGlobal("Pulse"), pulseColor, TextureUtilities.PositionType.CenterLeft, TextureUtilities.PositionType.CenterRight);
            var techniColorText = new UIText("TechniColorText", new Coordinate(5, 0), "Technicolor", ColorScale.GetFromGlobal("Rainbow"), techniColor, TextureUtilities.PositionType.CenterLeft, TextureUtilities.PositionType.CenterRight);
            var moodyColorText = new UIText("MoodyColorText", new Coordinate(5, 0), "Moody", ColorScale.GetFromGlobal("Random"), moodyColor, TextureUtilities.PositionType.CenterLeft, TextureUtilities.PositionType.CenterRight);

            backButton.RegisterCallback(SwapToTitleFromOptions);
            effectVolumeBarFill.RegisterCallback(ApplyEffectVolume);
            musicVolumeBarFill.RegisterCallback(ApplyMusicVolume);
            effectVolumeBarFill.ForceValue(new Vector2(AudioManager.EffectVolume, 1));
            musicVolumeBarFill.ForceValue(new Vector2(AudioManager.MusicVolume, 1));
            noParticles.RegisterCallback(ChangeParticleStyle);
            rainParticles.RegisterCallback(ChangeParticleStyle);
            windParticles.RegisterCallback(ChangeParticleStyle);
            randomParticles.RegisterCallback(ChangeParticleStyle);
            emberParticles.RegisterCallback(ChangeParticleStyle);
            defaultColor.RegisterCallback(ChangeParticleColor);
            pulseColor.RegisterCallback(ChangeParticleColor);
            whiteColor.RegisterCallback(ChangeParticleColor);
            techniColor.RegisterCallback(ChangeParticleColor);
            moodyColor.RegisterCallback(ChangeParticleColor);
            backButton.SetExtraStates("UI/button/SmallButtonDown", "UI/button/SmallButtonOver");
            noParticles.SetExtraStates("UI/button/RadioSelected");
            rainParticles.SetExtraStates("UI/button/RadioSelected");
            windParticles.SetExtraStates("UI/button/RadioSelected");
            randomParticles.SetExtraStates("UI/button/RadioSelected");
            emberParticles.SetExtraStates("UI/button/RadioSelected");
            defaultColor.SetExtraStates("UI/button/RadioSelected");
            whiteColor.SetExtraStates("UI/button/RadioSelected");
            pulseColor.SetExtraStates("UI/button/RadioSelected");
            techniColor.SetExtraStates("UI/button/RadioSelected");
            moodyColor.SetExtraStates("UI/button/RadioSelected");

            particleStylePanel.AddElements(noParticles, rainParticles, windParticles, randomParticles, emberParticles, noParticlesText, rainParticlesText, windParticlesText, randomParticlesText, emberwParticlesText, particleStyleText, particleText);
            particleColorPanel.AddElements(defaultColor, pulseColor, whiteColor, techniColor, moodyColor, defaultColorText, whiteColorText, pulseColorText, techniColorText, moodyColorText, particleColorText);
            _optionsUI.AddElements(backButton, backButtonText, effectVolumeBar, musicVolumeBar, effectVolumeBarFill, musicVolumeBarFill, effectVolumeBarText, musicVolumeBarText, particleStylePanel, particleColorPanel);

            // Load Menu setup
            var loadPanel = new UIPanel("LoadFilePanel", new Vector2(0.5f, 1), new Vector2(0, 0.5f), TextureUtilities.PositionType.CenterBottom);
            var loadFile1 = new UIClickable(SaveHandler.FILE_1, "UI/button/BigButton", new Vector2(0, 0), ColorScale.White, loadPanel, TextureUtilities.PositionType.CenterTop);
            var loadFile2 = new UIClickable(SaveHandler.FILE_2, "UI/button/BigButton", new Coordinate(0, 10), ColorScale.White, loadFile1, TextureUtilities.PositionType.CenterTop, TextureUtilities.PositionType.CenterBottom);
            var loadFile3 = new UIClickable(SaveHandler.FILE_3, "UI/button/BigButton", new Coordinate(0, 10), ColorScale.White, loadFile2, TextureUtilities.PositionType.CenterTop, TextureUtilities.PositionType.CenterBottom);
            var loadFile1Text = new UIText(SaveHandler.FILE_1 + "Text", new Coordinate(), "", ColorScale.Black, loadFile1, TextureUtilities.PositionType.Center, TextureUtilities.PositionType.Center);
            var loadFile2Text = new UIText(SaveHandler.FILE_2 + "Text", new Coordinate(), "", ColorScale.Black, loadFile2, TextureUtilities.PositionType.Center, TextureUtilities.PositionType.Center);
            var loadFile3Text = new UIText(SaveHandler.FILE_3 + "Text", new Coordinate(), "", ColorScale.Black, loadFile3, TextureUtilities.PositionType.Center, TextureUtilities.PositionType.Center);
            var deleteFileButton = new UITogglable("DeleteFileButton", "UI/button/SmallButton", new Coordinate(-10, -10), ColorScale.White, _loadUI, TextureUtilities.PositionType.BottomRight, TextureUtilities.PositionType.BottomRight, false);
            var deleteFileButtonText = new UIText("DeleteFileButtonText", new Coordinate(), "Delete File", ColorScale.Black, deleteFileButton, TextureUtilities.PositionType.Center, TextureUtilities.PositionType.Center);
            var backButton2 = new UIClickable("BackButton", "UI/button/SmallButton", new Coordinate(10, -10), ColorScale.White, _loadUI, TextureUtilities.PositionType.BottomLeft, TextureUtilities.PositionType.BottomLeft);
            var backButtonText2 = new UIText("BackButtonText", new Coordinate(), "Back", ColorScale.Black, backButton2, TextureUtilities.PositionType.Center, TextureUtilities.PositionType.Center);

            loadFile1.RegisterCallback(SelectLoad);
            loadFile2.RegisterCallback(SelectLoad);
            loadFile3.RegisterCallback(SelectLoad);
            deleteFileButton.RegisterCallback(ToggleDeleteMode);
            backButton2.RegisterCallback(SwapToTitleFromLoad);
            loadFile1.SetExtraStates("UI/button/BigButtonDown", "UI/button/BigButtonOver");
            loadFile2.SetExtraStates("UI/button/BigButtonDown", "UI/button/BigButtonOver");
            loadFile3.SetExtraStates("UI/button/BigButtonDown", "UI/button/BigButtonOver");
            deleteFileButton.SetExtraStates("UI/button/SmallButtonDown", "UI/button/SmallButtonOver");
            backButton2.SetExtraStates("UI/button/SmallButtonDown", "UI/button/SmallButtonOver");

            loadPanel.AddElements(loadFile1, loadFile2, loadFile3, loadFile1Text, loadFile2Text, loadFile3Text);
            _loadUI.AddElements(backButton2, backButtonText2, titleDisplay, deleteFileButton, deleteFileButtonText, loadPanel);

            //New Game Menu setup
            var cancelButton = new UIClickable("BackButton", "UI/button/SmallButton", new Coordinate(10, -10), ColorScale.White, _newGameUI, TextureUtilities.PositionType.BottomLeft, TextureUtilities.PositionType.BottomLeft);
            var cancelButtonText = new UIText("BackButtonText", new Coordinate(), "Cancel", ColorScale.Black, cancelButton, TextureUtilities.PositionType.Center, TextureUtilities.PositionType.Center);
            var confirmButton = new UIClickable("ConfirmButton", "UI/button/SmallButton", new Coordinate(-10, -10), ColorScale.White, _newGameUI, TextureUtilities.PositionType.BottomRight, TextureUtilities.PositionType.BottomRight);
            var confirmButtonText = new UIText("ConfirmButtonText", new Coordinate(), "Confirm", ColorScale.Black, confirmButton, TextureUtilities.PositionType.Center, TextureUtilities.PositionType.Center);
            var inputBox = new UITogglable("NewGameInputBox", "UI/field/SmallEntryBox", new Vector2(0.1f, 0.1f), ColorScale.Grey, _newGameUI, TextureUtilities.PositionType.TopLeft, false, true);
            var inputBoxText = new UIText("NewGameInputBoxText", new Coordinate(20, 20), "", ColorScale.Black, inputBox, TextureUtilities.PositionType.TopLeft, TextureUtilities.PositionType.TopLeft);

            cancelButton.RegisterCallback(CancelNewGame);
            inputBox.RegisterCallback(PrepForTextEntry);
            confirmButton.RegisterCallback(CreateNewGame);
            inputBoxText.SetRules(TextureLibrary.DoubleScaleRuleSet);
            cancelButton.SetExtraStates("UI/button/SmallButtonDown", "UI/button/SmallButtonOver");
            confirmButton.SetExtraStates("UI/button/SmallButtonDown", "UI/button/SmallButtonOver");
            inputBox.SetExtraStates("", "", ColorScale.White);

            _newGameUI.AddElements(cancelButton, cancelButtonText, inputBox, inputBoxText, confirmButton, confirmButtonText);
        }

        public override void Enter(Point point)
        {
            _lastMousePosition = point;
            _titleUI.OnMoveMouse(point);
            Dead = false;
            _type = CurrentMenu.Title;
            (_newGameUI.GetElement("NewGameInputBoxText") as UIText)?.SetText("");
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

        public override void OnMoveMouse(Point point)
        {
            ActivePanel().OnMoveMouse(point);
            _lastMousePosition = point;
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
