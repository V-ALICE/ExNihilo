using System;
using System.Diagnostics.Eventing.Reader;
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
            SetRulesAll(TextureLibrary.MediumScaleRuleSet, exitButton, exitButtonX);
            exitButton.SetExtraStates("UI/button/RedBulbDown", "UI/button/RedBulbOver");

            var hostButton = new UIClickable("HostButton", "UI/button/SmallButton", new Coordinate(-20, -20), ColorScale.White, backdrop, PositionType.BottomRight, PositionType.BottomRight);
            var hostText = new UIText("HostText", new Coordinate(), "Host Game", ColorScale.Black, hostButton, PositionType.Center, PositionType.Center);
            var clientButton = new UIClickable("ClientButton", "UI/button/SmallButton", new Coordinate(0, -20), ColorScale.White, hostButton, PositionType.BottomLeft, PositionType.TopLeft);
            var clientText = new UIText("ClientText", new Coordinate(), "Connect", ColorScale.Black, clientButton, PositionType.Center, PositionType.Center);
            var ipBox = new UITogglable("IPBox", "UI/field/SmallEntryBox", new Coordinate(-20, 0), ColorScale.White, clientButton, PositionType.CenterRight, PositionType.CenterLeft, false, true);
            var ipText = new UIText("IPText", new Coordinate(19, 20), "", new[] {ColorScale.Black, ColorScale.GetFromGlobal("__unblinker")}, ipBox, PositionType.TopLeft, PositionType.TopLeft);
            var disconnectButton = new UIClickable("DisconnectButton", "UI/button/SmallButton", new Coordinate(0, 18), ColorScale.White, ipBox, PositionType.CenterTop, PositionType.CenterBottom);
            var disconnectText = new UIText("DisconnectText", new Coordinate(), "Disconnect", ColorScale.Black, disconnectButton, PositionType.Center, PositionType.Center);

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

            _panelUI.AddElements(backdrop, exitButton, exitButtonX, ipBox, ipText, clientButton, clientText, hostButton, hostText, disconnectButton, disconnectText);
            _note = new NoteMenu(container, "There's a familiar island off in\nthe distance. Call out?", NoteAction);
        }

        public void UpdateDisplay(bool ending)
        {
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
