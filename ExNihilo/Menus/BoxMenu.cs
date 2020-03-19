
using System;
using System.Linq;
using ExNihilo.Entity;
using ExNihilo.Systems.Backend;
using ExNihilo.Systems.Backend.Network;
using ExNihilo.Systems.Bases;
using ExNihilo.Systems.Game;
using ExNihilo.Systems.Game.Items;
using ExNihilo.UI;
using ExNihilo.Util;
using ExNihilo.Util.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Position = ExNihilo.Util.Graphics.TextureUtilities.PositionType;

namespace ExNihilo.Menus
{
    public class BoxMenu : Menu
    {
        public static BoxMenu Menu { get; private set; }
        public static void CreateMenu(GameContainer container)
        {
            Menu = new BoxMenu(container);
        }

        private void MoveItem(UICallbackPackage package)
        {
            //Change slots in inventory object and update this menu display accordingly
            var startSlotType = new string(package.Caller.TakeWhile(char.IsLetter).ToArray());
            var startSlotBox = startSlotType == "Box";
            var startSlotNum = int.Parse(package.Caller.Substring(startSlotType.Length));
            int endSlotNum = -1;
            bool endSlotBox = false;

            for (int i = 0; i < _container.Length; i++)
            {
                var rect = new Rectangle(_container[i].OriginPosition.X, _container[i].OriginPosition.Y, _iconRefSize, _iconRefSize);
                if (rect.Contains(package.ScreenPos))
                {
                    endSlotNum = i;
                    endSlotBox = true;
                    break;
                }
            }
            if (!endSlotBox)
            {
                for (int i = 0; i < _inventory.Length; i++)
                {
                    var rect = new Rectangle(_inventory[i].OriginPosition.X, _inventory[i].OriginPosition.Y, _iconRefSize, _iconRefSize);
                    if (rect.Contains(package.ScreenPos))
                    {
                        endSlotNum = i;
                        break;
                    }
                }
            }

            if (endSlotNum == -1)
            {
                var trash = _panelUI.GetElement("Trash");
                var rect = new Rectangle(trash.OriginPosition.X, trash.OriginPosition.Y, trash.CurrentPixelSize.X, trash.CurrentPixelSize.Y);
                if (rect.Contains(package.ScreenPos))
                {
                    //Throw item away
                    if (startSlotBox)
                    {
                        _playerRef.Inventory.RemoveItem(_boxRef.Contents[startSlotNum]);
                        _boxRef.Contents[startSlotNum] = null;
                    }
                    else _playerRef.Inventory.RemoveItem(startSlotNum, false, true);
                    UpdateDisplay();
                }
            }
            else if (!startSlotBox && !endSlotBox && startSlotNum != endSlotNum)
            {
                //Swapping items in inventory
                if (_playerRef.Inventory.TrySwapItem(startSlotNum, endSlotNum, false, false)) UpdateDisplay();
            }
            else if (startSlotBox && endSlotBox && startSlotNum != endSlotNum)
            {
                //Swapping items in box
                var item = _boxRef.Contents[endSlotNum];
                _boxRef.Contents[endSlotNum] = _boxRef.Contents[startSlotNum];
                _boxRef.Contents[startSlotNum] = item;
                UpdateDisplay();
            }
            else if (startSlotBox && !endSlotBox)
            {
                //Swap item into inventory
                NetworkManager.SendMessage(new RemoveItem(NetworkManager.MyUniqueID, _boxRefIndex, _boxRef.Contents[startSlotNum].UID));
                _boxRef.Contents[startSlotNum] = _playerRef.Inventory.TryAddItem(_boxRef.Contents[startSlotNum], endSlotNum);
                UpdateDisplay();
            }
            else if (!startSlotBox && endSlotBox)
            {
                //Swap item into box
                _boxRef.Contents[endSlotNum] = _playerRef.Inventory.TryAddItem(_boxRef.Contents[endSlotNum], startSlotNum);
                UpdateDisplay();
            }
        }
        private void CloseMenu(UICallbackPackage package)
        {
            OnExit?.Invoke();
        }

        private BoxInteractive _boxRef;
        private PlayerEntityContainer _playerRef;

        private readonly UIPanel _panelUI;
        private readonly UIElement[] _container = new UIElement[BoxInteractive.ContainerSize];
        private readonly UIElement[] _inventory = new UIElement[Inventory.InventorySize];
        private readonly UIText _descText;
        private Point _lastMousePosition;
        private bool _mouseDown;
        private int _lastTextSlot = -1, _iconRefSize, _boxRefIndex;
        private const int _descCharLen = 30;

