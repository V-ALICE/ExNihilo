using System;
using System.Diagnostics.Eventing.Reader;
using ExNihilo.Entity;
using ExNihilo.Systems.Backend;
using ExNihilo.Systems.Backend.Network;
using ExNihilo.UI;
using ExNihilo.Util;
using ExNihilo.Util.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

using PositionType = ExNihilo.Util.Graphics.TextureUtilities.PositionType;

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

        private void DoHost(UICallbackPackage package)
        {
            (_panelUI.GetElement("ClientButton") as UIClickable)?.Disable(ColorScale.Grey);
            (_panelUI.GetElement("HostButton") as UIClickable)?.Disable(ColorScale.Grey);
            (_panelUI.GetElement("IPBox") as UITogglable)?.Disable(ColorScale.Grey);
            if (_panelUI.GetElement("DisconnectButton") is UIClickable button)
            {
                button.Enable();
                button.AllowDraw(true);
                _panelUI.GetElement("DisconnectText").AllowDraw(true);
            }
            Container.StartNewHost();
        }

        private void DoClient(UICallbackPackage package)
        {
            (_panelUI.GetElement("ClientButton") as UIClickable)?.Disable(ColorScale.Grey);
            (_panelUI.GetElement("HostButton") as UIClickable)?.Disable(ColorScale.Grey);
            (_panelUI.GetElement("IPBox") as UITogglable)?.Disable(ColorScale.Grey);
            if (_panelUI.GetElement("DisconnectButton") is UIClickable button)
            {
                button.Enable();
                button.AllowDraw(true);
                _panelUI.GetElement("DisconnectText").AllowDraw(true);
            }
            Container.StartNewClient(_ipInput);
        }

        private void Disconnect(UICallbackPackage package)
        {
            NetworkManager.CloseConnections();
            //UpdateDisplay(true);
        }

        private void StartEnteringIp(UICallbackPackage package)
        {
            _enteringIp = package.Value[0] > 0;
            if (_enteringIp && TypingKeyboard.Lock(this))
            {
                if (_panelUI.GetElement("IPText") is UIText text)
                {
                    if (_ipInput.Length < 15) text.SetText(_ipInput + "@c1|");
                    else text.SetText(_ipInput);
                }
            }
            else
            {
                (_panelUI.GetElement("IPText") as UIText)?.SetText(_ipInput);
                _enteringIp = false;
                TypingKeyboard.Unlock(this);
            }
        }

