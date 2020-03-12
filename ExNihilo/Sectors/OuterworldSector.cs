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
        private DivineMenu _divineMenu;
        private MultiplayerMenu _multiplayerMenu;
        private NoteMenu _fishMenu;

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
            _divineMenu.OnResize(graphics, gameWindow);
            _multiplayerMenu.OnResize(graphics, gameWindow);
            _fishMenu.OnResize(graphics, gameWindow);
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
            _divineMenu = new DivineMenu(Container, () => { MenuPoint = null;});
            _multiplayerMenu = new MultiplayerMenu(Container, () => { MenuPoint = null; });
            _fishMenu = new NoteMenu(Container, "No fishing allowed", x => { MenuPoint = null; }, true);
        }

        public override void LoadContent(GraphicsDevice graphicsDevice, ContentManager content)
        {
            base.LoadContent(graphicsDevice, content);
            _characterMenu.LoadContent(graphicsDevice, content);
            _divineMenu.LoadContent(graphicsDevice, content);
            _multiplayerMenu.LoadContent(graphicsDevice, content);
            _fishMenu.LoadContent(graphicsDevice, content);

            World.AddOverlay(content.Load<Texture2D>("World/tree"), 26, 5);
            World.AddInteractive(new MenuInteractive("Tree", _divineMenu), 27, 8, 2);
            var river = new MenuInteractive("River", _fishMenu);
            World.AddInteractive(river, 12, 6, 2);
            World.AddInteractive(river, 15, 8, 1, 2);
            World.AddInteractive(new MenuInteractive("Void", StairMenu), 22, 21, 2, 2);
            World.AddInteractive(new MenuInteractive("Pond", _characterMenu), 44, 3, 2, 2);
            World.AddInteractive(new MenuInteractive("Island", _multiplayerMenu), 13, 43);
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
            _divineMenu.Pack(game);
            _multiplayerMenu.Pack(game);
            _fishMenu.Pack(game);
        }

        public override void Unpack(PackedGame game)
        {
            //LoadContent(Container.GraphicsDevice, Container.Content);
            
            _characterMenu.Unpack(game);
            _divineMenu.Unpack(game);
            _multiplayerMenu.Unpack(game);
            _fishMenu.Unpack(game);
            World.Reset(Player, new Coordinate(10, 10), new Coordinate(3, 10));
            
        }
    }
}
