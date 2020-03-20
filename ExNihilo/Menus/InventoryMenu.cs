using System;
using System.Linq;
using ExNihilo.Entity;
using ExNihilo.Systems.Backend;
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
    public class InventoryMenu : Menu
    {
        private void UndoDelete(UICallbackPackage package)
        {
            _playerRef.Inventory.TryRestoreTrashedItem();
            UpdateDisplay();
        }
        private void MoveItem(UICallbackPackage package)
        {
            //Change slots in inventory object and update this menu display accordingly
            var startSlotType = new string(package.Caller.TakeWhile(char.IsLetter).ToArray());
            var startSlotEquip = startSlotType == "Equip";
            var startSlotNum = int.Parse(package.Caller.Substring(startSlotType.Length));
            int endSlotNum=-1;
            bool endSlotEquip = false;

            for (int i = 0; i < _equips.Length; i++)
            {
                var rect = new Rectangle(_equips[i].OriginPosition.X, _equips[i].OriginPosition.Y, _iconRefSize, _iconRefSize);
                if (rect.Contains(package.ScreenPos))
                {
                    endSlotNum = i;
                    endSlotEquip = true;
                    break;
                }
            }
            if (!endSlotEquip)
            {
                for (int i = 0; i < _items.Length; i++)
                {
                    var rect = new Rectangle(_items[i].OriginPosition.X, _items[i].OriginPosition.Y, _iconRefSize, _iconRefSize);
                    if (rect.Contains(package.ScreenPos))
                    {
                        endSlotNum = i;
                        break;
                    }
                }
            }

            if (endSlotNum == -1)
            {
                var _trash = _panelUI.GetElement("Trash");
                var rect = new Rectangle(_trash.OriginPosition.X, _trash.OriginPosition.Y, _trash.CurrentPixelSize.X, _trash.CurrentPixelSize.Y);
                if (rect.Contains(package.ScreenPos))
                {
                    //Throw item away
                    _playerRef.Inventory.RemoveItem(startSlotNum, startSlotEquip, true);
                    UpdateDisplay();
                }

                rect = new Rectangle(_portrait.OriginPosition.X, _portrait.OriginPosition.Y, _portrait.CurrentPixelSize.X, _portrait.CurrentPixelSize.Y);
                if (rect.Contains(package.ScreenPos) && _playerRef.Inventory.Items[startSlotNum] is UseInstance u)
                {
                    //Apply potion item
                    u.Activate(_playerRef.Inventory, null);
                    _playerRef.Inventory.RemoveItem(startSlotNum, startSlotEquip);
                    UpdateDisplay();
                }
            }
            else if (_playerRef.Inventory.TrySwapItem(startSlotNum, endSlotNum, startSlotEquip, endSlotEquip))
            {
                //Swapping items
                UpdateDisplay();
            }
        }

        private void CloseMenu(UICallbackPackage package)
        {
            OnExit?.Invoke();
        }

        private PlayerEntityContainer _playerRef;
        private readonly UIPanel _panelUI;
        private readonly UIElement[] _equips = new UIElement[7];
        private readonly UIElement[] _items = new UIElement[Inventory.InventorySize];
        private readonly UIElement _portrait; //Kept here since it may need to be accessed constantly
        private readonly UIElement _statBars; //Kept here since it may need to be accessed constantly
        private readonly UIText _descText; //Kept here since it may need to be accessed constantly
        private Point _lastMousePosition;
        private Coordinate _lastWindowSize;
        private EntityTexture.State _lastState;
        private bool _mouseDown;
        private int _lastTextSlot=-1, _iconRefSize;

        private const int _descCharLen = 31; //TODO: if the 31st char is space this fails
        //Text box is 9 rows of 30 characters

        public InventoryMenu(GameContainer container, Action onExit) : base(container, onExit)
        {
            _panelUI = new UIPanel("this.MenuKing", new Vector2(0.5f, 0.5f), Vector2.One, Position.Center);

            var backdrop = new UIElement("Backdrop", "UI/decor/Backdrop", new Vector2(0.5f, 0.5f), Color.White, _panelUI, Position.Center);
            var exitButton = new UIClickable("ExitButton", "UI/button/RedBulb", new Coordinate(-8, 8), ColorScale.White, backdrop, Position.Center, Position.TopRight);
            var exitButtonX = new UIElement("ExitButtonX", "UI/icon/No", new Coordinate(), ColorScale.White, exitButton, Position.Center, Position.Center);
            exitButton.RegisterCallback(CloseMenu);
            SetRulesAll(TextureLibrary.x1d25ScaleRuleSet, exitButton, exitButtonX);
            exitButton.SetExtraStates("UI/button/RedBulbDown", "UI/button/RedBulbOver");

            _statBars = new UIElement("InventoryBars", "UI/field/InventoryBars", new Coordinate(14, 14), ColorScale.White, backdrop, Position.TopLeft, Position.TopLeft);
            var textBox = new UIElement("TextBox", "UI/field/LargeEntryBox", new Coordinate(-17, 17), ColorScale.White, backdrop, Position.TopRight, Position.TopRight);
            var inventorySet = new UIElement("InventorySet", "UI/field/ThreeRowElementSet", new Coordinate(0, -17), ColorScale.White, backdrop, Position.CenterBottom, Position.CenterBottom);
            var equipmentSet = new UIElement("EquipmentSet", "UI/field/SevenElementSet", new Coordinate(0, -5), ColorScale.White, inventorySet, Position.CenterBottom, Position.CenterTop);
            _descText = new UIText("DescriptionBox", new Coordinate(14, 14), "", _descCharLen, new ColorScale[0], textBox, Position.TopLeft, Position.TopLeft);

            _portrait = new UIElement("Portrait", "null", new Coordinate(64, 60), ColorScale.White, _statBars, Position.Center, Position.TopLeft);

            var hpPipSet = new UIPanel("HPPipSet", new Coordinate(168, 12), new Coordinate(160, 24), _statBars, Position.TopLeft, Position.TopLeft);
            var mpPipSet = new UIPanel("MPPipSet", new Coordinate(168, 52), new Coordinate(160, 24), _statBars, Position.TopLeft, Position.TopLeft);
            var expPipSet = new UIPanel("EXPPipSet", new Coordinate(168, 92), new Coordinate(160, 24), _statBars, Position.TopLeft, Position.TopLeft);
            for (int i = 0; i < 10; i++)
            {
                hpPipSet.AddElements(new UIElement("HPPip"+i, "UI/fill/PipBarRed", new Coordinate(i*16, 0), ColorScale.White, hpPipSet, Position.TopLeft, Position.TopLeft));
                mpPipSet.AddElements(new UIElement("MPPip"+i, "UI/fill/PipBarBlue", new Coordinate(i*16, 0), ColorScale.White, mpPipSet, Position.TopLeft, Position.TopLeft));
                expPipSet.AddElements(new UIElement("EXPPip"+i, "UI/fill/PipBarGreen", new Coordinate(i*16, 0), ColorScale.White, expPipSet, Position.TopLeft, Position.TopLeft));
            }

            var _equipRef = new UIPanel("EquipmentZone", new Coordinate(), new Coordinate(664, 80), equipmentSet, Position.TopLeft, Position.TopLeft);
            var _itemRef = new UIPanel("ItemZone", new Coordinate(), new Coordinate(584, 224), inventorySet, Position.TopLeft, Position.TopLeft);

            var weapSlot = new UIElement("WeapSlot", "Icon/overlay/weapon", new Coordinate(36, 8), ColorScale.White, _equipRef, Position.TopLeft, Position.TopLeft);
            var headSlot = new UIElement("HeadSlot", "Icon/overlay/head", new Coordinate(124, 8), ColorScale.White, _equipRef, Position.TopLeft, Position.TopLeft);
            var chestSlot = new UIElement("ChestSlot", "Icon/overlay/chest", new Coordinate(212, 8), ColorScale.White, _equipRef, Position.TopLeft, Position.TopLeft);
            var handsSlot = new UIElement("HandsSlot", "Icon/overlay/hands", new Coordinate(300, 8), ColorScale.White, _equipRef, Position.TopLeft, Position.TopLeft);
            var legsSlot = new UIElement("LegsSlot", "Icon/overlay/legs", new Coordinate(388, 8), ColorScale.White, _equipRef, Position.TopLeft, Position.TopLeft);
            var feetSlot = new UIElement("FeetSlot", "Icon/overlay/feet", new Coordinate(476, 8), ColorScale.White, _equipRef, Position.TopLeft, Position.TopLeft);
            var accSlot = new UIElement("AccSlot", "Icon/overlay/acc", new Coordinate(564, 8), ColorScale.White, _equipRef, Position.TopLeft, Position.TopLeft);

            var _trash = new UIClickable("Trash", "Icon/action/toss", new Coordinate(512, 152), ColorScale.Grey, _itemRef, Position.TopLeft, Position.TopLeft);
            _trash.SetRules(TextureLibrary.HalfScaleRuleSet);
            _trash.SetExtraStates("", "", Color.DarkRed, Color.DarkRed);

            var undoButton = new UIClickable("UndoButton", "UI/button/BlackBulb", new Coordinate(), ColorScale.White, _trash, Position.Center, Position.TopRight);
            var undoButtonIcon = new UIElement("UndoButtonIcon", "UI/icon/Undo", new Coordinate(), ColorScale.White, undoButton, Position.Center, Position.Center);
            undoButton.RegisterCallback(UndoDelete);
            undoButton.SetExtraStates("UI/button/BlackBulbDown", "UI/button/BlackBulbOver");

            for (int i = 0; i < _equips.Length; i++)
            {
                _equips[i] = new UIMovable("Equip" + i, "null", new Coordinate(36 + i * 88, 8), ColorScale.White, _equipRef, Vector2.Zero, Vector2.One, Position.TopLeft, Position.TopLeft, true, false);
            }
            for (int i = 0; i < _items.Length; i++)
            {
                var row = i / 8;
                var col = i % 8;
                _items[i] = new UIMovable("Item"+i, "null", new Coordinate(8+col*72, 8+row*72), ColorScale.White, _itemRef, Vector2.Zero, Vector2.One, Position.TopLeft, Position.TopLeft, true, false);
            }

            _equipRef.AddElements(weapSlot, headSlot, chestSlot, handsSlot, legsSlot, feetSlot, accSlot);
            _equipRef.AddElements(_equips);
            SetRulesAll(TextureLibrary.HalfScaleRuleSet, _equips);
            RegisterAll(MoveItem, _equips);

            _itemRef.AddElements(_trash, undoButton, undoButtonIcon);
            _itemRef.AddElements(_items);
            SetRulesAll(TextureLibrary.HalfScaleRuleSet, _items);
            RegisterAll(MoveItem, _items);

            _portrait.SetRules(TextureLibrary.DoubleScaleRuleSet);
            _panelUI.AddElements(backdrop, _statBars, textBox, _descText, equipmentSet, inventorySet, _equipRef, _itemRef, hpPipSet, mpPipSet, expPipSet, _portrait, exitButton, exitButtonX);
        }

        public void SetReference(PlayerEntityContainer reference)
        {
            //This gets called initially and when characters change
            _playerRef = reference;
            reference.Inventory.Dirty = true;
            _lastState = EntityTexture.State.Down;
            _portrait.ChangeTexture(reference.Entity.GetTexture(EntityTexture.State.Down));
        }

        public void UpdateDisplay()
        {
            if (!_playerRef.Inventory.Dirty) return;

            var hpPips = (int)(_playerRef.Inventory.GetHealthAsPercentage() * 10);
            var mpPips = (int)(_playerRef.Inventory.GetManaAsPercentage() * 10);
            var expPips = (int)(_playerRef.Inventory.GetExpAsPercentage() * 10);
            var hpSet = _panelUI.GetElement("HPPipSet") as UIPanel;
            var mpSet = _panelUI.GetElement("MPPipSet") as UIPanel;
            var expSet = _panelUI.GetElement("EXPPipSet") as UIPanel;
            for (int i = 0; i < 10; i++)
            {
                hpSet?.GetElement("HPPip" + i).AllowDraw(hpPips > i);
                mpSet?.GetElement("MPPip" + i).AllowDraw(mpPips > i);
                expSet?.GetElement("EXPPip" + i).AllowDraw(expPips > i);
            }

            for (int i = 0; i < _playerRef.Inventory.Equipment.Length; i++)
            {
                var element = _equips[i] as UIMovable;
                if (_playerRef.Inventory.Equipment[i] is null)
                {
                    element?.SetNullTexture();
                }
                else
                {
                    element?.ChangeTexture(_playerRef.Inventory.Equipment[i].GetTexture());
                    element?.ChangeColor(_playerRef.Inventory.Equipment[i].GetIconColor());
                }
            }
            for (int i = 0; i < _playerRef.Inventory.Items.Length; i++)
            {
                var element = _items[i] as UIMovable;
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

            CheckForHighlight(_lastMousePosition);
            _playerRef.Inventory.Dirty = false;
            _lastTextSlot = -1;
        }
        public override void Enter(Point point)
        {
            base.Enter(point);
            _lastMousePosition = new Point(-1, -1);
            UpdateDisplay();
            //OnMoveMouse(point);
            _lastTextSlot = -1;
            //_descText.SetText("", ColorScale.Black);
        }

        public override void LoadContent(GraphicsDevice graphics, ContentManager content)
        {
            _panelUI.LoadContent(graphics, content);
            _iconRefSize = _panelUI.GetElement("Trash").CurrentPixelSize.X;
        }

        public override void OnResize(GraphicsDevice graphics, Coordinate gameWindow)
        {
            _lastWindowSize = gameWindow;
            _panelUI.OnResize(graphics, gameWindow);
            _iconRefSize = _panelUI.GetElement("Trash").CurrentPixelSize.X;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            _panelUI.Draw(spriteBatch);
        }

        private void MovePortrait(Point point)
        {
            var xdiff = point.X - (_portrait.OriginPosition.X + _portrait.CurrentPixelSize.X / 2);
            var ydiff = point.Y - _portrait.OriginPosition.Y;
            if (Math.Abs(ydiff) > Math.Abs(xdiff))
            {
                //Look up or down
                if (ydiff < 0 && _lastState != EntityTexture.State.Up)
                {
                    _portrait.ChangeTexture(_playerRef.Entity.GetTexture(EntityTexture.State.Up));
                    _lastState = EntityTexture.State.Up;
                }
                else if (ydiff >= 0 && _lastState != EntityTexture.State.Down)
                {
                    _portrait.ChangeTexture(_playerRef.Entity.GetTexture(EntityTexture.State.Down));
                    _lastState = EntityTexture.State.Down;
                }
            }
            else
            {
                //Look left or right
                if (xdiff < 0 && _lastState != EntityTexture.State.Left)
                {
                    _portrait.ChangeTexture(_playerRef.Entity.GetTexture(EntityTexture.State.Left));
                    _lastState = EntityTexture.State.Left;
                }
                else if (xdiff >= 0 && _lastState != EntityTexture.State.Right)
                {
                    _portrait.ChangeTexture(_playerRef.Entity.GetTexture(EntityTexture.State.Right));
                    _lastState = EntityTexture.State.Right;
                }
            }
        }
        private void CheckForHighlight(Point point)
        {
            if (_mouseDown) return;

            if (point.Y < _equips[0].OriginPosition.Y)
            {
                //On character maybe
                var rect = new Rectangle(_statBars.OriginPosition.X, _statBars.OriginPosition.Y, _statBars.CurrentPixelSize.X, _statBars.CurrentPixelSize.Y);
                if (rect.Contains(point))
                {
                    if (_lastTextSlot == 99) return;

                    _descText.SetText(TextDrawer.GetSmartSplit(_playerRef.ToString(), _descCharLen), ColorScale.Black);
                    _lastTextSlot = 99;
                    return;
                }
            }
            else if (point.Y < _items[0].OriginPosition.Y)
            {
                //On equipment maybe
                for (int i = 0; i < _equips.Length; i++)
                {
                    var rect = new Rectangle(_equips[i].OriginPosition.X, _equips[i].OriginPosition.Y, _equips[i].CurrentPixelSize.X, _equips[i].CurrentPixelSize.Y);
                    if (rect.Contains(point))
                    {
                        var item = _playerRef.Inventory.Equipment[i];
                        if (_lastTextSlot == i) return;
                        if (item is null) break;

                        _descText.SetText(TextDrawer.GetSmartSplit(item.GetSmartDesc(), _descCharLen), item.GetSmartColors(ColorScale.Black));
                        _lastTextSlot = i;
                        return;
                    }
                }
            }
            else
            {
                //On inventory maybe
                for (int i = 0; i < _items.Length; i++)
                {
                    var rect = new Rectangle(_items[i].OriginPosition.X, _items[i].OriginPosition.Y, _items[i].CurrentPixelSize.X, _items[i].CurrentPixelSize.Y);
                    if (rect.Contains(point))
                    {
                        var item = _playerRef.Inventory.Items[i];
                        if (_lastTextSlot == 7 + i) return;
                        if (item is null) break;

                        if (item is EquipInstance e)
                        {
                            var other = _playerRef.Inventory.Equipment[(int) e.Type];
                            _descText.SetText(TextDrawer.GetSmartSplit(e.GetSmartDesc(other), _descCharLen), e.GetSmartColors(ColorScale.Black));
                        }
                        else
                        {
                            _descText.SetText(TextDrawer.GetSmartSplit(item.GetSmartDesc(), _descCharLen), item.GetSmartColors(ColorScale.Black));
                        }

                        _lastTextSlot = 7 + i;
                        return;
                    }
                }
            }

            _descText.SetText("", ColorScale.Black);
            _lastTextSlot = -1;
        }

        public override bool OnMoveMouse(Point point)
        {
            if (point == _lastMousePosition) return false;
            _lastMousePosition = point;
            _panelUI.OnMoveMouse(point);
            MovePortrait(point);
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
