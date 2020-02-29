using System.Collections.Generic;
using ExNihilo.Entity;
using ExNihilo.Systems.Backend;
using ExNihilo.Systems.Game;
using ExNihilo.UI;
using ExNihilo.Util;
using ExNihilo.Util.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Position = ExNihilo.Util.Graphics.TextureUtilities.PositionType;

namespace ExNihilo.Menus
{
    public class CharacterMenu : Menu
    {
/********************************************************************
------->Menu Callbacks
********************************************************************/
        private void CloseMenu(UICallbackPackage package)
        {
            Dead = true;
        }

        private void DeleteChar()
        {
            /* Process for deleting a character
             * Character slot is set to null
             * Panel is disabled
             * Portrait is cleared
             * Tooltip is cleared (for good measure)
             * Delete button is disabled
             * Enable new character button because there will always be an open slot post delete
             */
            _chars[_charInJeopardy] = null;
            var panel = _panelUI.GetElement("CharPanel" + (_charInJeopardy + 1)) as UIPanel;
            panel?.GetElement("Portrait" + (_charInJeopardy + 1))?.ChangeTexture(TextureLibrary.Lookup("null"));
            var text = panel?.GetElement("CharInfoText" + (_charInJeopardy + 1)) as UIText;
            text?.SetText("", ColorScale.Black);
            panel?.Disable(ColorScale.Grey);
            (_panelUI.GetElement("Delete" + (_charInJeopardy + 1)) as UIClickable)?.Disable(ColorScale.Grey);

            _selectedChar = _currentChar;
            _panelUI.GetElement("CharacterDisplay")?.ChangeTexture(_chars[_selectedChar].Entity.GetTexture(EntityTexture.State.DownMoving));
            (_panelUI.GetElement("NewCharButton") as UIClickable)?.Enable();
            Container.Pack(); //Save game
        }

        private void WarnOnDelete(UICallbackPackage package)
        {
            switch (package.Caller)
            {
                case "Delete1":
                    _charInJeopardy = 0;
                    break;
                case "Delete2":
                    _charInJeopardy = 1;
                    break;
                case "Delete3":
                    _charInJeopardy = 2;
                    break;
                case "Delete4":
                    _charInJeopardy = 3;
                    break;
                case "Delete5":
                    _charInJeopardy = 4;
                    break;
                case "Delete6":
                    _charInJeopardy = 5;
                    break;
                case "Delete7":
                    _charInJeopardy = 6;
                    break;
            }

            var message = "Are you sure you want to\ndelete " + _chars[_charInJeopardy].Name + "?";
            _warningMessage = new NoteMenu(Container, message, false);
            _warningMessage.LoadContent(Container.GraphicsDevice, Container.Content);
            _type = CurrentMenu.Warning;
            _warningMessage.Enter(_lastMousePosition);
            _warningMessage.OnResize(Container.GraphicsDevice, _lastWindowSize);
        }

        private void MakeNewChar(UICallbackPackage package)
        {
            _type = CurrentMenu.New;
            (_newCharUI.GetElement("NewCharInputBoxText") as UIText)?.SetText("New Char");
            _newCharUI.OnMoveMouse(_lastMousePosition);
        }

        private void ChangeChar(UICallbackPackage package)
        {
            /* Process for swapping characters
             * Delete button for old character is enabled
             * Delete button for new character is disabled
             * Set current character as new character
             * Main portrait is set as new character
             * Disable change character button since the selected and current characters are the same
             */
            (_panelUI.GetElement("Delete" + (_currentChar + 1)) as UIClickable)?.Enable();
            (_panelUI.GetElement("Delete" + (_selectedChar + 1)) as UIClickable)?.Disable(ColorScale.Grey);
            _chars[_selectedChar].Entity.SetState(_world.GetCurrentState());
            _currentChar = _selectedChar;
            _world.SwapEntity(_chars[_currentChar]);
            (_panelUI.GetElement("ChangeCharButton") as UIClickable)?.Disable(ColorScale.Grey);
            Container.Pack(); //Save game
        }

        private void SelectChar(UICallbackPackage package)
        {
            var tmp = 0;
            switch (package.Caller)
            {
                case "CharPanel1":
                    tmp = 0;
                    break;
                case "CharPanel2":
                    tmp = 1;
                    break;
                case "CharPanel3":
                    tmp = 2;
                    break;
                case "CharPanel4":
                    tmp = 3;
                    break;
                case "CharPanel5":
                    tmp = 4;
                    break;
                case "CharPanel6":
                    tmp = 5;
                    break;
                case "CharPanel7":
                    tmp = 6;
                    break;
            }

            if (_chars[tmp] is null) return;

            _selectedChar = tmp;
            if (_selectedChar == _currentChar) (_panelUI.GetElement("ChangeCharButton") as UIClickable)?.Disable(ColorScale.Grey);
            else (_panelUI.GetElement("ChangeCharButton") as UIClickable)?.Enable();

            _panelUI.GetElement("CharacterDisplay")?.ChangeTexture(_chars[_selectedChar].Entity.GetTexture(EntityTexture.State.DownMoving));
        }

/********************************************************************
------->New Character Callbacks
********************************************************************/
        private void CancelNewChar(UICallbackPackage package)
        {
            _type = CurrentMenu.Main;
            _panelUI.OnMoveMouse(_lastMousePosition);
        }

