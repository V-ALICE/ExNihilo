using System.Collections.Generic;
using ExNihilo.Entity;
using ExNihilo.Menus;
using ExNihilo.Systems.Backend;
using ExNihilo.Systems.Backend.Network;
using ExNihilo.Systems.Game;
using ExNihilo.Util;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace ExNihilo.Sectors
{
    public class VoidSector : PlayerBasedSector
    {
        private Level ActiveLevel => World as Level;
        private PlayerEntityContainer Player => Container.Player;

        public bool VoidIsActive;
        public static int Seed;

        public VoidSector(GameContainer container) : base(container, new Level())
        {
        }

/********************************************************************
------->Game loop
********************************************************************/

        public override void OnResize(GraphicsDevice graphicsDevice, Coordinate gameWindow)
        {
            base.OnResize(graphicsDevice, gameWindow);
            if (!(MenuPoint is BoxMenu)) BoxMenu.Menu.OnResize(graphicsDevice, gameWindow);
        }

        public override void Enter(Point point, Coordinate gameWindow)
        {
            //VoidIsActive = true;
            InvRef.SetReference(Player);
            BoxMenu.Menu.SetReference(Player);
            base.Enter(point, gameWindow);
        }

        public override void LoadContent(GraphicsDevice graphicsDevice, ContentManager content)
        {
            base.LoadContent(graphicsDevice, content);
            StairMenu.LoadContent(graphicsDevice, content);
        }

        public void Return()
        {
            VoidIsActive = false;
            ActiveLevel.Purge();
            Container.Pack();
            Player.Inventory.Dirty = true;
            AudioManager.PlaySong("Outerworld", true);
        }

        protected override void DrawDebugInfo(SpriteBatch spriteBatch)
        {
            base.DrawDebugInfo(spriteBatch);
            ActiveLevel.DrawDebugInfo(spriteBatch, DebugPosition);
        }

        public override void Draw(SpriteBatch spriteBatch, bool drawDebugInfo)
        {  
            World.Draw(spriteBatch);
            World.DrawOverlays(spriteBatch);
            MenuPoint?.Draw(spriteBatch);
            if (drawDebugInfo) DrawDebugInfo(spriteBatch);
        }

/********************************************************************
------->Game functions
********************************************************************/
        public override void Pack(PackedGame game)
        {
            game.Seed = Seed;
            game.InVoid = VoidIsActive;
            ActiveLevel.Pack(game);
        }

        public override void Unpack(PackedGame game)
        {
            Seed = game.Seed;
            VoidIsActive = game.InVoid;
            ActiveLevel.Unpack(game);
        }

        public void Descend(int seed, int itemSeed, int floor, List<PlayerOverlay> refList)
        {
            ActiveLevel.ChangeSeed(seed);
            ActiveLevel.Reset(Player, new Coordinate(10, 10), new Coordinate(3, 10));
            if (!VoidIsActive)
            {
                ActiveLevel.ClearPlayers();
                if (refList != null)
                {
                    foreach (var player in refList) ActiveLevel.AddPlayerRef(player);
                }
            }
            ActiveLevel.DoGenerationQueue(Container, floor, itemSeed, StairMenu);
            VoidIsActive = true;
        }

        public void PrintMap(bool all=false)
        {
            ActiveLevel?.PrintMap(all);
        }

/********************************************************************
        ------->Parameter functions
        ********************************************************************/

        public void ForceRemoveFromBox(int boxNum, int itemId)
        {
            ActiveLevel.ForceRemoveFromBox(boxNum, itemId);
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

    }
}
