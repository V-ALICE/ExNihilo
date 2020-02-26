using System.Linq;
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
            _invRef.TryRestoreTrashedItem();
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
                var rect = new Rectangle(_trash.OriginPosition.X, _trash.OriginPosition.Y,
                    (int)(_trash.CurrentScale * _iconRefSize), (int)(_trash.CurrentScale * _iconRefSize));
                if (rect.Contains(package.ScreenPos))
                {
                    //Throw item away
                    _invRef.RemoveItem(startSlotNum, startSlotEquip, true);
                    //TODO: this can be made to only update the one affected object instead
                    UpdateDisplay();
                }
            }
            else if (_invRef.TrySwapItem(startSlotNum, endSlotNum, startSlotEquip, endSlotEquip))
            {
                //Swapping items
                //TODO: this can be made to only update the two affected objects instead
                UpdateDisplay();
            }
        }

        private Inventory _invRef;
        private readonly UIPanel _panelUI;
        private readonly UIElement[] _equips = new UIElement[7];
        private readonly UIElement[] _items = new UIElement[Inventory.InventorySize];
        private readonly UIClickable _trash;
        //private readonly UIText descText;
        private Point _lastMousePosition;
        private Coordinate _lastWindowSize;
        private int _iconRefSize = 128; //TODO: set this in a smarter way 

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
            
            var _equipRef = new UIPanel("EquipmentZone", new Coordinate(), new Coordinate(664, 80), equipmentSet, Position.TopLeft, Position.TopLeft);
            var weapSlot = new UIElement("WeapSlot", "Icon/overlay/weapon", new Coordinate(36, 8), ColorScale.White, _equipRef, Position.TopLeft, Position.TopLeft);
            var headSlot = new UIElement("HeadSlot", "Icon/overlay/head", new Coordinate(124, 8), ColorScale.White, _equipRef, Position.TopLeft, Position.TopLeft);
            var chestSlot = new UIElement("ChestSlot", "Icon/overlay/chest", new Coordinate(212, 8), ColorScale.White, _equipRef, Position.TopLeft, Position.TopLeft);
            var handsSlot = new UIElement("HandsSlot", "Icon/overlay/hands", new Coordinate(300, 8), ColorScale.White, _equipRef, Position.TopLeft, Position.TopLeft);
            var legsSlot = new UIElement("LegsSlot", "Icon/overlay/legs", new Coordinate(388, 8), ColorScale.White, _equipRef, Position.TopLeft, Position.TopLeft);
            var feetSlot = new UIElement("FeetSlot", "Icon/overlay/feet", new Coordinate(476, 8), ColorScale.White, _equipRef, Position.TopLeft, Position.TopLeft);
            var accSlot = new UIElement("AccSlot", "Icon/overlay/acc", new Coordinate(564, 8), ColorScale.White, _equipRef, Position.TopLeft, Position.TopLeft);

            _equips[0] = new UIMovable("Equip0", "null", new Coordinate(36, 8), ColorScale.White, _equipRef, Vector2.Zero, Vector2.One, Position.TopLeft, Position.TopLeft, true, false);
            _equips[1] = new UIMovable("Equip1", "null", new Coordinate(124, 8), ColorScale.White, _equipRef, Vector2.Zero, Vector2.One, Position.TopLeft, Position.TopLeft, true, false);
            _equips[2] = new UIMovable("Equip2", "null", new Coordinate(212, 8), ColorScale.White, _equipRef, Vector2.Zero, Vector2.One, Position.TopLeft, Position.TopLeft, true, false);
            _equips[3] = new UIMovable("Equip3", "null", new Coordinate(300, 8), ColorScale.White, _equipRef, Vector2.Zero, Vector2.One, Position.TopLeft, Position.TopLeft, true, false);
            _equips[4] = new UIMovable("Equip4", "null", new Coordinate(388, 8), ColorScale.White, _equipRef, Vector2.Zero, Vector2.One, Position.TopLeft, Position.TopLeft, true, false);
            _equips[5] = new UIMovable("Equip5", "null", new Coordinate(476, 8), ColorScale.White, _equipRef, Vector2.Zero, Vector2.One, Position.TopLeft, Position.TopLeft, true, false);
            _equips[6] = new UIMovable("Equip6", "null", new Coordinate(564, 8), ColorScale.White, _equipRef, Vector2.Zero, Vector2.One, Position.TopLeft, Position.TopLeft, true, false);

            var _itemRef = new UIPanel("ItemZone", new Coordinate(), new Coordinate(584, 224), inventorySet, Position.TopLeft, Position.TopLeft);
            _items[0] = new UIMovable("Item0", "null", new Coordinate(8, 8), ColorScale.White, _itemRef, Vector2.Zero, Vector2.One, Position.TopLeft, Position.TopLeft, true, false);
            _items[1] = new UIMovable("Item1", "null", new Coordinate(80, 8), ColorScale.White, _itemRef, Vector2.Zero, Vector2.One, Position.TopLeft, Position.TopLeft, true, false);
            _items[2] = new UIMovable("Item2", "null", new Coordinate(152, 8), ColorScale.White, _itemRef, Vector2.Zero, Vector2.One, Position.TopLeft, Position.TopLeft, true, false);
            _items[3] = new UIMovable("Item3", "null", new Coordinate(224, 8), ColorScale.White, _itemRef, Vector2.Zero, Vector2.One, Position.TopLeft, Position.TopLeft, true, false);
            _items[4] = new UIMovable("Item4", "null", new Coordinate(296, 8), ColorScale.White, _itemRef, Vector2.Zero, Vector2.One, Position.TopLeft, Position.TopLeft, true, false);
            _items[5] = new UIMovable("Item5", "null", new Coordinate(368, 8), ColorScale.White, _itemRef, Vector2.Zero, Vector2.One, Position.TopLeft, Position.TopLeft, true, false);
            _items[6] = new UIMovable("Item6", "null", new Coordinate(440, 8), ColorScale.White, _itemRef, Vector2.Zero, Vector2.One, Position.TopLeft, Position.TopLeft, true, false);
            _items[7] = new UIMovable("Item7", "null", new Coordinate(512, 8), ColorScale.White, _itemRef, Vector2.Zero, Vector2.One, Position.TopLeft, Position.TopLeft, true, false);
            _items[8] = new UIMovable("Item8", "null", new Coordinate(8, 80), ColorScale.White, _itemRef, Vector2.Zero, Vector2.One, Position.TopLeft, Position.TopLeft, true, false);
            _items[9] = new UIMovable("Item9", "null", new Coordinate(80, 80), ColorScale.White, _itemRef, Vector2.Zero, Vector2.One, Position.TopLeft, Position.TopLeft, true, false);
            _items[10] = new UIMovable("Item10", "null", new Coordinate(152, 80), ColorScale.White, _itemRef, Vector2.Zero, Vector2.One, Position.TopLeft, Position.TopLeft, true, false);
            _items[11] = new UIMovable("Item11", "null", new Coordinate(224, 80), ColorScale.White, _itemRef, Vector2.Zero, Vector2.One, Position.TopLeft, Position.TopLeft, true, false);
            _items[12] = new UIMovable("Item12", "null", new Coordinate(296, 80), ColorScale.White, _itemRef, Vector2.Zero, Vector2.One, Position.TopLeft, Position.TopLeft, true, false);
            _items[13] = new UIMovable("Item13", "null", new Coordinate(368, 80), ColorScale.White, _itemRef, Vector2.Zero, Vector2.One, Position.TopLeft, Position.TopLeft, true, false);
            _items[14] = new UIMovable("Item14", "null", new Coordinate(440, 80), ColorScale.White, _itemRef, Vector2.Zero, Vector2.One, Position.TopLeft, Position.TopLeft, true, false);
            _items[15] = new UIMovable("Item15", "null", new Coordinate(512, 80), ColorScale.White, _itemRef, Vector2.Zero, Vector2.One, Position.TopLeft, Position.TopLeft, true, false);
            _items[16] = new UIMovable("Item16", "null", new Coordinate(8, 152), ColorScale.White, _itemRef, Vector2.Zero, Vector2.One, Position.TopLeft, Position.TopLeft, true, false);
            _items[17] = new UIMovable("Item17", "null", new Coordinate(80, 152), ColorScale.White, _itemRef, Vector2.Zero, Vector2.One, Position.TopLeft, Position.TopLeft, true, false);
            _items[18] = new UIMovable("Item18", "null", new Coordinate(152, 152), ColorScale.White, _itemRef, Vector2.Zero, Vector2.One, Position.TopLeft, Position.TopLeft, true, false);
            _items[19] = new UIMovable("Item19", "null", new Coordinate(224, 152), ColorScale.White, _itemRef, Vector2.Zero, Vector2.One, Position.TopLeft, Position.TopLeft, true, false);
            _items[20] = new UIMovable("Item20", "null", new Coordinate(296, 152), ColorScale.White, _itemRef, Vector2.Zero, Vector2.One, Position.TopLeft, Position.TopLeft, true, false);
            _items[21] = new UIMovable("Item21", "null", new Coordinate(368, 152), ColorScale.White, _itemRef, Vector2.Zero, Vector2.One, Position.TopLeft, Position.TopLeft, true, false);
            _items[22] = new UIMovable("Item22", "null", new Coordinate(440, 152), ColorScale.White, _itemRef, Vector2.Zero, Vector2.One, Position.TopLeft, Position.TopLeft, true, false);
            //_items[23] = new UIMovable("Item23", "null", new Coordinate(512, 152), ColorScale.White, _itemRef, Vector2.Zero, Vector2.One, Position.TopLeft, Position.TopLeft, true, false);

            _trash = new UIClickable("Trash", "Icon/action/toss", new Coordinate(512, 152), ColorScale.Grey, _itemRef, Position.TopLeft, Position.TopLeft);
            _trash.SetRules(TextureLibrary.HalfScaleRuleSet);
            _trash.SetExtraStates("", "", Color.DarkRed, Color.DarkRed);

            var undoButton = new UIClickable("UndoButton", "UI/button/BlackBulb", new Coordinate(), ColorScale.White, _trash, Position.Center, Position.TopRight);
            var undoButtonIcon = new UIElement("UndoButtonIcon", "UI/icon/Undo", new Coordinate(), ColorScale.White, undoButton, Position.Center, Position.Center);
            undoButton.RegisterCallback(UndoDelete);
            undoButton.SetExtraStates("UI/button/BlackBulbDown", "UI/button/BlackBulbOver");

            _equipRef.AddElements(weapSlot, headSlot, chestSlot, handsSlot, legsSlot, feetSlot, accSlot);
            _equipRef.AddElements(_equips);
            SetRulesAll(TextureLibrary.HalfScaleRuleSet, _equips);
            RegisterAll(MoveItem, _equips);

            _itemRef.AddElements(_trash, undoButton, undoButtonIcon);
            _itemRef.AddElements(_items);
            SetRulesAll(TextureLibrary.HalfScaleRuleSet, _items);
            RegisterAll(MoveItem, _items);

            _panelUI.AddElements(backdrop, inventoryBars, textBox, descText, equipmentSet, inventorySet, _equipRef, _itemRef);
        }

        public void SetReference(Inventory reference)
        {
            //This gets called initially and when characters change
            _invRef = reference;
            _invRef.Dirty = true;
        }

        public void UpdateDisplay()
        {
            if (!_invRef.Dirty) return;

            for (int i = 0; i < _invRef._equipment.Length; i++)
            {
                var element = _equips[i] as UIMovable;
                if (_invRef._equipment[i] is null)
                {
                    element?.SetNullTexture();
                }
                else
                {
                    element?.ChangeTexture(_invRef._equipment[i].GetTexture());
                    element?.ChangeColor(_invRef._equipment[i].GetIconColor());
                }
            }
            for (int i = 0; i < _invRef._inventory.Length; i++)
            {
                var element = _items[i] as UIMovable;
                if (_invRef._inventory[i] is null)
                {
                    element?.SetNullTexture();
                }
                else
                {
                    element?.ChangeTexture(_invRef._inventory[i].GetTexture());
                    element?.ChangeColor(_invRef._inventory[i].GetIconColor());
                }
            }

            if (_invRef.CanRestoreTrashedItem()) (_panelUI.GetElement("UndoButton") as UIClickable)?.Enable();
            else (_panelUI.GetElement("UndoButton") as UIClickable)?.Disable(ColorScale.Ghost);

            _invRef.Dirty = false;
        }
        public override void Enter(Point point)
        {
            base.Enter(point);
            UpdateDisplay();
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

        public override bool OnMoveMouse(Point point)
        {
            _lastMousePosition = point;
            _panelUI.OnMoveMouse(point);
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