        private void ChangeCharacter(UICallbackPackage package)
        {
            var left = package.Caller.EndsWith("Left");
            
            if (package.Caller.StartsWith("Cloth"))
            {
                if (left) _cloth = (_cloth - 1 + _clothCount) % _clothCount;
                else _cloth = (_cloth + 1) % _clothCount;
                var sheet = TextureLibrary.Lookup("Char/cloth/" + (_cloth+1));
                var rect = new Rectangle(0, sheet.Height / 2, sheet.Width, sheet.Height / 4);
                var cloth = new AnimatableTexture(TextureUtilities.GetSubTexture(Container.GraphicsDevice, sheet, rect), 4, 4);
                _newCharUI.GetElement("CharClothDisplay")?.ChangeTexture(cloth);
                _newCharUI.GetElement("CharBodyDisplay")?.ResetTextureFrame();
                _newCharUI.GetElement("CharHairDisplay")?.ResetTextureFrame();
            }
            else if (package.Caller.StartsWith("Hair"))
            {
                if (left) _hair = (_hair - 1 + _hairCount) % _hairCount;
                else _hair = (_hair + 1) % _hairCount;
                var sheet = TextureLibrary.Lookup("Char/hair/" + (_hair+1) + "-" + (_color+1));
                var rect = new Rectangle(0, sheet.Height / 2, sheet.Width, sheet.Height / 4);
                var hair = new AnimatableTexture(TextureUtilities.GetSubTexture(Container.GraphicsDevice, sheet, rect), 4, 4);
                _newCharUI.GetElement("CharHairDisplay")?.ChangeTexture(hair);
                _newCharUI.GetElement("CharBodyDisplay")?.ResetTextureFrame();
                _newCharUI.GetElement("CharClothDisplay")?.ResetTextureFrame();
            }
            else if (package.Caller.StartsWith("Color"))
            {
                if (left) _color = (_color - 1 + _colorCount) % _colorCount;
                else _color = (_color + 1) % _colorCount;
                var sheet = TextureLibrary.Lookup("Char/hair/" + (_hair+1) + "-" + (_color+1));
                var rect = new Rectangle(0, sheet.Height / 2, sheet.Width, sheet.Height / 4);
                var hair = new AnimatableTexture(TextureUtilities.GetSubTexture(Container.GraphicsDevice, sheet, rect), 4, 4);
                _newCharUI.GetElement("CharHairDisplay")?.ChangeTexture(hair);
                _newCharUI.GetElement("CharBodyDisplay")?.ResetTextureFrame();
                _newCharUI.GetElement("CharClothDisplay")?.ResetTextureFrame();
            }
            else if (package.Caller.StartsWith("Body"))
            {
                if (left) _body = (_body - 1 + _bodyCount) % _bodyCount;
                else _body = (_body + 1) % _bodyCount;
                var sheet = TextureLibrary.Lookup("Char/base/" + (_body+1));
                var rect = new Rectangle(0, sheet.Height / 2, sheet.Width, sheet.Height / 4);
                var body = new AnimatableTexture(TextureUtilities.GetSubTexture(Container.GraphicsDevice, sheet, rect), 4, 4);
                _newCharUI.GetElement("CharBodyDisplay")?.ChangeTexture(body);
                _newCharUI.GetElement("CharClothDisplay")?.ResetTextureFrame();
                _newCharUI.GetElement("CharHairDisplay")?.ResetTextureFrame();
            }
        }

