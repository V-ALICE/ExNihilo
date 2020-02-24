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
            //var backButton = new UIClickable("BackButton", "UI/button/SmallButton", new Coordinate(14, -14), ColorScale.White, backdrop, Position.BottomLeft, Position.BottomLeft);
            //var backButtonText = new UIText("BackButtonText", new Coordinate(), "Back", ColorScale.Black, backButton, Position.Center, Position.Center);
            var inventoryBars = new UIElement("InventoryBars", "UI/field/InventoryBars", new Coordinate(14, 14), ColorScale.White, backdrop, Position.TopLeft, Position.TopLeft);
            var textBox = new UIElement("TextBox", "UI/field/LargeEntryBox", new Coordinate(-14, 14), ColorScale.White, backdrop, Position.TopRight, Position.TopRight);
            var inventorySet = new UIElement("InventorySet", "UI/field/ThreeRowElementSet", new Coordinate(0, -14), ColorScale.White, backdrop, Position.CenterBottom, Position.CenterBottom);
            var equipmentSet = new UIElement("EquipmentSet", "UI/field/SevenElementSet", new Coordinate(0, -5), ColorScale.White, inventorySet, Position.CenterBottom, Position.CenterTop);
            descTextBox = new UIText("DescriptionBox", new Coordinate(), "", new ColorScale[0], textBox, Position.TopLeft, Position.TopLeft);
            //TODO: add trash can somewhere

            var _equipRef = new UIPanel("EquipmentZone", new Coordinate(0, 0), new Coordinate(), equipmentSet, Position.TopLeft, Position.TopLeft);
            var _itemRef = new UIPanel("ItemZone", new Coordinate(0, 0), new Coordinate(), inventorySet, Position.TopLeft, Position.TopLeft);

            _equips[0] = new UIDynamicElement("Equip0", "null", new Coordinate(), ColorScale.White, _equipRef, Position.TopLeft, Position.TopLeft);
            _equips[1] = new UIDynamicElement("Equip1", "null", new Coordinate(), ColorScale.White, _equipRef, Position.TopLeft, Position.TopLeft);
            _equips[2] = new UIDynamicElement("Equip2", "null", new Coordinate(), ColorScale.White, _equipRef, Position.TopLeft, Position.TopLeft);
            _equips[3] = new UIDynamicElement("Equip3", "null", new Coordinate(), ColorScale.White, _equipRef, Position.TopLeft, Position.TopLeft);
            _equips[4] = new UIDynamicElement("Equip4", "null", new Coordinate(), ColorScale.White, _equipRef, Position.TopLeft, Position.TopLeft);
            _equips[5] = new UIDynamicElement("Equip5", "null", new Coordinate(), ColorScale.White, _equipRef, Position.TopLeft, Position.TopLeft);
            _equips[6] = new UIDynamicElement("Equip6", "null", new Coordinate(), ColorScale.White, _equipRef, Position.TopLeft, Position.TopLeft);

            _items[0] = new UIDynamicElement("Item0", "null", new Coordinate(8, 8), ColorScale.White, _itemRef, Position.TopLeft, Position.TopLeft);
            _items[1] = new UIDynamicElement("Item1", "null", new Coordinate(144, 0), ColorScale.White, _items[0], Position.TopLeft, Position.TopLeft);
            _items[2] = new UIDynamicElement("Item2", "null", new Coordinate(144, 0), ColorScale.White, _items[1], Position.TopLeft, Position.TopLeft);
            _items[3] = new UIDynamicElement("Item3", "null", new Coordinate(144, 0), ColorScale.White, _items[2], Position.TopLeft, Position.TopLeft);
            _items[4] = new UIDynamicElement("Item4", "null", new Coordinate(144, 0), ColorScale.White, _items[3], Position.TopLeft, Position.TopLeft);
            _items[5] = new UIDynamicElement("Item5", "null", new Coordinate(144, 0), ColorScale.White, _items[4], Position.TopLeft, Position.TopLeft);
            _items[6] = new UIDynamicElement("Item6", "null", new Coordinate(144, 0), ColorScale.White, _items[5], Position.TopLeft, Position.TopLeft);
            _items[7] = new UIDynamicElement("Item7", "null", new Coordinate(144, 0), ColorScale.White, _items[6], Position.TopLeft, Position.TopLeft);
            _items[8] = new UIDynamicElement("Item8", "null", new Coordinate(0, 144), ColorScale.White, _items[0], Position.TopLeft, Position.TopLeft);
            _items[9] = new UIDynamicElement("Item9", "null", new Coordinate(144, 0), ColorScale.White, _items[8], Position.TopLeft, Position.TopLeft);
            _items[10] = new UIDynamicElement("Item10", "null", new Coordinate(144, 0), ColorScale.White, _items[9], Position.TopLeft, Position.TopLeft);
            _items[11] = new UIDynamicElement("Item11", "null", new Coordinate(144, 0), ColorScale.White, _items[10], Position.TopLeft, Position.TopLeft);
            _items[12] = new UIDynamicElement("Item12", "null", new Coordinate(144, 0), ColorScale.White, _items[11], Position.TopLeft, Position.TopLeft);
            _items[13] = new UIDynamicElement("Item13", "null", new Coordinate(144, 0), ColorScale.White, _items[12], Position.TopLeft, Position.TopLeft);
            _items[14] = new UIDynamicElement("Item14", "null", new Coordinate(144, 0), ColorScale.White, _items[13], Position.TopLeft, Position.TopLeft);
            _items[15] = new UIDynamicElement("Item15", "null", new Coordinate(144, 0), ColorScale.White, _items[14], Position.TopLeft, Position.TopLeft);
            _items[16] = new UIDynamicElement("Item16", "null", new Coordinate(0, 144), ColorScale.White, _items[8], Position.TopLeft, Position.TopLeft);
            _items[17] = new UIDynamicElement("Item17", "null", new Coordinate(144, 0), ColorScale.White, _items[16], Position.TopLeft, Position.TopLeft);
            _items[18] = new UIDynamicElement("Item18", "null", new Coordinate(144, 0), ColorScale.White, _items[17], Position.TopLeft, Position.TopLeft);
            _items[19] = new UIDynamicElement("Item19", "null", new Coordinate(144, 0), ColorScale.White, _items[18], Position.TopLeft, Position.TopLeft);
            _items[20] = new UIDynamicElement("Item20", "null", new Coordinate(144, 0), ColorScale.White, _items[19], Position.TopLeft, Position.TopLeft);
            _items[21] = new UIDynamicElement("Item21", "null", new Coordinate(144, 0), ColorScale.White, _items[20], Position.TopLeft, Position.TopLeft);
            _items[22] = new UIDynamicElement("Item22", "null", new Coordinate(144, 0), ColorScale.White, _items[21], Position.TopLeft, Position.TopLeft);
            _items[23] = new UIDynamicElement("Item23", "null", new Coordinate(144, 0), ColorScale.White, _items[22], Position.TopLeft, Position.TopLeft);

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
                var element = _equips[i] as UIDynamicElement;
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
                var element = _items[i] as UIDynamicElement;
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
