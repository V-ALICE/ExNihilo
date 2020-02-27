using ExNihilo.Entity;
using ExNihilo.Systems.Backend;
using ExNihilo.Systems.Game;
using ExNihilo.Util;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace ExNihilo.Sectors
{
    public class VoidSector : PlayerBasedSector
    {
        private Level ActiveLevel => _world as Level;
        private PlayerEntityContainer Player => Container.Player;

        public VoidSector(GameContainer container) : base(container)
        {
        }

/********************************************************************
------->Game loop
********************************************************************/
        public override void Initialize()
        {
            base.Initialize();
            _world = new Level();
        }

        public override void Enter(Point point, Coordinate gameWindow)
        {
            _invRef.SetReference(Player);
            base.Enter(point, gameWindow);
        }

        public override void Leave(GameContainer.SectorID newSector)
        {
            if (newSector == GameContainer.SectorID.Outerworld)
            {
                ActiveLevel.Purge();
                Container.Pack();
                Player.Inventory.Dirty = true;
                AudioManager.PlaySong("Outerworld", true);
            }
        }

        protected override void DrawDebugInfo(SpriteBatch spriteBatch)
        {
            base.DrawDebugInfo(spriteBatch);
            ActiveLevel.DrawDebugInfo(spriteBatch, _debugPosition);
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
            ActiveLevel.Pack(game);
        }

        public override void Unpack(PackedGame game)
        {
            ActiveLevel.Unpack(game);
        }

        public void StartNewGame(int floor)
        {
            ActiveLevel.Reset(Player, new Coordinate(10, 10), new Coordinate(3, 10));
            SetFloor(floor);
        }

        public void PrintMap(bool all=false)
        {
            ActiveLevel?.PrintMap(all);
        }

/********************************************************************
------->Parameter functions
********************************************************************/

        public void SetFloor(int floor=-1)
        {
            ActiveLevel.DoGenerationQueue(Container, floor);
        }

        public void SetGenType(MapGenerator.Type type)
        {
            ActiveLevel.ChangeGenerationType(type);
        }

        public void SetTexturePack(string[] files)
        {
            ActiveLevel.ChangeTexturePack(files);
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
