using ExNihilo.Entity;
using ExNihilo.Menus;
using ExNihilo.Systems;
using ExNihilo.Util;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace ExNihilo.Sectors
{
    public class UnderworldSector : PlayerBasedSector
    {
        //TODO: add inventory, entityList

        //private Menu _inventoryMenu;

        private Level _activeLevel => _world as Level;

        public UnderworldSector(GameContainer container) : base(container)
        {
        }

/********************************************************************
------->Game loop
********************************************************************/
        public override void OnResize(GraphicsDevice graphicsDevice, Coordinate gameWindow)
        {
            base.OnResize(graphicsDevice, gameWindow);
            //_inventoryMenu.OnResize(graphicsDevice, gameWindow);
        }

        public override void Initialize()
        {
            base.Initialize();
            _world = new Level(16);
            //_inventoryMenu = new InventoryMenu(Container, _inventory);
        }

        public override void LoadContent(GraphicsDevice graphicsDevice, ContentManager content)
        {
            base.LoadContent(graphicsDevice, content);
            //_inventory.LoadContent(graphicsDevice, content);
        }

        public override void Update()
        {
            base.Update();
            _activeLevel.Update();
        }

        protected override void DrawDebugInfo(SpriteBatch spriteBatch)
        {
            base.DrawDebugInfo(spriteBatch);
        }

        public override void Draw(SpriteBatch spriteBatch, bool drawDebugInfo)
        {  
            _world.Draw(spriteBatch);
            _world.DrawOverlays(spriteBatch);
            _menuPoint?.Draw(spriteBatch);
            if (drawDebugInfo) DrawDebugInfo(spriteBatch);
        }

/********************************************************************
------->Game functions
********************************************************************/
        public override void Pack(PackedGame game)
        {
            //_inventoryMenu.Pack(game);
        }

        public override void Unpack(PackedGame game)
        {
            //_inventoryMenu.Unpack(game);
        }

        public bool StartNewGame(EntityContainer player, int seed)
        {
            _activeLevel.ChangeSeed(seed);
            _activeLevel.GenerateLevel(MapGenerator.Type.Random, 1);
            _activeLevel.Reset(player, new Coordinate(10, 10), new Coordinate(3, 10));
            return true;
        }
    }
}
