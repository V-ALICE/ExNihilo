using System;
using System.Collections.Generic;
using ExNihilo.Entity;
using ExNihilo.Systems;
using ExNihilo.Systems.Backend;
using ExNihilo.Systems.Bases;
using ExNihilo.Systems.Game;
using ExNihilo.UI;
using ExNihilo.Util;
using ExNihilo.Util.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

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

        private void CheckDeleteCharButtons()
        {
            if (!(_panelUI.GetElement("CurrentCharArray") is UIPanel panel)) return;
            if (_characters.Count > 1 && _currentChar != 0) (panel.GetElement("CharPanel1") as UIPanel)?.Enable();
            else (panel.GetElement("CharPanel1") as UIPanel)?.Disable(ColorScale.Grey);
            for (int i = 1; i < MAX_CHARACTERS; i++)
            {
                var del = panel.GetElement("CharPanel"+(i+1)) as UIPanel;
                if (i < _characters.Count && _currentChar != i) del?.Enable();
                else del?.Disable(ColorScale.Grey);
            }
        }

        private void DeleteChar()
        {
            if (!(_panelUI.GetElement("CurrentCharArray") is UIPanel panel)) return;

            if (_charInJeopardy < _currentChar) _currentChar--;
            if (_charInJeopardy <= _selectedChar && _selectedChar != 0) _selectedChar--;
            (_panelUI.GetElement("CharacterDisplay") as UIDynamicElement)?.ChangeTexture(_characters[_selectedChar].Entity.GetTexture(EntityTexture.State.DownMoving));

            _characters.RemoveAt(_charInJeopardy);
            for (int i = 0; i < MAX_CHARACTERS; i++)
            {
                var tex = i < _characters.Count ? _characters[i].Entity.GetTexture(EntityTexture.State.Down) : (AnimatableTexture)TextureLibrary.Lookup("null");
                (panel.GetElement("Portrait" + (i + 1)) as UIDynamicElement)?.ChangeTexture(tex);
            }
            
            if (_characters.Count < MAX_CHARACTERS)
            {
                (_panelUI.GetElement("NewCharButton") as UIClickable)?.Enable();
            }
            CheckDeleteCharButtons();
            if (_selectedChar == _currentChar) (_panelUI.GetElement("ChangeCharButton") as UIClickable)?.Disable(ColorScale.Grey);
            else (_panelUI.GetElement("ChangeCharButton") as UIClickable)?.Enable();

            Container.Pack();
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

            var message = "Are you sure you want to\ndelete " + _characters[_charInJeopardy].Name + "?";
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
            _characters[_selectedChar].Entity.SetState(_world.GetCurrentState());
            _currentChar = _selectedChar;
            _world.SwapEntity(_characters[_currentChar]);
            (_panelUI.GetElement("ChangeCharButton") as UIClickable)?.Disable(ColorScale.Grey);
            CheckDeleteCharButtons();
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

            if (tmp >= _characters.Count) return;

            _selectedChar = tmp;
            if (_selectedChar == _currentChar) (_panelUI.GetElement("ChangeCharButton") as UIClickable)?.Disable(ColorScale.Grey);
            else (_panelUI.GetElement("ChangeCharButton") as UIClickable)?.Enable();

            (_panelUI.GetElement("CharacterDisplay") as UIDynamicElement)?.ChangeTexture(_characters[_selectedChar].Entity.GetTexture(EntityTexture.State.DownMoving));
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
                (_newCharUI.GetElement("CharClothDisplay") as UIDynamicElement)?.ChangeTexture(cloth);
                (_newCharUI.GetElement("CharBodyDisplay") as UIDynamicElement)?.ResetTexture();
                (_newCharUI.GetElement("CharHairDisplay") as UIDynamicElement)?.ResetTexture();
            }
            else if (package.Caller.StartsWith("Hair"))
            {
                if (left) _hair = (_hair - 1 + _hairCount) % _hairCount;
                else _hair = (_hair + 1) % _hairCount;
                var sheet = TextureLibrary.Lookup("Char/hair/" + (_hair+1) + "-" + (_color+1));
                var rect = new Rectangle(0, sheet.Height / 2, sheet.Width, sheet.Height / 4);
                var hair = new AnimatableTexture(TextureUtilities.GetSubTexture(Container.GraphicsDevice, sheet, rect), 4, 4);
                (_newCharUI.GetElement("CharHairDisplay") as UIDynamicElement)?.ChangeTexture(hair);
                (_newCharUI.GetElement("CharBodyDisplay") as UIDynamicElement)?.ResetTexture();
                (_newCharUI.GetElement("CharClothDisplay") as UIDynamicElement)?.ResetTexture();
            }
            else if (package.Caller.StartsWith("Color"))
            {
                if (left) _color = (_color - 1 + _colorCount) % _colorCount;
                else _color = (_color + 1) % _colorCount;
                var sheet = TextureLibrary.Lookup("Char/hair/" + (_hair+1) + "-" + (_color+1));
                var rect = new Rectangle(0, sheet.Height / 2, sheet.Width, sheet.Height / 4);
                var hair = new AnimatableTexture(TextureUtilities.GetSubTexture(Container.GraphicsDevice, sheet, rect), 4, 4);
                (_newCharUI.GetElement("CharHairDisplay") as UIDynamicElement)?.ChangeTexture(hair);
                (_newCharUI.GetElement("CharBodyDisplay") as UIDynamicElement)?.ResetTexture();
                (_newCharUI.GetElement("CharClothDisplay") as UIDynamicElement)?.ResetTexture();
            }
            else if (package.Caller.StartsWith("Body"))
            {
                if (left) _body = (_body - 1 + _bodyCount) % _bodyCount;
                else _body = (_body + 1) % _bodyCount;
                var sheet = TextureLibrary.Lookup("Char/base/" + (_body+1));
                var rect = new Rectangle(0, sheet.Height / 2, sheet.Width, sheet.Height / 4);
                var body = new AnimatableTexture(TextureUtilities.GetSubTexture(Container.GraphicsDevice, sheet, rect), 4, 4);
                (_newCharUI.GetElement("CharBodyDisplay") as UIDynamicElement)?.ChangeTexture(body);
                (_newCharUI.GetElement("CharClothDisplay") as UIDynamicElement)?.ResetTexture();
                (_newCharUI.GetElement("CharHairDisplay") as UIDynamicElement)?.ResetTexture();
            }
        }

        private void ConfirmNewChar(UICallbackPackage package)
        {
            if (!(_newCharUI.GetElement("NewCharInputBoxText") is UIText text) || text.Text.Length == 0) return;

            var newChar = new PlayerEntityContainer(Container.GraphicsDevice, text.Text, _body, _hair, _cloth, _color);
            _characters.Add(newChar);
            (_panelUI.GetElement("Portrait" + _characters.Count) as UIDynamicElement)?.ChangeTexture(newChar.Texture);
            CheckDeleteCharButtons();

            _type = CurrentMenu.Main;
            _panelUI.OnMoveMouse(_lastMousePosition);
            if (_characters.Count == MAX_CHARACTERS)
            {
                (_panelUI.GetElement("NewCharButton") as UIClickable)?.Disable(ColorScale.Grey);
            }
            ResetNewChar(Container.GraphicsDevice);
            Container.Pack();
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
        private readonly List<PlayerEntityContainer> _characters = new List<PlayerEntityContainer>();

        private const int MAX_NEWCHAR_TEXT_SIZE = 15, MAX_CHARACTERS = 7;
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

            _panelUI = new UIPanel("this.MenuKing", new Vector2(0.5f, 0.5f), Vector2.One, TextureUtilities.PositionType.Center);
            _newCharUI = new UIPanel("this.MenuKing", new Vector2(0.5f, 0.5f), Vector2.One, TextureUtilities.PositionType.Center);

            //Main Panel
            var backdrop = new UIElement("Backdrop", "UI/decor/Backdrop", new Vector2(0.5f, 0.5f), Color.White, _panelUI, TextureUtilities.PositionType.Center);
            var backButton = new UIClickable("BackButton", "UI/button/SmallButton", new Coordinate(14, -14), ColorScale.White, backdrop, TextureUtilities.PositionType.BottomLeft, TextureUtilities.PositionType.BottomLeft);
            var backButtonText = new UIText("BackButtonText", new Coordinate(), "Back", ColorScale.Black, backButton, TextureUtilities.PositionType.Center, TextureUtilities.PositionType.Center);
            var currentCharacterSet = new UIElement("CharacterSet", "UI/field/SevenElementSet", new Coordinate(18, -100), ColorScale.White, backdrop, TextureUtilities.PositionType.BottomLeft, TextureUtilities.PositionType.BottomLeft);
            var newCharButton = new UIClickable("NewCharButton", "UI/button/SmallButton", new Coordinate(-50, 50), ColorScale.White, backdrop, TextureUtilities.PositionType.TopRight, TextureUtilities.PositionType.TopRight);
            var newCharButtonText = new UIText("NewCharButtonText", new Coordinate(), "New", ColorScale.Black, newCharButton, TextureUtilities.PositionType.Center, TextureUtilities.PositionType.Center);
            var changeCharButton = new UIClickable("ChangeCharButton", "UI/button/SmallButton", new Coordinate(0, 25), ColorScale.White, newCharButton, TextureUtilities.PositionType.TopLeft, TextureUtilities.PositionType.BottomLeft);
            var changeCharButtonText = new UIText("ChangeCharButtonText", new Coordinate(), " @hChange\nCharacter", ColorScale.Black, changeCharButton, TextureUtilities.PositionType.Center, TextureUtilities.PositionType.Center);
            var charDisplay = new UIDynamicElement("CharacterDisplay", "null", new Coordinate(50, 50), ColorScale.White, backdrop, TextureUtilities.PositionType.TopLeft, TextureUtilities.PositionType.TopLeft);

            var charArray = new UIPanel("CurrentCharArray", new Coordinate(38, 12), new Coordinate(588, 60), currentCharacterSet, TextureUtilities.PositionType.TopLeft, TextureUtilities.PositionType.TopLeft);
            var charPanel1 = new UIPanel("CharPanel1", new Coordinate(), new Coordinate(60, 60), charArray, TextureUtilities.PositionType.TopLeft, TextureUtilities.PositionType.TopLeft);
            var charPanel2 = new UIPanel("CharPanel2", new Coordinate(28, 0), new Coordinate(60, 60), charPanel1, TextureUtilities.PositionType.TopLeft, TextureUtilities.PositionType.TopRight);
            var charPanel3 = new UIPanel("CharPanel3", new Coordinate(28, 0), new Coordinate(60, 60), charPanel2, TextureUtilities.PositionType.TopLeft, TextureUtilities.PositionType.TopRight);
            var charPanel4 = new UIPanel("CharPanel4", new Coordinate(28, 0), new Coordinate(60, 60), charPanel3, TextureUtilities.PositionType.TopLeft, TextureUtilities.PositionType.TopRight);
            var charPanel5 = new UIPanel("CharPanel5", new Coordinate(28, 0), new Coordinate(60, 60), charPanel4, TextureUtilities.PositionType.TopLeft, TextureUtilities.PositionType.TopRight);
            var charPanel6 = new UIPanel("CharPanel6", new Coordinate(28, 0), new Coordinate(60, 60), charPanel5, TextureUtilities.PositionType.TopLeft, TextureUtilities.PositionType.TopRight);
            var charPanel7 = new UIPanel("CharPanel7", new Coordinate(28, 0), new Coordinate(60, 60), charPanel6, TextureUtilities.PositionType.TopLeft, TextureUtilities.PositionType.TopRight);

            var portrait1 = new UIDynamicElement("Portrait1", "null", new Coordinate(), ColorScale.GetFromGlobal("Random"), charPanel1, TextureUtilities.PositionType.Center, TextureUtilities.PositionType.Center);
            var portrait2 = new UIDynamicElement("Portrait2", "null", new Coordinate(), ColorScale.GetFromGlobal("Random"), charPanel2, TextureUtilities.PositionType.Center, TextureUtilities.PositionType.Center);
            var portrait3 = new UIDynamicElement("Portrait3", "null", new Coordinate(), ColorScale.GetFromGlobal("Random"), charPanel3, TextureUtilities.PositionType.Center, TextureUtilities.PositionType.Center);
            var portrait4 = new UIDynamicElement("Portrait4", "null", new Coordinate(), ColorScale.GetFromGlobal("Random"), charPanel4, TextureUtilities.PositionType.Center, TextureUtilities.PositionType.Center);
            var portrait5 = new UIDynamicElement("Portrait5", "null", new Coordinate(), ColorScale.GetFromGlobal("Random"), charPanel5, TextureUtilities.PositionType.Center, TextureUtilities.PositionType.Center);
            var portrait6 = new UIDynamicElement("Portrait6", "null", new Coordinate(), ColorScale.GetFromGlobal("Random"), charPanel6, TextureUtilities.PositionType.Center, TextureUtilities.PositionType.Center);
            var portrait7 = new UIDynamicElement("Portrait7", "null", new Coordinate(), ColorScale.GetFromGlobal("Random"), charPanel7, TextureUtilities.PositionType.Center, TextureUtilities.PositionType.Center);

            var deleteButton1 = new UIClickable("Delete1", "UI/button/RedBulb", new Coordinate(), ColorScale.White, charPanel1, TextureUtilities.PositionType.Center, TextureUtilities.PositionType.TopRight);
            var deleteButton2 = new UIClickable("Delete2", "UI/button/RedBulb", new Coordinate(), ColorScale.White, charPanel2, TextureUtilities.PositionType.Center, TextureUtilities.PositionType.TopRight);
            var deleteButton3 = new UIClickable("Delete3", "UI/button/RedBulb", new Coordinate(), ColorScale.White, charPanel3, TextureUtilities.PositionType.Center, TextureUtilities.PositionType.TopRight);
            var deleteButton4 = new UIClickable("Delete4", "UI/button/RedBulb", new Coordinate(), ColorScale.White, charPanel4, TextureUtilities.PositionType.Center, TextureUtilities.PositionType.TopRight);
            var deleteButton5 = new UIClickable("Delete5", "UI/button/RedBulb", new Coordinate(), ColorScale.White, charPanel5, TextureUtilities.PositionType.Center, TextureUtilities.PositionType.TopRight);
            var deleteButton6 = new UIClickable("Delete6", "UI/button/RedBulb", new Coordinate(), ColorScale.White, charPanel6, TextureUtilities.PositionType.Center, TextureUtilities.PositionType.TopRight);
            var deleteButton7 = new UIClickable("Delete7", "UI/button/RedBulb", new Coordinate(), ColorScale.White, charPanel7, TextureUtilities.PositionType.Center, TextureUtilities.PositionType.TopRight);

            var deleteButton1Icon = new UIElement("Delete1Icon", "UI/icon/No", new Coordinate(), ColorScale.Ghost, deleteButton1, TextureUtilities.PositionType.Center, TextureUtilities.PositionType.Center);
            var deleteButton2Icon = new UIElement("Delete2Icon", "UI/icon/No", new Coordinate(), ColorScale.Ghost, deleteButton2, TextureUtilities.PositionType.Center, TextureUtilities.PositionType.Center);
            var deleteButton3Icon = new UIElement("Delete3Icon", "UI/icon/No", new Coordinate(), ColorScale.Ghost, deleteButton3, TextureUtilities.PositionType.Center, TextureUtilities.PositionType.Center);
            var deleteButton4Icon = new UIElement("Delete4Icon", "UI/icon/No", new Coordinate(), ColorScale.Ghost, deleteButton4, TextureUtilities.PositionType.Center, TextureUtilities.PositionType.Center);
            var deleteButton5Icon = new UIElement("Delete5Icon", "UI/icon/No", new Coordinate(), ColorScale.Ghost, deleteButton5, TextureUtilities.PositionType.Center, TextureUtilities.PositionType.Center);
            var deleteButton6Icon = new UIElement("Delete6Icon", "UI/icon/No", new Coordinate(), ColorScale.Ghost, deleteButton6, TextureUtilities.PositionType.Center, TextureUtilities.PositionType.Center);
            var deleteButton7Icon = new UIElement("Delete7Icon", "UI/icon/No", new Coordinate(), ColorScale.Ghost, deleteButton7, TextureUtilities.PositionType.Center, TextureUtilities.PositionType.Center);

            var charInfo1 = new UIElement("CharInfo1", "UI/field/LargeEntryBox", new Coordinate(), ColorScale.White, charPanel1, TextureUtilities.PositionType.BottomLeft, TextureUtilities.PositionType.TopLeft);
            var charInfo2 = new UIElement("CharInfo2", "UI/field/LargeEntryBox", new Coordinate(), ColorScale.White, charPanel2, TextureUtilities.PositionType.CenterBottom, TextureUtilities.PositionType.TopLeft);
            var charInfo3 = new UIElement("CharInfo3", "UI/field/LargeEntryBox", new Coordinate(), ColorScale.White, charPanel3, TextureUtilities.PositionType.CenterBottom, TextureUtilities.PositionType.TopLeft);
            var charInfo4 = new UIElement("CharInfo4", "UI/field/LargeEntryBox", new Coordinate(), ColorScale.White, charPanel4, TextureUtilities.PositionType.CenterBottom, TextureUtilities.PositionType.TopLeft);
            var charInfo5 = new UIElement("CharInfo5", "UI/field/LargeEntryBox", new Coordinate(), ColorScale.White, charPanel5, TextureUtilities.PositionType.CenterBottom, TextureUtilities.PositionType.TopLeft);
            var charInfo6 = new UIElement("CharInfo6", "UI/field/LargeEntryBox", new Coordinate(), ColorScale.White, charPanel6, TextureUtilities.PositionType.CenterBottom, TextureUtilities.PositionType.TopLeft);
            var charInfo7 = new UIElement("CharInfo7", "UI/field/LargeEntryBox", new Coordinate(), ColorScale.White, charPanel7, TextureUtilities.PositionType.BottomRight, TextureUtilities.PositionType.TopLeft);

            var charInfoText1 = new UIText("CharInfoText1", new Coordinate(14, 14), "Box1", ColorScale.Black, charInfo1, TextureUtilities.PositionType.TopLeft, TextureUtilities.PositionType.TopLeft);
            var charInfoText2 = new UIText("CharInfoText2", new Coordinate(14, 14), "Box2", ColorScale.Black, charInfo2, TextureUtilities.PositionType.TopLeft, TextureUtilities.PositionType.TopLeft);
            var charInfoText3 = new UIText("CharInfoText3", new Coordinate(14, 14), "Box3", ColorScale.Black, charInfo3, TextureUtilities.PositionType.TopLeft, TextureUtilities.PositionType.TopLeft);
            var charInfoText4 = new UIText("CharInfoText4", new Coordinate(14, 14), "Box4", ColorScale.Black, charInfo4, TextureUtilities.PositionType.TopLeft, TextureUtilities.PositionType.TopLeft);
            var charInfoText5 = new UIText("CharInfoText5", new Coordinate(14, 14), "Box5", ColorScale.Black, charInfo5, TextureUtilities.PositionType.TopLeft, TextureUtilities.PositionType.TopLeft);
            var charInfoText6 = new UIText("CharInfoText6", new Coordinate(14, 14), "Box6", ColorScale.Black, charInfo6, TextureUtilities.PositionType.TopLeft, TextureUtilities.PositionType.TopLeft);
            var charInfoText7 = new UIText("CharInfoText7", new Coordinate(14, 14), "Box7", ColorScale.Black, charInfo7, TextureUtilities.PositionType.TopLeft, TextureUtilities.PositionType.TopLeft);

            charPanel1.AddTooltip(new Coordinate(60, 60), new Vector2(0, -17), charInfo1, charInfoText1);
            charPanel2.AddTooltip(new Coordinate(60, 60), new Vector2(0, -17), charInfo2, charInfoText2);
            charPanel3.AddTooltip(new Coordinate(60, 60), new Vector2(0, -17), charInfo3, charInfoText3);
            charPanel4.AddTooltip(new Coordinate(60, 60), new Vector2(0, -17), charInfo4, charInfoText4);
            charPanel5.AddTooltip(new Coordinate(60, 60), new Vector2(0, -17), charInfo5, charInfoText5);
            charPanel6.AddTooltip(new Coordinate(60, 60), new Vector2(0, -17), charInfo6, charInfoText6);
            charPanel7.AddTooltip(new Coordinate(60, 60), new Vector2(0, -17), charInfo7, charInfoText7);

            backButton.RegisterCallback(CloseMenu);
            newCharButton.RegisterCallback(MakeNewChar);
            changeCharButton.RegisterCallback(ChangeChar);
            RegisterAll(WarnOnDelete, deleteButton1, deleteButton2, deleteButton3, deleteButton4, deleteButton5, deleteButton6, deleteButton7);
            RegisterAll(SelectChar, charPanel1, charPanel2, charPanel3, charPanel4, charPanel5, charPanel6, charPanel7);
            SetExtrasAll("UI/button/SmallButtonDown", "UI/button/SmallButtonOver", null, null, backButton, newCharButton, changeCharButton);
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
            _panelUI.AddElements(backdrop, currentCharacterSet, backButton, backButtonText, newCharButton, newCharButtonText, changeCharButton, changeCharButtonText, charArray, charDisplay);

            //New Character Panel
            var backdrop2 = new UIElement("Backdrop2", "UI/decor/Backdrop", new Vector2(0.5f, 0.5f), Color.White, _newCharUI, TextureUtilities.PositionType.Center);
            var cancelButton = new UIClickable("CancelButton", "UI/button/SmallButton", new Coordinate(10, -10), ColorScale.White, backdrop2, TextureUtilities.PositionType.BottomLeft, TextureUtilities.PositionType.BottomLeft);
            var cancelButtonText = new UIText("CancelButtonText", new Coordinate(), "Cancel", ColorScale.Black, cancelButton, TextureUtilities.PositionType.Center, TextureUtilities.PositionType.Center);
            var confirmButton = new UIClickable("ConfirmButton", "UI/button/SmallButton", new Coordinate(-10, -10), ColorScale.White, backdrop2, TextureUtilities.PositionType.BottomRight, TextureUtilities.PositionType.BottomRight);
            var confirmButtonText = new UIText("ConfirmButtonText", new Coordinate(), "Confirm", ColorScale.Black, confirmButton, TextureUtilities.PositionType.Center, TextureUtilities.PositionType.Center);
            var inputBox = new UITogglable("NewCharInputBox", "UI/field/SmallEntryBox", new Coordinate(50, 50), ColorScale.Ghost, backdrop2, TextureUtilities.PositionType.TopLeft, TextureUtilities.PositionType.TopLeft, false, true);
            var inputBoxText = new UIText("NewCharInputBoxText", new Coordinate(20, 20), "New Char", ColorScale.Black, inputBox, TextureUtilities.PositionType.TopLeft, TextureUtilities.PositionType.TopLeft);

            var charDesignPanel = new UIPanel("CharDesignPanel", new Coordinate(-50, 50), new Coordinate(200, 125), backdrop2, TextureUtilities.PositionType.TopRight, TextureUtilities.PositionType.TopRight);
            var charBodyDisplay = new UIDynamicElement("CharBodyDisplay", "null", new Coordinate(), ColorScale.White, charDesignPanel, TextureUtilities.PositionType.Center, TextureUtilities.PositionType.Center);
            var charClothDisplay = new UIDynamicElement("CharClothDisplay", "null", new Coordinate(), ColorScale.White, charDesignPanel, TextureUtilities.PositionType.Center, TextureUtilities.PositionType.Center);
            var charHairDisplay = new UIDynamicElement("CharHairDisplay", "null", new Coordinate(), ColorScale.White, charDesignPanel, TextureUtilities.PositionType.Center, TextureUtilities.PositionType.Center);

            var leftHair = new UIClickable("HairLeft", "UI/button/RedBulb", new Vector2(), ColorScale.White, charDesignPanel, TextureUtilities.PositionType.Center);
            var leftBody = new UIClickable("BodyLeft", "UI/button/GreenBulb", new Vector2(0, 0.33f), ColorScale.White, charDesignPanel, TextureUtilities.PositionType.Center);
            var leftCloth = new UIClickable("ClothLeft", "UI/button/BlueBulb", new Vector2(0, 0.67f), ColorScale.White, charDesignPanel, TextureUtilities.PositionType.Center);
            var leftColor = new UIClickable("ColorLeft", "UI/button/BlackBulb", new Vector2(0, 1), ColorScale.White, charDesignPanel, TextureUtilities.PositionType.Center);

            var rightHair = new UIClickable("HairRight", "UI/button/RedBulb", new Vector2(1, 0), ColorScale.White, charDesignPanel, TextureUtilities.PositionType.Center);
            var rightBody = new UIClickable("BodyRight", "UI/button/GreenBulb", new Vector2(1, 0.33f), ColorScale.White, charDesignPanel, TextureUtilities.PositionType.Center);
            var rightCloth = new UIClickable("ClothRight", "UI/button/BlueBulb", new Vector2(1, 0.67f), ColorScale.White, charDesignPanel, TextureUtilities.PositionType.Center);
            var rightColor = new UIClickable("ColorRight", "UI/button/BlackBulb", new Vector2(1, 1), ColorScale.White, charDesignPanel, TextureUtilities.PositionType.Center);

            var leftArrow1 = new UIElement("Left1", "UI/icon/Left", new Coordinate(), ColorScale.Ghost, leftHair, TextureUtilities.PositionType.Center, TextureUtilities.PositionType.Center);
            var leftArrow2 = new UIElement("Left2", "UI/icon/Left", new Coordinate(), ColorScale.Ghost, leftBody, TextureUtilities.PositionType.Center, TextureUtilities.PositionType.Center);
            var leftArrow3 = new UIElement("Left3", "UI/icon/Left", new Coordinate(), ColorScale.Ghost, leftCloth, TextureUtilities.PositionType.Center, TextureUtilities.PositionType.Center);
            var leftArrow4 = new UIElement("Left4", "UI/icon/Left", new Coordinate(), ColorScale.Ghost, leftColor, TextureUtilities.PositionType.Center, TextureUtilities.PositionType.Center);
            
            var rightArrow1 = new UIElement("Right1", "UI/icon/Right", new Coordinate(), ColorScale.Ghost, rightHair, TextureUtilities.PositionType.Center, TextureUtilities.PositionType.Center);
            var rightArrow2 = new UIElement("Right2", "UI/icon/Right", new Coordinate(), ColorScale.Ghost, rightBody, TextureUtilities.PositionType.Center, TextureUtilities.PositionType.Center);
            var rightArrow3 = new UIElement("Right3", "UI/icon/Right", new Coordinate(), ColorScale.Ghost, rightCloth, TextureUtilities.PositionType.Center, TextureUtilities.PositionType.Center);
            var rightArrow4 = new UIElement("Right4", "UI/icon/Right", new Coordinate(), ColorScale.Ghost, rightColor, TextureUtilities.PositionType.Center, TextureUtilities.PositionType.Center);

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
            (_panelUI.GetElement("ChangeCharButton") as UIClickable)?.Disable(ColorScale.Grey);
            (_panelUI.GetElement("CharacterDisplay") as UIDynamicElement)?.ChangeTexture(_characters[_selectedChar].Entity.GetTexture(EntityTexture.State.DownMoving));
        }

        protected void ResetNewChar(GraphicsDevice graphics)
        {
            _body = _hair = _cloth = _color = 0;
            var sheet = TextureLibrary.Lookup("Char/base/" + (_body + 1));
            var rect = new Rectangle(0, sheet.Height / 2, sheet.Width, sheet.Height / 4);
            var body = new AnimatableTexture(TextureUtilities.GetSubTexture(graphics, sheet, rect), 4, 4);
            var cloth = new AnimatableTexture(TextureUtilities.GetSubTexture(graphics, TextureLibrary.Lookup("Char/cloth/" + (_cloth + 1)), rect), 4, 4);
            var hair = new AnimatableTexture(TextureUtilities.GetSubTexture(graphics, TextureLibrary.Lookup("Char/hair/" + (_hair + 1) + "-" + (_color + 1)), rect), 4, 4);
            (_newCharUI.GetElement("CharBodyDisplay") as UIDynamicElement)?.ChangeTexture(body);
            (_newCharUI.GetElement("CharClothDisplay") as UIDynamicElement)?.ChangeTexture(cloth);
            (_newCharUI.GetElement("CharHairDisplay") as UIDynamicElement)?.ChangeTexture(hair);
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

        public override void OnMoveMouse(Point point)
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
                    _warningMessage.OnMoveMouse(point);
                    break;
            }
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
            game.SavedCharacters.Clear();
            foreach (var c in _characters)
            {
                game.SavedCharacters.Add(c.GetPacked());
            }
        }

        public override void Unpack(PackedGame game)
        {
            //take characters
            _characters.Clear();
            foreach (var c in game.SavedCharacters)
            {
                var player = new PlayerEntityContainer(Container.GraphicsDevice, c.Name, c.TextureSet[0], c.TextureSet[1], c.TextureSet[2], c.TextureSet[3], c.Inventory);
                _characters.Add(player);
                (_panelUI.GetElement("Portrait" + _characters.Count) as UIDynamicElement)?.ChangeTexture(player.Entity.GetTexture(EntityTexture.State.Down));
            }
            for (int i = game.SavedCharacters.Count; i < MAX_CHARACTERS; i++)
            {
                (_panelUI.GetElement("Portrait" + (i + 1)) as UIDynamicElement)?.ChangeTexture(TextureLibrary.Lookup("null"));
            }
            CheckDeleteCharButtons();
            _selectedChar = _currentChar = game.CurrentPlayer;
            (_panelUI.GetElement("CharacterDisplay") as UIDynamicElement)?.ChangeTexture(_characters[_selectedChar].Entity.GetTexture(EntityTexture.State.DownMoving));
        }

        public PlayerEntityContainer GetCurrentChar()
        {
            return _characters.Count > 0 ? _characters[_currentChar] : null;
        }
    }
}