        private void ConfirmNewChar(UICallbackPackage package)
        {
            if (!(_newCharUI.GetElement("NewCharInputBoxText") is UIText text) || text.Text.Length == 0) return;

            var newChar = new PlayerEntityContainer(Container.GraphicsDevice, text.Text, _body, _hair, _cloth, _color);
            var slot = GetFirstOpening();
            _chars[slot] = newChar;

            /* Process for creating a new character
             * Empty character panel is enabled
             * Portrait is set as new character
             * Tooltip text is set as info for new character
             * Delete button for new character is enabled (since new character isn't swapped to automatically) 
             */
            var panel = _panelUI.GetElement("CharPanel" + (slot + 1)) as UIPanel;
            panel?.Enable();
            panel?.GetElement("Portrait" + (slot+1))?.ChangeTexture(newChar.Texture);
            var t = panel?.GetElement("CharInfoText" + (slot + 1)) as UIText;
            t?.SetText(_chars[slot].ToString(), ColorScale.Black);
            (panel?.GetElement("Delete" + (slot + 1)) as UIClickable)?.Enable();
            
            _type = CurrentMenu.Main;
            _panelUI.OnMoveMouse(_lastMousePosition);
            if (slot == MAX_CHARACTERS-1)
            {
                (_panelUI.GetElement("NewCharButton") as UIClickable)?.Disable(ColorScale.Grey);
            }
            ResetNewChar(Container.GraphicsDevice);
            Container.Pack(); //Save game
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

/********************************************************************
------->Menu Functions
********************************************************************/
        private enum CurrentMenu
        {
            Main, New, Warning
        }
        private CurrentMenu _type;

        private readonly World _world;
        private NoteMenu _warningMessage;
        private readonly UIPanel _panelUI, _newCharUI;
        private int _body, _hair, _cloth, _color;
        private int _selectedChar, _charInJeopardy, _currentChar;
        private bool _textEntryMode;
        private Point _lastMousePosition;
        private Coordinate _lastWindowSize;
        private readonly PlayerEntityContainer[] _chars = new PlayerEntityContainer[MAX_CHARACTERS];

        public const int MAX_NEWCHAR_TEXT_SIZE = 15, MAX_CHARACTERS = 7;
        private const int _bodyCount = 3, _hairCount = 42, _clothCount = 43, _colorCount = 10;

        public CharacterMenu(GameContainer container, World world) : base(container)
        {
            _world = world;
            var rules = new ScaleRuleSet
            (
                new ScaleRule(1.5f, 1400, 1000),
                new ScaleRule(3, 2100, 1500),
                new ScaleRule(4.5f, 2800, 2000),
                new ScaleRule(6, ScaleRule.MAX_X, ScaleRule.MAX_Y)
            );

            _panelUI = new UIPanel("this.MenuKing", new Vector2(0.5f, 0.5f), Vector2.One, Position.Center);
            _newCharUI = new UIPanel("this.MenuKing", new Vector2(0.5f, 0.5f), Vector2.One, Position.Center);

            //Main Panel
            var backdrop = new UIElement("Backdrop", "UI/decor/Backdrop", new Vector2(0.5f, 0.5f), Color.White, _panelUI, Position.Center);
            var exitButton = new UIClickable("ExitButton", "UI/button/RedBulb", new Coordinate(-8, 8), ColorScale.White, backdrop, Position.Center, Position.TopRight);
            var exitButtonX = new UIElement("ExitButtonX", "UI/icon/No", new Coordinate(), ColorScale.White, exitButton, Position.Center, Position.Center);
            exitButton.RegisterCallback(CloseMenu);
            SetRulesAll(TextureLibrary.MediumScaleRuleSet, exitButton, exitButtonX);
            exitButton.SetExtraStates("UI/button/RedBulbDown", "UI/button/RedBulbOver");

            var currentCharacterSet = new UIElement("CharacterSet", "UI/field/SevenElementSet", new Coordinate(18, -100), ColorScale.White, backdrop, Position.BottomLeft, Position.BottomLeft);
            var newCharButton = new UIClickable("NewCharButton", "UI/button/SmallButton", new Coordinate(-50, 50), ColorScale.White, backdrop, Position.TopRight, Position.TopRight);
            var newCharButtonText = new UIText("NewCharButtonText", new Coordinate(), "New", ColorScale.Black, newCharButton, Position.Center, Position.Center);
            var changeCharButton = new UIClickable("ChangeCharButton", "UI/button/SmallButton", new Coordinate(0, 25), ColorScale.White, newCharButton, Position.TopLeft, Position.BottomLeft);
            var changeCharButtonText = new UIText("ChangeCharButtonText", new Coordinate(), " @hChange\nCharacter", ColorScale.Black, changeCharButton, Position.Center, Position.Center);
            var charDisplay = new UIElement("CharacterDisplay", "null", new Coordinate(50, 50), ColorScale.White, backdrop, Position.TopLeft, Position.TopLeft);

            var charArray = new UIPanel("CurrentCharArray", new Coordinate(38, 12), new Coordinate(588, 60), currentCharacterSet, Position.TopLeft, Position.TopLeft);
            var charPanel1 = new UIPanel("CharPanel1", new Coordinate(), new Coordinate(60, 60), charArray, Position.TopLeft, Position.TopLeft);
            var charPanel2 = new UIPanel("CharPanel2", new Coordinate(28, 0), new Coordinate(60, 60), charPanel1, Position.TopLeft, Position.TopRight);
            var charPanel3 = new UIPanel("CharPanel3", new Coordinate(28, 0), new Coordinate(60, 60), charPanel2, Position.TopLeft, Position.TopRight);
            var charPanel4 = new UIPanel("CharPanel4", new Coordinate(28, 0), new Coordinate(60, 60), charPanel3, Position.TopLeft, Position.TopRight);
            var charPanel5 = new UIPanel("CharPanel5", new Coordinate(28, 0), new Coordinate(60, 60), charPanel4, Position.TopLeft, Position.TopRight);
            var charPanel6 = new UIPanel("CharPanel6", new Coordinate(28, 0), new Coordinate(60, 60), charPanel5, Position.TopLeft, Position.TopRight);
            var charPanel7 = new UIPanel("CharPanel7", new Coordinate(28, 0), new Coordinate(60, 60), charPanel6, Position.TopLeft, Position.TopRight);

            var portrait1 = new UIElement("Portrait1", "null", new Coordinate(), ColorScale.GetFromGlobal("Random"), charPanel1, Position.Center, Position.Center);
            var portrait2 = new UIElement("Portrait2", "null", new Coordinate(), ColorScale.GetFromGlobal("Random"), charPanel2, Position.Center, Position.Center);
            var portrait3 = new UIElement("Portrait3", "null", new Coordinate(), ColorScale.GetFromGlobal("Random"), charPanel3, Position.Center, Position.Center);
            var portrait4 = new UIElement("Portrait4", "null", new Coordinate(), ColorScale.GetFromGlobal("Random"), charPanel4, Position.Center, Position.Center);
            var portrait5 = new UIElement("Portrait5", "null", new Coordinate(), ColorScale.GetFromGlobal("Random"), charPanel5, Position.Center, Position.Center);
            var portrait6 = new UIElement("Portrait6", "null", new Coordinate(), ColorScale.GetFromGlobal("Random"), charPanel6, Position.Center, Position.Center);
            var portrait7 = new UIElement("Portrait7", "null", new Coordinate(), ColorScale.GetFromGlobal("Random"), charPanel7, Position.Center, Position.Center);

            var deleteButton1 = new UIClickable("Delete1", "UI/button/RedBulb", new Coordinate(), ColorScale.White, charPanel1, Position.Center, Position.TopRight);
            var deleteButton2 = new UIClickable("Delete2", "UI/button/RedBulb", new Coordinate(), ColorScale.White, charPanel2, Position.Center, Position.TopRight);
            var deleteButton3 = new UIClickable("Delete3", "UI/button/RedBulb", new Coordinate(), ColorScale.White, charPanel3, Position.Center, Position.TopRight);
            var deleteButton4 = new UIClickable("Delete4", "UI/button/RedBulb", new Coordinate(), ColorScale.White, charPanel4, Position.Center, Position.TopRight);
            var deleteButton5 = new UIClickable("Delete5", "UI/button/RedBulb", new Coordinate(), ColorScale.White, charPanel5, Position.Center, Position.TopRight);
            var deleteButton6 = new UIClickable("Delete6", "UI/button/RedBulb", new Coordinate(), ColorScale.White, charPanel6, Position.Center, Position.TopRight);
            var deleteButton7 = new UIClickable("Delete7", "UI/button/RedBulb", new Coordinate(), ColorScale.White, charPanel7, Position.Center, Position.TopRight);

            var deleteButton1Icon = new UIElement("Delete1Icon", "UI/icon/No", new Coordinate(), ColorScale.Ghost, deleteButton1, Position.Center, Position.Center);
            var deleteButton2Icon = new UIElement("Delete2Icon", "UI/icon/No", new Coordinate(), ColorScale.Ghost, deleteButton2, Position.Center, Position.Center);
            var deleteButton3Icon = new UIElement("Delete3Icon", "UI/icon/No", new Coordinate(), ColorScale.Ghost, deleteButton3, Position.Center, Position.Center);
            var deleteButton4Icon = new UIElement("Delete4Icon", "UI/icon/No", new Coordinate(), ColorScale.Ghost, deleteButton4, Position.Center, Position.Center);
            var deleteButton5Icon = new UIElement("Delete5Icon", "UI/icon/No", new Coordinate(), ColorScale.Ghost, deleteButton5, Position.Center, Position.Center);
            var deleteButton6Icon = new UIElement("Delete6Icon", "UI/icon/No", new Coordinate(), ColorScale.Ghost, deleteButton6, Position.Center, Position.Center);
            var deleteButton7Icon = new UIElement("Delete7Icon", "UI/icon/No", new Coordinate(), ColorScale.Ghost, deleteButton7, Position.Center, Position.Center);

            var charInfo1 = new UIElement("CharInfo1", "UI/field/LargeEntryBox", new Coordinate(), ColorScale.White, charPanel1, Position.BottomLeft, Position.TopLeft);
            var charInfo2 = new UIElement("CharInfo2", "UI/field/LargeEntryBox", new Coordinate(), ColorScale.White, charPanel2, Position.CenterBottom, Position.TopLeft);
            var charInfo3 = new UIElement("CharInfo3", "UI/field/LargeEntryBox", new Coordinate(), ColorScale.White, charPanel3, Position.CenterBottom, Position.TopLeft);
            var charInfo4 = new UIElement("CharInfo4", "UI/field/LargeEntryBox", new Coordinate(), ColorScale.White, charPanel4, Position.CenterBottom, Position.TopLeft);
            var charInfo5 = new UIElement("CharInfo5", "UI/field/LargeEntryBox", new Coordinate(), ColorScale.White, charPanel5, Position.CenterBottom, Position.TopLeft);
            var charInfo6 = new UIElement("CharInfo6", "UI/field/LargeEntryBox", new Coordinate(), ColorScale.White, charPanel6, Position.CenterBottom, Position.TopLeft);
            var charInfo7 = new UIElement("CharInfo7", "UI/field/LargeEntryBox", new Coordinate(), ColorScale.White, charPanel7, Position.BottomRight, Position.TopLeft);

            var charInfoText1 = new UIText("CharInfoText1", new Coordinate(14, 14), "Box1", ColorScale.Black, charInfo1, Position.TopLeft, Position.TopLeft);
            var charInfoText2 = new UIText("CharInfoText2", new Coordinate(14, 14), "Box2", ColorScale.Black, charInfo2, Position.TopLeft, Position.TopLeft);
            var charInfoText3 = new UIText("CharInfoText3", new Coordinate(14, 14), "Box3", ColorScale.Black, charInfo3, Position.TopLeft, Position.TopLeft);
            var charInfoText4 = new UIText("CharInfoText4", new Coordinate(14, 14), "Box4", ColorScale.Black, charInfo4, Position.TopLeft, Position.TopLeft);
            var charInfoText5 = new UIText("CharInfoText5", new Coordinate(14, 14), "Box5", ColorScale.Black, charInfo5, Position.TopLeft, Position.TopLeft);
            var charInfoText6 = new UIText("CharInfoText6", new Coordinate(14, 14), "Box6", ColorScale.Black, charInfo6, Position.TopLeft, Position.TopLeft);
            var charInfoText7 = new UIText("CharInfoText7", new Coordinate(14, 14), "Box7", ColorScale.Black, charInfo7, Position.TopLeft, Position.TopLeft);

            charPanel1.AddTooltip(new Coordinate(60, 60), new Vector2(0, -17), charInfo1, charInfoText1);
            charPanel2.AddTooltip(new Coordinate(60, 60), new Vector2(0, -17), charInfo2, charInfoText2);
            charPanel3.AddTooltip(new Coordinate(60, 60), new Vector2(0, -17), charInfo3, charInfoText3);
            charPanel4.AddTooltip(new Coordinate(60, 60), new Vector2(0, -17), charInfo4, charInfoText4);
            charPanel5.AddTooltip(new Coordinate(60, 60), new Vector2(0, -17), charInfo5, charInfoText5);
            charPanel6.AddTooltip(new Coordinate(60, 60), new Vector2(0, -17), charInfo6, charInfoText6);
            charPanel7.AddTooltip(new Coordinate(60, 60), new Vector2(0, -17), charInfo7, charInfoText7);

            newCharButton.RegisterCallback(MakeNewChar);
            changeCharButton.RegisterCallback(ChangeChar);
            RegisterAll(WarnOnDelete, deleteButton1, deleteButton2, deleteButton3, deleteButton4, deleteButton5, deleteButton6, deleteButton7);
            RegisterAll(SelectChar, charPanel1, charPanel2, charPanel3, charPanel4, charPanel5, charPanel6, charPanel7);
            SetExtrasAll("UI/button/SmallButtonDown", "UI/button/SmallButtonOver", null, null, newCharButton, changeCharButton);
            SetExtrasAll("UI/button/RedBulbDown", "UI/button/RedBulbOver", null, null, deleteButton1, deleteButton2, deleteButton3, deleteButton4, deleteButton5, deleteButton6, deleteButton7);
            charDisplay.SetRules(TextureLibrary.GiantScaleRuleSet);
            SetRulesAll(rules, portrait1, portrait2, portrait3, portrait4, portrait5, portrait6, portrait7);
            DisableAll(ColorScale.Grey, charPanel1, charPanel2, charPanel3, charPanel4, charPanel5, charPanel6, charPanel7);

            charPanel1.AddElements(portrait1, deleteButton1, deleteButton1Icon);
            charPanel2.AddElements(portrait2, deleteButton2, deleteButton2Icon);
            charPanel3.AddElements(portrait3, deleteButton3, deleteButton3Icon);
            charPanel4.AddElements(portrait4, deleteButton4, deleteButton4Icon);
            charPanel5.AddElements(portrait5, deleteButton5, deleteButton5Icon);
            charPanel6.AddElements(portrait6, deleteButton6, deleteButton6Icon);
            charPanel7.AddElements(portrait7, deleteButton7, deleteButton7Icon);
            charArray.AddElements(charPanel1, charPanel2, charPanel3, charPanel4, charPanel5, charPanel6, charPanel7);
            _panelUI.AddElements(backdrop, currentCharacterSet, exitButton, exitButtonX, newCharButton, newCharButtonText, changeCharButton, changeCharButtonText, charArray, charDisplay);

            //New Character Panel
            var backdrop2 = new UIElement("Backdrop2", "UI/decor/Backdrop", new Vector2(0.5f, 0.5f), Color.White, _newCharUI, Position.Center);
            var cancelButton = new UIClickable("CancelButton", "UI/button/SmallButton", new Coordinate(10, -10), ColorScale.White, backdrop2, Position.BottomLeft, Position.BottomLeft);
            var cancelButtonText = new UIText("CancelButtonText", new Coordinate(), "Cancel", ColorScale.Black, cancelButton, Position.Center, Position.Center);
            var confirmButton = new UIClickable("ConfirmButton", "UI/button/SmallButton", new Coordinate(-10, -10), ColorScale.White, backdrop2, Position.BottomRight, Position.BottomRight);
            var confirmButtonText = new UIText("ConfirmButtonText", new Coordinate(), "Confirm", ColorScale.Black, confirmButton, Position.Center, Position.Center);
            var inputBox = new UITogglable("NewCharInputBox", "UI/field/SmallEntryBox", new Coordinate(50, 50), ColorScale.Ghost, backdrop2, Position.TopLeft, Position.TopLeft, false, true);
            var inputBoxText = new UIText("NewCharInputBoxText", new Coordinate(20, 20), "New Char", ColorScale.Black, inputBox, Position.TopLeft, Position.TopLeft);

            var charDesignPanel = new UIPanel("CharDesignPanel", new Coordinate(-50, 50), new Coordinate(200, 125), backdrop2, Position.TopRight, Position.TopRight);
            var charBodyDisplay = new UIElement("CharBodyDisplay", "null", new Coordinate(), ColorScale.White, charDesignPanel, Position.Center, Position.Center);
            var charClothDisplay = new UIElement("CharClothDisplay", "null", new Coordinate(), ColorScale.White, charDesignPanel, Position.Center, Position.Center);
            var charHairDisplay = new UIElement("CharHairDisplay", "null", new Coordinate(), ColorScale.White, charDesignPanel, Position.Center, Position.Center);

            var leftHair = new UIClickable("HairLeft", "UI/button/RedBulb", new Vector2(), ColorScale.White, charDesignPanel, Position.Center);
            var leftBody = new UIClickable("BodyLeft", "UI/button/GreenBulb", new Vector2(0, 0.33f), ColorScale.White, charDesignPanel, Position.Center);
            var leftCloth = new UIClickable("ClothLeft", "UI/button/BlueBulb", new Vector2(0, 0.67f), ColorScale.White, charDesignPanel, Position.Center);
            var leftColor = new UIClickable("ColorLeft", "UI/button/BlackBulb", new Vector2(0, 1), ColorScale.White, charDesignPanel, Position.Center);

            var rightHair = new UIClickable("HairRight", "UI/button/RedBulb", new Vector2(1, 0), ColorScale.White, charDesignPanel, Position.Center);
            var rightBody = new UIClickable("BodyRight", "UI/button/GreenBulb", new Vector2(1, 0.33f), ColorScale.White, charDesignPanel, Position.Center);
            var rightCloth = new UIClickable("ClothRight", "UI/button/BlueBulb", new Vector2(1, 0.67f), ColorScale.White, charDesignPanel, Position.Center);
            var rightColor = new UIClickable("ColorRight", "UI/button/BlackBulb", new Vector2(1, 1), ColorScale.White, charDesignPanel, Position.Center);

            var leftArrow1 = new UIElement("Left1", "UI/icon/Left", new Coordinate(), ColorScale.Ghost, leftHair, Position.Center, Position.Center);
            var leftArrow2 = new UIElement("Left2", "UI/icon/Left", new Coordinate(), ColorScale.Ghost, leftBody, Position.Center, Position.Center);
            var leftArrow3 = new UIElement("Left3", "UI/icon/Left", new Coordinate(), ColorScale.Ghost, leftCloth, Position.Center, Position.Center);
            var leftArrow4 = new UIElement("Left4", "UI/icon/Left", new Coordinate(), ColorScale.Ghost, leftColor, Position.Center, Position.Center);
            
            var rightArrow1 = new UIElement("Right1", "UI/icon/Right", new Coordinate(), ColorScale.Ghost, rightHair, Position.Center, Position.Center);
            var rightArrow2 = new UIElement("Right2", "UI/icon/Right", new Coordinate(), ColorScale.Ghost, rightBody, Position.Center, Position.Center);
            var rightArrow3 = new UIElement("Right3", "UI/icon/Right", new Coordinate(), ColorScale.Ghost, rightCloth, Position.Center, Position.Center);
            var rightArrow4 = new UIElement("Right4", "UI/icon/Right", new Coordinate(), ColorScale.Ghost, rightColor, Position.Center, Position.Center);

            cancelButton.RegisterCallback(CancelNewChar);
            confirmButton.RegisterCallback(ConfirmNewChar);
            inputBox.RegisterCallback(PrepForTextEntry);
            RegisterAll(ChangeCharacter, leftHair, leftBody, leftCloth, leftColor, rightBody, rightCloth, rightColor, rightHair);
            SetExtrasAll("UI/button/SmallButtonDown", "UI/button/SmallButtonOver", null, null, confirmButton, cancelButton);
            SetExtrasAll("UI/button/RedBulbDown", "UI/button/RedBulbOver", null, null, leftHair, rightHair);
            SetExtrasAll("UI/button/GreenBulbDown", "UI/button/GreenBulbOver", null, null, leftBody, rightBody);
            SetExtrasAll("UI/button/BlueBulbDown", "UI/button/BlueBulbOver", null, null, leftCloth, rightCloth);
            SetExtrasAll("UI/button/BlackBulbDown", "UI/button/BlackBulbOver", null, null, leftColor, rightColor);
            inputBox.SetExtraStates("", "", ColorScale.White);
            inputBoxText.SetRules(TextureLibrary.DoubleScaleRuleSet);
            SetRulesAll(TextureLibrary.GiantScaleRuleSet, charBodyDisplay, charHairDisplay, charClothDisplay);

            charDesignPanel.AddElements(charBodyDisplay, charClothDisplay, charHairDisplay, leftBody, leftCloth, leftColor, leftHair, rightColor, rightBody, rightCloth, rightHair,
                leftArrow1, leftArrow2, leftArrow3, leftArrow4, rightArrow1, rightArrow2, rightArrow3, rightArrow4);
            _newCharUI.AddElements(backdrop2, cancelButton, cancelButtonText, confirmButton, confirmButtonText, charDesignPanel, inputBox, inputBoxText);
        }

        public override void Enter(Point point)
        {
            _charInJeopardy = -1;
            _lastMousePosition = point;
            _panelUI.OnMoveMouse(point);
            Dead = false;
            _textEntryMode = false;
            _type = CurrentMenu.Main;
            _selectedChar = _currentChar;

            var text = _panelUI.GetElement("CharInfoText" + (_currentChar + 1)) as UIText;
            text?.SetText(_chars[_currentChar].ToString(), ColorScale.Black);
            (_panelUI.GetElement("ChangeCharButton") as UIClickable)?.Disable(ColorScale.Grey);
            _panelUI.GetElement("CharacterDisplay")?.ChangeTexture(_chars[_selectedChar].Entity.GetTexture(EntityTexture.State.DownMoving));
        }

        private int GetFirstOpening()
        {
            for (int i = 0; i < _chars.Length; i++)
            {
                if (_chars[i] is null) return i;
            }

            return -1;
        }

        private void ResetNewChar(GraphicsDevice graphics)
        {
            _body = _hair = _cloth = _color = 0;
            var sheet = TextureLibrary.Lookup("Char/base/" + (_body + 1));
            var rect = new Rectangle(0, sheet.Height / 2, sheet.Width, sheet.Height / 4);
            var body = new AnimatableTexture(TextureUtilities.GetSubTexture(graphics, sheet, rect), 4, 4);
            var cloth = new AnimatableTexture(TextureUtilities.GetSubTexture(graphics, TextureLibrary.Lookup("Char/cloth/" + (_cloth + 1)), rect), 4, 4);
            var hair = new AnimatableTexture(TextureUtilities.GetSubTexture(graphics, TextureLibrary.Lookup("Char/hair/" + (_hair + 1) + "-" + (_color + 1)), rect), 4, 4);
            _newCharUI.GetElement("CharBodyDisplay")?.ChangeTexture(body);
            _newCharUI.GetElement("CharClothDisplay")?.ChangeTexture(cloth);
            _newCharUI.GetElement("CharHairDisplay")?.ChangeTexture(hair);
        }

        public override void LoadContent(GraphicsDevice graphics, ContentManager content)
        {
            _panelUI.LoadContent(graphics, content);
            _newCharUI.LoadContent(graphics, content);
            ResetNewChar(graphics);
        }

        public override void OnResize(GraphicsDevice graphics, Coordinate gameWindow)
        {
            _lastWindowSize = gameWindow;
            _panelUI.OnResize(graphics, gameWindow);
            _newCharUI.OnResize(graphics, gameWindow);
            _warningMessage?.OnResize(graphics, gameWindow);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            switch (_type)
            {
                case CurrentMenu.Main:
                    _panelUI.Draw(spriteBatch);
                    break;
                case CurrentMenu.New:
                    _newCharUI.Draw(spriteBatch);
                    break;
                case CurrentMenu.Warning:
                    _panelUI.Draw(spriteBatch);
                    _warningMessage.Draw(spriteBatch);
                    break;
            }
        }

        public override bool OnMoveMouse(Point point)
        {
            _lastMousePosition = point;
            switch (_type)
            {
                case CurrentMenu.Main:
                    _panelUI.OnMoveMouse(point);
                    break;
                case CurrentMenu.New:
                    _newCharUI.OnMoveMouse(point);
                    break;
                case CurrentMenu.Warning:
                    _panelUI.OnMoveMouse(point);
                    _warningMessage.OnMoveMouse(point);
                    break;
            }

            return false;
        }

        public override bool OnLeftClick(Point point)
        {
            switch (_type)
            {
                case CurrentMenu.Main:
                    return _panelUI.OnLeftClick(point);
                case CurrentMenu.New:
                    return _newCharUI.OnLeftClick(point);
                case CurrentMenu.Warning:
                    return _warningMessage.OnLeftClick(point);
            }

            return false;
        }

        public override void OnLeftRelease(Point point)
        {
            switch (_type)
            {
                case CurrentMenu.Main:
                    _panelUI.OnLeftRelease(point);
                    break;
                case CurrentMenu.New:
                    _newCharUI.OnLeftRelease(point);
                    break;
                case CurrentMenu.Warning:
                    _warningMessage.OnLeftRelease(point);
                    if (_warningMessage.Confirmed)
                    {
                        DeleteChar();
                        _warningMessage = null;
                        _charInJeopardy = -1;
                        _type = CurrentMenu.Main;
                        _panelUI.OnMoveMouse(_lastMousePosition);
                    }
                    else if (_warningMessage.Dead)
                    {
                        _warningMessage = null;
                        _charInJeopardy = -1;
                        _type = CurrentMenu.Main;
                        _panelUI.OnMoveMouse(_lastMousePosition);
                    }
                    break;
            }
        }

        public override void Backspace(int len)
        {
            if (!_textEntryMode) return;

            if (_newCharUI.GetElement("NewCharInputBoxText") is UIText text)
            {
                if (text.Text.Length >= len) text.SetText(text.Text.Substring(0, text.Text.Length - len));
            }
        }

        public override void ReceiveInput(string input)
        {
            if(!_textEntryMode) return;

            if (_newCharUI.GetElement("NewCharInputBoxText") is UIText text)
            {
                text.SetText(Utilities.Clamp(text.Text + input, MAX_NEWCHAR_TEXT_SIZE));
            }
        }

        public override void Pack(PackedGame game)
        {
            //save characters
            var arr = new PlayerEntityContainer.PackedPlayerEntityContainer[MAX_CHARACTERS];
            for (int i = 0; i < _chars.Length; i++) arr[i] = _chars[i]?.GetPacked();
            game.SavedCharacters = arr;
            game.CurrentPlayer = _currentChar;
        }

        public override void Unpack(PackedGame game)
        {
            //take characters
            for (int i=0; i<game.SavedCharacters.Length; i++)
            {
                var c = game.SavedCharacters[i];
                if (c is null)
                {
                    var panel = _panelUI.GetElement("CharPanel" + (i + 1)) as UIPanel;
                    panel?.GetElement("Portrait" + (i + 1))?.ChangeTexture(TextureLibrary.Lookup("null"));
                    var text = panel?.GetElement("CharInfoText" + (i + 1)) as UIText;
                    text?.SetText("", ColorScale.Black);
                    panel?.Disable(ColorScale.Grey);
                    (_panelUI.GetElement("Delete" + (i + 1)) as UIClickable)?.Disable(ColorScale.Grey);
                }
                else
                {
                    _chars[i] = new PlayerEntityContainer(Container.GraphicsDevice, c.Name, c.TextureSet[0], c.TextureSet[1], c.TextureSet[2], c.TextureSet[3], c.Inventory);
                    var panel = _panelUI.GetElement("CharPanel" + (i + 1)) as UIPanel;
                    panel?.Enable();
                    panel?.GetElement("Portrait" + (i + 1))?.ChangeTexture(_chars[i].Entity.GetTexture(EntityTexture.State.Down));
                    var text = panel?.GetElement("CharInfoText" + (i + 1)) as UIText;
                    text?.SetText(_chars[i].ToString(), ColorScale.Black);
                    var delete = panel?.GetElement("Delete" + (i + 1)) as UIClickable;
                    if (i == game.CurrentPlayer) delete?.Disable(ColorScale.Grey);
                    else delete?.Enable();
                }
            }

            if (GetFirstOpening() == -1) (_panelUI.GetElement("NewCharButton") as UIClickable)?.Disable(ColorScale.Grey);
            _selectedChar = _currentChar = game.CurrentPlayer;
            _panelUI.GetElement("CharacterDisplay")?.ChangeTexture(_chars[_selectedChar].Entity.GetTexture(EntityTexture.State.DownMoving));
        }

        public PlayerEntityContainer GetCurrentChar()
        {
            return _chars[_currentChar];
        }
    }
}