        private BoxMenu(GameContainer container) : base(container, null)
        {
            _panelUI = new UIPanel("this.MenuKing", new Vector2(0.5f, 0.5f), Vector2.One, Position.Center);

            var backdrop = new UIElement("Backdrop", "UI/decor/Backdrop", new Vector2(0.5f, 0.5f), Color.White, _panelUI, Position.Center);
            var exitButton = new UIClickable("ExitButton", "UI/button/RedBulb", new Coordinate(-8, 8), ColorScale.White, backdrop, Position.Center, Position.TopRight);
            var exitButtonX = new UIElement("ExitButtonX", "UI/icon/No", new Coordinate(), ColorScale.White, exitButton, Position.Center, Position.Center);
            exitButton.RegisterCallback(CloseMenu);
            SetRulesAll(TextureLibrary.x1d25ScaleRuleSet, exitButton, exitButtonX);
            exitButton.SetExtraStates("UI/button/RedBulbDown", "UI/button/RedBulbOver");

            var inventorySet = new UIElement("InventorySet", "UI/field/ThreeRowElementSet", new Coordinate(0, -39), ColorScale.White, backdrop, Position.CenterBottom, Position.CenterBottom);
            var textBox = new UIElement("TextBox", "UI/field/LargeEntryBox", new Coordinate(-20, -46), ColorScale.White, inventorySet, Position.BottomRight, Position.TopRight);
            var containerSet = new UIElement("ContainerSet", "UI/field/SixElementSet", new Coordinate(20, -46), ColorScale.White, inventorySet, Position.BottomLeft, Position.TopLeft);
            _descText = new UIText("DescriptionBox", new Coordinate(14, 14), "", _descCharLen, new ColorScale[0], textBox, Position.TopLeft, Position.TopLeft);

            var containerRef = new UIPanel("ContainerZone", new Coordinate(), new Coordinate(664, 80), containerSet, Position.TopLeft, Position.TopLeft);
            var inventoryRef = new UIPanel("InventoryZone", new Coordinate(), new Coordinate(584, 224), inventorySet, Position.TopLeft, Position.TopLeft);

            var trash = new UIClickable("Trash", "Icon/action/toss", new Coordinate(512, 152), ColorScale.Grey, inventoryRef, Position.TopLeft, Position.TopLeft);
            trash.SetRules(TextureLibrary.HalfScaleRuleSet);
            trash.SetExtraStates("", "", Color.DarkRed, Color.DarkRed);

            for (int i = 0; i < _container.Length; i++)
            {
                var row = i / 3;
                var col = i % 3;
                _container[i] = new UIMovable("Box" + i, "null", new Coordinate(8 + col * 72, 8 + row * 72), ColorScale.White, containerRef, Vector2.Zero, Vector2.One, Position.TopLeft, Position.TopLeft, true, false);
            }
            for (int i = 0; i < _inventory.Length; i++)
            {
                var row = i / 8;
                var col = i % 8;
                _inventory[i] = new UIMovable("Inv" + i, "null", new Coordinate(8 + col * 72, 8 + row * 72), ColorScale.White, inventoryRef, Vector2.Zero, Vector2.One, Position.TopLeft, Position.TopLeft, true, false);
            }

            containerRef.AddElements(_container);
            SetRulesAll(TextureLibrary.HalfScaleRuleSet, _container);
            RegisterAll(MoveItem, _container);

            inventoryRef.AddElements(trash);
            inventoryRef.AddElements(_inventory);
            SetRulesAll(TextureLibrary.HalfScaleRuleSet, _inventory);
            RegisterAll(MoveItem, _inventory);

            _panelUI.AddElements(backdrop, textBox, _descText, containerSet, inventorySet, containerRef, inventoryRef, exitButton, exitButtonX);
        }

        public void SetReference(PlayerEntityContainer reference)
        {
            //This gets called initially and when characters change
            _playerRef = reference;
        }

