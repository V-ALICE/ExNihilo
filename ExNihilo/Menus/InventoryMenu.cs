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
        private readonly UIPanel _panelUI, _equipRef, _itemRef;
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

            _equipRef = new UIPanel("EquipmentZone", new Coordinate(0, 0), new Vector2(1, 1), equipmentSet, Position.TopLeft, Position.TopLeft);
            _itemRef = new UIPanel("ItemZone", new Coordinate(0, 0), new Vector2(1, 1), inventorySet, Position.TopLeft, Position.TopLeft);

            var equip0 = new UIDynamicElement("Equip0", "null", new Coordinate(), ColorScale.White, _equipRef, Position.TopLeft, Position.TopLeft);
            var equip1 = new UIDynamicElement("Equip1", "null", new Coordinate(), ColorScale.White, _equipRef, Position.TopLeft, Position.TopLeft);
            var equip2 = new UIDynamicElement("Equip2", "null", new Coordinate(), ColorScale.White, _equipRef, Position.TopLeft, Position.TopLeft);
            var equip3 = new UIDynamicElement("Equip3", "null", new Coordinate(), ColorScale.White, _equipRef, Position.TopLeft, Position.TopLeft);
            var equip4 = new UIDynamicElement("Equip4", "null", new Coordinate(), ColorScale.White, _equipRef, Position.TopLeft, Position.TopLeft);
            var equip5 = new UIDynamicElement("Equip5", "null", new Coordinate(), ColorScale.White, _equipRef, Position.TopLeft, Position.TopLeft);
            var equip6 = new UIDynamicElement("Equip6", "null", new Coordinate(), ColorScale.White, _equipRef, Position.TopLeft, Position.TopLeft);

            var item0 = new UIDynamicElement("Item0", "null", new Coordinate(), ColorScale.White, _itemRef, Position.TopLeft, Position.TopLeft);
            var item1 = new UIDynamicElement("Item1", "null", new Coordinate(), ColorScale.White, _itemRef, Position.TopLeft, Position.TopLeft);
            var item2 = new UIDynamicElement("Item2", "null", new Coordinate(), ColorScale.White, _itemRef, Position.TopLeft, Position.TopLeft);
            var item3 = new UIDynamicElement("Item3", "null", new Coordinate(), ColorScale.White, _itemRef, Position.TopLeft, Position.TopLeft);
            var item4 = new UIDynamicElement("Item4", "null", new Coordinate(), ColorScale.White, _itemRef, Position.TopLeft, Position.TopLeft);
            var item5 = new UIDynamicElement("Item5", "null", new Coordinate(), ColorScale.White, _itemRef, Position.TopLeft, Position.TopLeft);
            var item6 = new UIDynamicElement("Item6", "null", new Coordinate(), ColorScale.White, _itemRef, Position.TopLeft, Position.TopLeft);
            var item7 = new UIDynamicElement("Item7", "null", new Coordinate(), ColorScale.White, _itemRef, Position.TopLeft, Position.TopLeft);
            var item8 = new UIDynamicElement("Item8", "null", new Coordinate(), ColorScale.White, _itemRef, Position.TopLeft, Position.TopLeft);
            var item9 = new UIDynamicElement("Item9", "null", new Coordinate(), ColorScale.White, _itemRef, Position.TopLeft, Position.TopLeft);
            var item10 = new UIDynamicElement("Item10", "null", new Coordinate(), ColorScale.White, _itemRef, Position.TopLeft, Position.TopLeft);
            var item11 = new UIDynamicElement("Item11", "null", new Coordinate(), ColorScale.White, _itemRef, Position.TopLeft, Position.TopLeft);
            var item12 = new UIDynamicElement("Item12", "null", new Coordinate(), ColorScale.White, _itemRef, Position.TopLeft, Position.TopLeft);
            var item13 = new UIDynamicElement("Item13", "null", new Coordinate(), ColorScale.White, _itemRef, Position.TopLeft, Position.TopLeft);
            var item14 = new UIDynamicElement("Item14", "null", new Coordinate(), ColorScale.White, _itemRef, Position.TopLeft, Position.TopLeft);
            var item15 = new UIDynamicElement("Item15", "null", new Coordinate(), ColorScale.White, _itemRef, Position.TopLeft, Position.TopLeft);
            var item16 = new UIDynamicElement("Item16", "null", new Coordinate(), ColorScale.White, _itemRef, Position.TopLeft, Position.TopLeft);
            var item17 = new UIDynamicElement("Item17", "null", new Coordinate(), ColorScale.White, _itemRef, Position.TopLeft, Position.TopLeft);
            var item18 = new UIDynamicElement("Item18", "null", new Coordinate(), ColorScale.White, _itemRef, Position.TopLeft, Position.TopLeft);
            var item19 = new UIDynamicElement("Item19", "null", new Coordinate(), ColorScale.White, _itemRef, Position.TopLeft, Position.TopLeft);
            var item20 = new UIDynamicElement("Item20", "null", new Coordinate(), ColorScale.White, _itemRef, Position.TopLeft, Position.TopLeft);
            var item21 = new UIDynamicElement("Item21", "null", new Coordinate(), ColorScale.White, _itemRef, Position.TopLeft, Position.TopLeft);
            var item22 = new UIDynamicElement("Item22", "null", new Coordinate(), ColorScale.White, _itemRef, Position.TopLeft, Position.TopLeft);
            var item23 = new UIDynamicElement("Item23", "null", new Coordinate(), ColorScale.White, _itemRef, Position.TopLeft, Position.TopLeft);

            _equipRef.AddElements(equip0, equip1, equip2, equip3, equip4, equip5, equip6);
            _itemRef.AddElements(item0, item1, item2, item3, item4, item5, item6, item7, item8, item9, item10, item11, item12, item13, item14, item15, item16, item17, item18, item19, item20, item21, item22, item23);
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
            
            //TODO: reload inventory display

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
