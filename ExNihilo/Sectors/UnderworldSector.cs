using System.Threading.Tasks;
using ExNihilo.Entity;
using ExNihilo.Menus;
using ExNihilo.Systems;
using ExNihilo.Util;
using ExNihilo.Util.Graphics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace ExNihilo.Sectors
{
    public class UnderworldSector : PlayerBasedSector
    {
        //TODO: add inventory, entityList

        //private Menu _inventoryMenu;

        private Level ActiveLevel => _world as Level;

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
            ActiveLevel.Update();
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
            //TODO: pack and unpack current seed and parallax 
        }

        public void StartNewGame(EntityContainer player)
        {
            ActiveLevel.Reset(player, new Coordinate(10, 10), new Coordinate(3, 10));
            SetFloor(1);
        }

        public void PrintMap(bool all=false)
        {
            ActiveLevel?.PrintMap(all);
        }

/********************************************************************
------->Parameter functions
********************************************************************/
        public void SetMapSize(int size)
        {
            ActiveLevel.ChangeMapSize(size);
        }

        public void SetFloor(int floor=-1)
        {
            ActiveLevel.DoGenerationQueue(Container, floor);
        }

        public void SetGenType(MapGenerator.Type type)
        {
            ActiveLevel.ChangeGenerationType(type);
        }

        public void SetParallax(int levels)
        {
            ActiveLevel.ChangeParallax(levels);
        }

        public void SetSeed(int seed)
        {
            ActiveLevel.ChangeSeed(seed);
        }

        public int GetSeed()
        {
            return ActiveLevel.GetSeed();
        }
    }
}