        private void UpdateDisplay()
        {
            for (int i = 0; i < _boxRef.Contents.Length; i++)
            {
                var element = _container[i] as UIMovable;
                if (_boxRef.Contents[i] is null)
                {
                    element?.SetNullTexture();
                }
                else
                {
                    element?.ChangeTexture(_boxRef.Contents[i].GetTexture());
                    element?.ChangeColor(_boxRef.Contents[i].GetIconColor());
                }
            }
            for (int i = 0; i < _playerRef.Inventory.Items.Length; i++)
            {
                var element = _inventory[i] as UIMovable;
                if (_playerRef.Inventory.Items[i] is null)
                {
                    element?.SetNullTexture();
                }
                else
                {
                    element?.ChangeTexture(_playerRef.Inventory.Items[i].GetTexture());
                    element?.ChangeColor(_playerRef.Inventory.Items[i].GetIconColor());
                }
            }

            if (_playerRef.Inventory.CanRestoreTrashedItem()) (_panelUI.GetElement("UndoButton") as UIClickable)?.Enable();
            else (_panelUI.GetElement("UndoButton") as UIClickable)?.Disable(ColorScale.Ghost);

            _lastTextSlot = -1;
        }
        public void Enter(Point point, BoxInteractive refBox, int boxIndex, Action onExit)
        {
            base.Enter(point);
            OnExit = onExit;
            _boxRef = refBox;
            _boxRefIndex = boxIndex;
            UpdateDisplay();
            _lastMousePosition = new Point(-1, -1);
            OnMoveMouse(point);
            _lastTextSlot = -1;
        }

        public override void LoadContent(GraphicsDevice graphics, ContentManager content)
        {
            _panelUI.LoadContent(graphics, content);
            _iconRefSize = _panelUI.GetElement("Trash").CurrentPixelSize.X;
        }

        public override void OnResize(GraphicsDevice graphics, Coordinate gameWindow)
        {
            _panelUI.OnResize(graphics, gameWindow);
            _iconRefSize = _panelUI.GetElement("Trash").CurrentPixelSize.X;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (_boxRef.Dirty)
            {
                UpdateDisplay();
                _boxRef.Clean();
            }
            _panelUI.Draw(spriteBatch);
            _panelUI.DrawFinal(spriteBatch);
        }

        private void CheckForHighlight(Point point)
        {
            if (_mouseDown) return;

            if (point.Y < _inventory[0].OriginPosition.Y)
            {
                for (int i = 0; i < _container.Length; i++)
                {
                    var rect = new Rectangle(_container[i].OriginPosition.X, _container[i].OriginPosition.Y, _container[i].CurrentPixelSize.X, _container[i].CurrentPixelSize.Y);
                    if (rect.Contains(point))
                    {
                        var item = _boxRef.Contents[i];
                        if (_lastTextSlot == i) return;
                        if (item is null) break;

                        _descText.SetText(item.GetSmartDesc(), _descCharLen, item.GetSmartColors(ColorScale.Black));
                        _lastTextSlot = i;
                        return;
                    }
                }
            }
            else
            {
                for (int i = 0; i < _inventory.Length; i++)
                {
                    var rect = new Rectangle(_inventory[i].OriginPosition.X, _inventory[i].OriginPosition.Y, _inventory[i].CurrentPixelSize.X, _inventory[i].CurrentPixelSize.Y);
                    if (rect.Contains(point))
                    {
                        var item = _playerRef.Inventory.Items[i];
                        if (_lastTextSlot == 7 + i) return;
                        if (item is null) break;

                        if (item is EquipInstance e)
                        {
                            var other = _playerRef.Inventory.Equipment[(int)e.Type];
                            _descText.SetText(e.GetSmartDesc(other), _descCharLen, e.GetSmartColors(ColorScale.Black));
                        }
                        else
                        {
                            _descText.SetText(item.GetSmartDesc(), _descCharLen, item.GetSmartColors(ColorScale.Black));
                        }

                        _lastTextSlot = 7 + i;
                        return;
                    }
                }
            }

            _descText.SetText("", _descCharLen, ColorScale.Black);
            _lastTextSlot = -1;
        }
        public override bool OnMoveMouse(Point point)
        {
            if (point == _lastMousePosition) return false;
            _lastMousePosition = point;
            _panelUI.OnMoveMouse(point);
            CheckForHighlight(point);
            return false;
        }

        public override bool OnLeftClick(Point point)
        {
            _mouseDown = true;
            return _panelUI.OnLeftClick(point);
        }

        public override void OnLeftRelease(Point point)
        {
            _mouseDown = false;
            _panelUI.OnLeftRelease(point);
        }

        public override void ReceiveInput(string input)
        {
        }
    }
}
