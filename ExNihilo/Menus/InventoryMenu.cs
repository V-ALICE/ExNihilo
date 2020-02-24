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
        private Inventory _invRef;
        private readonly UIPanel _panelUI;
        private readonly UIElement[] _equips = new UIElement[7];
        private readonly UIElement[] _items = new UIElement[Inventory.InventorySize];
        private readonly UIText descTextBox;
        private Point _lastMousePosition;
        private Coordinate _lastWindowSize;

        //Text box is 11 characters

        public InventoryMenu(GameContainer container) : base(container)
        {
            _panelUI = new UIPanel("this.MenuKing", new Vector2(0.5f, 0.5f), Vector2.One, Position.Center);

            var backdrop = new UIElement("Backdrop", "UI/decor/Backdrop", new Vector2(0.5f, 0.5f), Color.White, _panelUI, Position.Center);
            var inventoryBars = new UIElement("InventoryBars", "UI/field/InventoryBars", new Coordinate(14, 14), ColorScale.White, backdrop, Position.TopLeft, Position.TopLeft);
            var textBox = new UIElement("TextBox", "UI/field/LargeEntryBox", new Coordinate(-14, 14), ColorScale.White, backdrop, Position.TopRight, Position.TopRight);
            var inventorySet = new UIElement("InventorySet", "UI/field/ThreeRowElementSet", new Coordinate(0, -14), ColorScale.White, backdrop, Position.CenterBottom, Position.CenterBottom);
            var equipmentSet = new UIElement("EquipmentSet", "UI/field/SevenElementSet", new Coordinate(0, -5), ColorScale.White, inventorySet, Position.CenterBottom, Position.CenterTop);
            descTextBox = new UIText("DescriptionBox", new Coordinate(), "", new ColorScale[0], textBox, Position.TopLeft, Position.TopLeft);
            //TODO: add trash can somewhere

            //var _equipRef = new UIPanel("EquipmentZone", new Coordinate(), new Coordinate(664, 80), equipmentSet, Position.TopLeft, Position.TopLeft);
            //var _itemRef = new UIPanel("ItemZone", new Coordinate(), new Coordinate(584, 224), inventorySet, Position.TopLeft, Position.TopLeft);
            var _equipRef = new UIPanel("EquipmentZone", new Coordinate(), new Coordinate(700, 500), backdrop, Position.TopLeft, Position.TopLeft);
            var _itemRef = new UIPanel("ItemZone", new Coordinate(), new Coordinate(700, 500), backdrop, Position.TopLeft, Position.TopLeft);

            //TODO: these need to move to the actual slots somehow
            _equips[0] = new UIMovable("Equip0", "null", new Coordinate(36, 10), ColorScale.White, _equipRef, Position.TopLeft, Position.TopLeft, true, false);
            _equips[1] = new UIMovable("Equip1", "null", new Coordinate(124, 10), ColorScale.White, _equipRef, Position.TopLeft, Position.TopLeft, true, false);
            _equips[2] = new UIMovable("Equip2", "null", new Coordinate(212, 10), ColorScale.White, _equipRef, Position.TopLeft, Position.TopLeft, true, false);
            _equips[3] = new UIMovable("Equip3", "null", new Coordinate(300, 10), ColorScale.White, _equipRef, Position.TopLeft, Position.TopLeft, true, false);
            _equips[4] = new UIMovable("Equip4", "null", new Coordinate(388, 10), ColorScale.White, _equipRef, Position.TopLeft, Position.TopLeft, true, false);
            _equips[5] = new UIMovable("Equip5", "null", new Coordinate(476, 10), ColorScale.White, _equipRef, Position.TopLeft, Position.TopLeft, true, false);
            _equips[6] = new UIMovable("Equip6", "null", new Coordinate(564, 10), ColorScale.White, _equipRef, Position.TopLeft, Position.TopLeft, true, false);

            _items[0] = new UIMovable("Item0", "null", new Coordinate(8, 8), ColorScale.White, _itemRef, Position.TopLeft, Position.TopLeft, true, false);
            _items[1] = new UIMovable("Item1", "null", new Coordinate(80, 8), ColorScale.White, _itemRef, Position.TopLeft, Position.TopLeft, true, false);
            _items[2] = new UIMovable("Item2", "null", new Coordinate(152, 8), ColorScale.White, _itemRef, Position.TopLeft, Position.TopLeft, true, false);
            _items[3] = new UIMovable("Item3", "null", new Coordinate(224, 8), ColorScale.White, _itemRef, Position.TopLeft, Position.TopLeft, true, false);
            _items[4] = new UIMovable("Item4", "null", new Coordinate(296, 8), ColorScale.White, _itemRef, Position.TopLeft, Position.TopLeft, true, false);
            _items[5] = new UIMovable("Item5", "null", new Coordinate(368, 8), ColorScale.White, _itemRef, Position.TopLeft, Position.TopLeft, true, false);
            _items[6] = new UIMovable("Item6", "null", new Coordinate(440, 8), ColorScale.White, _itemRef, Position.TopLeft, Position.TopLeft, true, false);
            _items[7] = new UIMovable("Item7", "null", new Coordinate(512, 8), ColorScale.White, _itemRef, Position.TopLeft, Position.TopLeft, true, false);
            _items[8] = new UIMovable("Item8", "null", new Coordinate(8, 80), ColorScale.White, _itemRef, Position.TopLeft, Position.TopLeft, true, false);
            _items[9] = new UIMovable("Item9", "null", new Coordinate(80, 80), ColorScale.White, _itemRef, Position.TopLeft, Position.TopLeft, true, false);
            _items[10] = new UIMovable("Item10", "null", new Coordinate(152, 80), ColorScale.White, _itemRef, Position.TopLeft, Position.TopLeft, true, false);
            _items[11] = new UIMovable("Item11", "null", new Coordinate(224, 80), ColorScale.White, _itemRef, Position.TopLeft, Position.TopLeft, true, false);
            _items[12] = new UIMovable("Item12", "null", new Coordinate(296, 80), ColorScale.White, _itemRef, Position.TopLeft, Position.TopLeft, true, false);
            _items[13] = new UIMovable("Item13", "null", new Coordinate(368, 80), ColorScale.White, _itemRef, Position.TopLeft, Position.TopLeft, true, false);
            _items[14] = new UIMovable("Item14", "null", new Coordinate(440, 80), ColorScale.White, _itemRef, Position.TopLeft, Position.TopLeft, true, false);
            _items[15] = new UIMovable("Item15", "null", new Coordinate(512, 80), ColorScale.White, _itemRef, Position.TopLeft, Position.TopLeft, true, false);
            _items[16] = new UIMovable("Item16", "null", new Coordinate(8, 152), ColorScale.White, _itemRef, Position.TopLeft, Position.TopLeft, true, false);
            _items[17] = new UIMovable("Item17", "null", new Coordinate(80, 152), ColorScale.White, _itemRef, Position.TopLeft, Position.TopLeft, true, false);
            _items[18] = new UIMovable("Item18", "null", new Coordinate(152, 152), ColorScale.White, _itemRef, Position.TopLeft, Position.TopLeft, true, false);
            _items[19] = new UIMovable("Item19", "null", new Coordinate(224, 152), ColorScale.White, _itemRef, Position.TopLeft, Position.TopLeft, true, false);
            _items[20] = new UIMovable("Item20", "null", new Coordinate(296, 152), ColorScale.White, _itemRef, Position.TopLeft, Position.TopLeft, true, false);
            _items[21] = new UIMovable("Item21", "null", new Coordinate(368, 152), ColorScale.White, _itemRef, Position.TopLeft, Position.TopLeft, true, false);
            _items[22] = new UIMovable("Item22", "null", new Coordinate(440, 152), ColorScale.White, _itemRef, Position.TopLeft, Position.TopLeft, true, false);
            _items[23] = new UIMovable("Item23", "null", new Coordinate(512, 152), ColorScale.White, _itemRef, Position.TopLeft, Position.TopLeft, true, false);

            _equipRef.AddElements(_equips);
            SetRulesAll(TextureLibrary.HalfScaleRuleSet, _equips);
            _itemRef.AddElements(_items);
            SetRulesAll(TextureLibrary.HalfScaleRuleSet, _items);

            _panelUI.AddElements(backdrop, inventoryBars, textBox, equipmentSet, inventorySet, _equipRef, _itemRef);
        }

        public void SetReference(Inventory reference)
        {
            //This gets called initially and when characters change
            _invRef = reference;
        }

        public override void Enter(Point point)
        {
            base.Enter(point);
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

            _invRef.Dirty = false;
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

        public override void OnMoveMouse(Point point)
        {
            _lastMousePosition = point;
            _panelUI.OnMoveMouse(point);
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
