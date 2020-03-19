using ExNihilo.Entity;
using ExNihilo.Menus;
using ExNihilo.Systems.Backend;
using ExNihilo.Systems.Backend.Network;
using ExNihilo.Systems.Bases;
using ExNihilo.Systems.Game;
using ExNihilo.Util;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace ExNihilo.Sectors
{
    public class OuterworldSector : PlayerBasedSector
    {
        //Preset Menus (since they never change)
        private CharacterMenu _characterMenu;
        private StorageMenu _storageMenu;
        private MultiplayerMenu _multiplayerMenu;

        public PlayerEntityContainer Player => _characterMenu.GetCurrentChar();

        public OuterworldSector(GameContainer container) : base(container, new World(16))
        {
        }

/********************************************************************
------->Game loop
********************************************************************/
        public override void OnResize(GraphicsDevice graphics, Coordinate gameWindow)
        {
            base.OnResize(graphics, gameWindow);
            _characterMenu.OnResize(graphics, gameWindow);
            _storageMenu.OnResize(graphics, gameWindow);
            _multiplayerMenu.OnResize(graphics, gameWindow);
        }

        public override void Enter(Point point, Coordinate gameWindow)
        {
            InvRef.SetReference(Player);
            base.Enter(point, gameWindow);
        }

        public override void Leave(GameContainer.SectorID newSector)
        {
            if (newSector == GameContainer.SectorID.Loading)
            {
                //Assumed to be entering void
                World.Reset(Player, new Coordinate(10, 10), new Coordinate(3, 10));
                AudioManager.PlaySong("Void", true);
            }
        }

        public override void Initialize()
        {
            void CharChange()
            {
                InvRef.SetReference(Player);
                MenuPoint = null;
            }

            base.Initialize();
            _characterMenu = new CharacterMenu(Container, CharChange, World);
            _storageMenu = new StorageMenu(Container, () => { MenuPoint = null;});
            _multiplayerMenu = new MultiplayerMenu(Container, () => { MenuPoint = null; });
        }

        public override void LoadContent(GraphicsDevice graphicsDevice, ContentManager content)
        {
            base.LoadContent(graphicsDevice, content);
            _characterMenu.LoadContent(graphicsDevice, content);
            _storageMenu.LoadContent(graphicsDevice, content);
            _multiplayerMenu.LoadContent(graphicsDevice, content);

            World.AddOverlay(content.Load<Texture2D>("World/ship_overlay"), 0, 0);
            //World.AddOverlay(content.Load<Texture2D>("World/ship_overlay_alt"), 0, 0);
            World.AddInteractive(new MenuInteractive("Storage", _storageMenu), 6, 6, 2, 2);
            World.AddInteractive(new MenuInteractive("Void", StairMenu), 6, 22, 2, 2);
            World.AddInteractive(new MenuInteractive("Cabin", _characterMenu), 9, 19);
            World.AddInteractive(new MenuInteractive("Wheel", _multiplayerMenu), 8, 13, 2, 1);
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

        public override void CheckNetwork(bool ending)
        {
            base.CheckNetwork(ending);
            if (ReferenceEquals(_multiplayerMenu, MenuPoint)) _multiplayerMenu.UpdateDisplay(ending);
        }

        public override void OnLeftRelease(Point point)
        {
            if (MenuActive)
            {
                MenuPoint.OnLeftRelease(point);
            }
        }

        public override void Pack(PackedGame game)
        {
            _characterMenu.Pack(game);
            _storageMenu.Pack(game);
            _multiplayerMenu.Pack(game);
        }

        public override void Unpack(PackedGame game)
        {
            //LoadContent(Container.GraphicsDevice, Container.Content);
            
            _characterMenu.Unpack(game);
            _storageMenu.Unpack(game);
            _multiplayerMenu.Unpack(game);
            World.Reset(Player, new Coordinate(10, 10), new Coordinate(3, 10));
            
        }
    }
}