/********************************************************************
------->Menu Functions
********************************************************************/
        private readonly UIPanel _panelUI;
        private readonly NoteMenu _note;
        private Texture2D _defaultCharacterModel;
        private Point _lastMousePosition;
        private bool _showingNote, _enteringIp;
        private string _ipInput="";

        public MultiplayerMenu(GameContainer container, Action onExit) : base(container, onExit)
        {
            _panelUI = new UIPanel("this.MenuKing", new Vector2(0.5f, 0.5f), Vector2.One, PositionType.Center);

            var backdrop = new UIElement("Backdrop", "UI/decor/Backdrop", new Vector2(0.5f, 0.5f), Color.White, _panelUI, PositionType.Center);
            var exitButton = new UIClickable("ExitButton", "UI/button/RedBulb", new Coordinate(-8, 8), ColorScale.White, backdrop, PositionType.Center, PositionType.TopRight);
            var exitButtonX = new UIElement("ExitButtonX", "UI/icon/No", new Coordinate(), ColorScale.White, exitButton, PositionType.Center, PositionType.Center);
            exitButton.RegisterCallback(CloseMenu);
            SetRulesAll(TextureLibrary.x1d25ScaleRuleSet, exitButton, exitButtonX);
            exitButton.SetExtraStates("UI/button/RedBulbDown", "UI/button/RedBulbOver");

            var hostButton = new UIClickable("HostButton", "UI/button/SmallButton", new Coordinate(-20, -20), ColorScale.White, backdrop, PositionType.BottomRight, PositionType.BottomRight);
            var hostText = new UIText("HostText", new Coordinate(), "Host Game", ColorScale.Black, hostButton, PositionType.Center, PositionType.Center);
            var clientButton = new UIClickable("ClientButton", "UI/button/SmallButton", new Coordinate(0, -20), ColorScale.White, hostButton, PositionType.BottomLeft, PositionType.TopLeft);
            var clientText = new UIText("ClientText", new Coordinate(), "Connect", ColorScale.Black, clientButton, PositionType.Center, PositionType.Center);
            var ipBox = new UITogglable("IPBox", "UI/field/SmallEntryBox", new Coordinate(-20, 0), ColorScale.White, clientButton, PositionType.CenterRight, PositionType.CenterLeft, false, true);
            var ipText = new UIText("IPText", new Coordinate(19, 20), "", new[] {ColorScale.Black, ColorScale.GetFromGlobal("__unblinker")}, ipBox, PositionType.TopLeft, PositionType.TopLeft);
            var ipLabel = new UIText("IPLabel", new Coordinate(12, -2), "IP Address", ColorScale.Black, ipBox, PositionType.BottomLeft, PositionType.TopLeft);
            var disconnectButton = new UIClickable("DisconnectButton", "UI/button/SmallButton", new Coordinate(0, 18), ColorScale.White, ipBox, PositionType.CenterTop, PositionType.CenterBottom);
            var disconnectText = new UIText("DisconnectText", new Coordinate(), "Disconnect", ColorScale.Black, disconnectButton, PositionType.Center, PositionType.Center);
            var charPortrait2 = new UIElement("CharPortrait2", "null", new Coordinate(0, 30), ColorScale.White, backdrop, PositionType.CenterTop, PositionType.CenterTop);
            var charPortrait1 = new UIElement("CharPortrait1", "null", new Coordinate(-20, 15), ColorScale.White, charPortrait2, PositionType.TopRight, PositionType.TopLeft);
            var charPortrait3 = new UIElement("CharPortrait3", "null", new Coordinate(20, 15), ColorScale.White, charPortrait2, PositionType.TopLeft, PositionType.TopRight);
            var charName1 = new UIText("CharName1", new Coordinate(), "Open Slot", ColorScale.Black, charPortrait1, PositionType.CenterTop, PositionType.CenterBottom);
            var charName2 = new UIText("CharName2", new Coordinate(), "Open Slot", ColorScale.Black, charPortrait2, PositionType.CenterTop, PositionType.CenterBottom);
            var charName3 = new UIText("CharName3", new Coordinate(), "Open Slot", ColorScale.Black, charPortrait3, PositionType.CenterTop, PositionType.CenterBottom);

            clientButton.Disable(ColorScale.Grey);
            disconnectButton.Disable(ColorScale.Grey);
            disconnectButton.AllowDraw(false);
            disconnectText.AllowDraw(false);
            hostButton.RegisterCallback(DoHost);
            clientButton.RegisterCallback(DoClient);
            disconnectButton.RegisterCallback(Disconnect);
            ipBox.RegisterCallback(StartEnteringIp);
            ipBox.SetExtraStates("", "", ColorScale.White);
            ipText.SetRules(TextureLibrary.DoubleScaleRuleSet);
            SetExtrasAll("UI/button/SmallButtonDown", "UI/button/SmallButtonOver", null, null, clientButton, hostButton, disconnectButton);
            SetRulesAll(TextureLibrary.QuadScaleRuleSet, charPortrait1, charPortrait2, charPortrait3);

            _panelUI.AddElements(backdrop, exitButton, exitButtonX, ipBox, ipText, ipLabel, clientButton, clientText, hostButton, hostText, disconnectButton, disconnectText, charPortrait1, charPortrait2, charPortrait3, charName1, charName2, charName3);
            _note = new NoteMenu(container, "The void teems with adventurers.\nWould you like to set sail?", NoteAction);
        }

        public void UpdateDisplay(bool ending)
        {
            for (int i = 0; i < 3; i++)
            {
                var item = _panelUI.GetElement("CharPortrait" + (i + 1));
                var text = _panelUI.GetElement("CharName" + (i + 1)) as UIText;
                if (Container.OtherPlayers.Count > i)
                {
                    item?.ChangeTexture(Container.OtherPlayers[i].GetStateTexture(EntityTexture.State.Down));
                    text?.SetText(Container.OtherPlayers[i].Name);
                }
                else
                {
                    item?.ChangeTexture(_defaultCharacterModel);
                    text?.SetText("Open Slot");
                }
            }
            if (NetworkManager.Active && !ending)
            {
                (_panelUI.GetElement("ClientButton") as UIClickable)?.Disable(ColorScale.Grey);
                (_panelUI.GetElement("HostButton") as UIClickable)?.Disable(ColorScale.Grey);
                (_panelUI.GetElement("IPBox") as UITogglable)?.Disable(ColorScale.Grey);
                if (_panelUI.GetElement("DisconnectButton") is UIClickable button)
                {
                    button.Enable();
                    button.AllowDraw(true);
                    _panelUI.GetElement("DisconnectText").AllowDraw(true);
                }
            }
            else
            {
                if (LegalIP(_ipInput)) (_panelUI.GetElement("ClientButton") as UIClickable)?.Enable();
                else (_panelUI.GetElement("ClientButton") as UIClickable)?.Disable(ColorScale.Grey);
                (_panelUI.GetElement("IPBox") as UITogglable)?.Enable();
                (_panelUI.GetElement("HostButton") as UIClickable)?.Enable();
                if (_panelUI.GetElement("DisconnectButton") is UIClickable button)
                {
                    button.Disable(ColorScale.Grey);
                    button.AllowDraw(false);
                    _panelUI.GetElement("DisconnectText").AllowDraw(false);
                }
            }
            (_panelUI.GetElement("IPText") as UIText)?.SetText(_ipInput);
        }

        public override void Enter(Point point)
        {
            _lastMousePosition = point;
            _showingNote = !NetworkManager.Active;
            _enteringIp = false;
            _ipInput = "";
            _note.Enter(point);
            UpdateDisplay(false);
        }

        public override void BackOut()
        {
            if (_enteringIp)
            {
                (_panelUI.GetElement("IPBox") as UITogglable)?.ForcePush(false);
            }
            else base.BackOut();
        }

        public override void LoadContent(GraphicsDevice graphics, ContentManager content)
        {
            _panelUI.LoadContent(graphics, content);
            _note.LoadContent(graphics, content);
            var tex = new EntityTexture(graphics, TextureUtilities.GetPlayerTexture(graphics, new[] {0, 0, 0, 0}), 1);
            _defaultCharacterModel = TextureUtilities.GetSilhouette(graphics, tex.GetTexture(EntityTexture.State.Down), Color.Black);
            _panelUI.GetElement("CharPortrait1")?.ChangeTexture(_defaultCharacterModel);
            _panelUI.GetElement("CharPortrait2")?.ChangeTexture(_defaultCharacterModel);
            _panelUI.GetElement("CharPortrait3")?.ChangeTexture(_defaultCharacterModel);
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

        private static bool LegalIPInput(string ip)
        {
            var dotCount = 0;
            var curNum = "";
            foreach (var c in ip)
            {
                if (char.IsDigit(c))
                {
                    curNum += c;
                    if (curNum.Length > 3) return false;
                    if (int.TryParse(curNum, out var num) && num > 255) return false;
                }
                else if (c == '.')
                {
                    dotCount++;
                    if (dotCount > 3) return false;
                    if (curNum.Length == 0) return false;
                    curNum = "";
                }
                else return false;
            }

            return true;
        }

        private static bool LegalIP(string ip)
        {
            var numCount = 0;
            var dotCount = 0;
            var curNum = "";
            foreach (var c in ip)
            {
                if (char.IsDigit(c))
                {
                    if (curNum.Length == 0) numCount++;
                    curNum += c;
                }
                else if (c == '.')
                {
                    curNum = "";
                    dotCount++;
                }
            }

            return numCount == 4 && dotCount == 3;
        }

        public override void ReceiveInput(string input)
        {
            if (_enteringIp && input.Length > 0)
            {
                foreach (var c in input)
                {
                    if (LegalIPInput(_ipInput + c)) _ipInput += c;
                }
                if (_panelUI.GetElement("IPText") is UIText text)
                {
                    if (LegalIP(_ipInput))
                    {
                        text.ChangeColor(new[] { ColorScale.Black, ColorScale.GetFromGlobal("__unblinker") });
                        (_panelUI.GetElement("ClientButton") as UIClickable)?.Enable();
                    }
                    else
                    {
                        text.ChangeColor(new[] { (ColorScale)Color.DarkRed, ColorScale.GetFromGlobal("__unblinker") });
                        (_panelUI.GetElement("ClientButton") as UIClickable)?.Disable(ColorScale.Grey);
                    }
                    if (_ipInput.Length < 15) text.SetText(_ipInput+ "@c1|");
                    else text.SetText(_ipInput);
                }
            }
        }

        public override void Backspace(int len)
        {
            if (_enteringIp && len > 0)
            {
                if (_ipInput.Length <= len) _ipInput = "";
                else _ipInput = _ipInput.Substring(0, _ipInput.Length - len);
                if (_panelUI.GetElement("IPText") is UIText text)
                {
                    if (LegalIP(_ipInput))
                    {
                        text.ChangeColor(new[] { ColorScale.Black, ColorScale.GetFromGlobal("__unblinker") });
                        (_panelUI.GetElement("ClientButton") as UIClickable)?.Enable();
                    }
                    else
                    {
                        text.ChangeColor(new[] { (ColorScale)Color.DarkRed, ColorScale.GetFromGlobal("__unblinker") });
                        (_panelUI.GetElement("ClientButton") as UIClickable)?.Disable(ColorScale.Grey);
                    }
                    if (_ipInput.Length < 15) text.SetText(_ipInput + "@c1|");
                    else text.SetText(_ipInput);
                }
            }
        }

    }
}
