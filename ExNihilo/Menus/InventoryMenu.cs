using System;
using System.Linq;
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
                var rect = new Rectangle(_equips[i].OriginPosition.X, _equips[i].OriginPosition.Y, 
                    (int) (_equips[i].CurrentScale* _iconRefSize), (int) (_equips[i].CurrentScale * _iconRefSize));
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
                    var rect = new Rectangle(_items[i].OriginPosition.X, _items[i].OriginPosition.Y, 
                        (int) (_items[i].CurrentScale * _iconRefSize), (int) (_items[i].CurrentScale * _iconRefSize));
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
                var rect = new Rectangle(_trash.OriginPosition.X, _trash.OriginPosition.Y,
                    (int)(_trash.CurrentScale * _iconRefSize), (int)(_trash.CurrentScale * _iconRefSize));
                if (rect.Contains(package.ScreenPos))
                {
                    //Throw item away
                    _playerRef.Inventory.RemoveItem(startSlotNum, startSlotEquip, true);
                    //TODO: this can be made to only update the one affected object instead
                    UpdateDisplay();
                }
            }
            else if (_playerRef.Inventory.TrySwapItem(startSlotNum, endSlotNum, startSlotEquip, endSlotEquip))
            {
                //Swapping items
                //TODO: this can be made to only update the two affected objects instead
                UpdateDisplay();
            }
        }
        
        private PlayerEntityContainer _playerRef;
        private readonly UIPanel _panelUI;
        private readonly UIElement[] _equips = new UIElement[7];
        private readonly UIElement[] _items = new UIElement[Inventory.InventorySize];
        private readonly UIElement _portrait; //Kept here since it may need to be accessed constantly
        //private readonly UIText descText;
        private Point _lastMousePosition;
        private Coordinate _lastWindowSize;
        private readonly int _iconRefSize = 128; //TODO: better way to do this
        private EntityTexture.State _lastState;

        //Text box is 9 rows of 30 characters

        public InventoryMenu(GameContainer container) : base(container)
        {
            _panelUI = new UIPanel("this.MenuKing", new Vector2(0.5f, 0.5f), Vector2.One, Position.Center);

            var backdrop = new UIElement("Backdrop", "UI/decor/Backdrop", new Vector2(0.5f, 0.5f), Color.White, _panelUI, Position.Center);
            var inventoryBars = new UIElement("InventoryBars", "UI/field/InventoryBars", new Coordinate(14, 14), ColorScale.White, backdrop, Position.TopLeft, Position.TopLeft);
            var textBox = new UIElement("TextBox", "UI/field/LargeEntryBox", new Coordinate(-14, 14), ColorScale.White, backdrop, Position.TopRight, Position.TopRight);
            var inventorySet = new UIElement("InventorySet", "UI/field/ThreeRowElementSet", new Coordinate(0, -14), ColorScale.White, backdrop, Position.CenterBottom, Position.CenterBottom);
            var equipmentSet = new UIElement("EquipmentSet", "UI/field/SevenElementSet", new Coordinate(0, -5), ColorScale.White, inventorySet, Position.CenterBottom, Position.CenterTop);
            var descText = new UIText("DescriptionBox", new Coordinate(14, 14), "0123456789012345678901234567890\n1\n2\n3\n4\n5\n6\n7\n8", new ColorScale[0], textBox, Position.TopLeft, Position.TopLeft);

            _portrait = new UIElement("Portrait", "null", new Coordinate(63, 62), ColorScale.White, inventoryBars, Position.Center, Position.TopLeft);

            var hpPipSet = new UIPanel("HPPipSet", new Coordinate(168, 12), new Coordinate(160, 24), inventoryBars, Position.TopLeft, Position.TopLeft);
            var mpPipSet = new UIPanel("MPPipSet", new Coordinate(168, 52), new Coordinate(160, 24), inventoryBars, Position.TopLeft, Position.TopLeft);
            var expPipSet = new UIPanel("EXPPipSet", new Coordinate(168, 92), new Coordinate(160, 24), inventoryBars, Position.TopLeft, Position.TopLeft);
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
            _panelUI.AddElements(backdrop, inventoryBars, textBox, descText, equipmentSet, inventorySet, _equipRef, _itemRef, hpPipSet, mpPipSet, expPipSet, _portrait);
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

            _playerRef.Inventory.Dirty = false;
        }
        public override void Enter(Point point)
        {
            base.Enter(point);
            UpdateDisplay();
            OnMoveMouse(point);
        }

        public override void LoadContent(GraphicsDevice graphics, ContentManager content)
        {
            _panelUI.LoadContent(graphics, content);
        }

        public override void OnResize(GraphicsDevice graphics, Coordinate gameWindow)
        {
            _lastWindowSize = gameWindow;
            _panelUI.OnResize(graphics, gameWindow);
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

        public override bool OnMoveMouse(Point point)
        {
            _lastMousePosition = point;
            _panelUI.OnMoveMouse(point);
            MovePortrait(point);
            return false;
        }

        public override bool OnLeftClick(Point point)
        {
            return _panelUI.OnLeftClick(point);
        }

        public override void OnLeftRelease(Point point)
        {
            _panelUI.OnLeftRelease(point);
        }

        public override void ReceiveInput(string input)
        {
        }
    }
}
