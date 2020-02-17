using ExNihilo.Entity;
using ExNihilo.Menus;
using ExNihilo.Systems.Backend;
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
        private NoteMenu _voidMenu;
        private NoteMenu _fishMenu;

        public PlayerEntityContainer Player => _characterMenu.GetCurrentChar();

        public OuterworldSector(GameContainer container) : base(container)
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
            _voidMenu.OnResize(graphics, gameWindow);
        }

        public override void Leave(GameContainer.SectorID newSector)
        {
            if (newSector == GameContainer.SectorID.Loading)
                _world.Reset(Player, new Coordinate(10, 10), new Coordinate(3, 10));
        }

        public override void Initialize()
        {
            base.Initialize();
            _world = new World(16);
            _characterMenu = new CharacterMenu(Container, _world);
            _divineMenu = new DivineMenu(Container);
            _multiplayerMenu = new MultiplayerMenu(Container);
            _fishMenu = new NoteMenu(Container, "No fishing allowed", true);
            _voidMenu = new NoteMenu(Container, "Ready to Enter the Void?", false);
        }

        public override void LoadContent(GraphicsDevice graphicsDevice, ContentManager content)
        {
            base.LoadContent(graphicsDevice, content);
            _characterMenu.LoadContent(graphicsDevice, content);
            _divineMenu.LoadContent(graphicsDevice, content);
            _multiplayerMenu.LoadContent(graphicsDevice, content);
            _fishMenu.LoadContent(graphicsDevice, content);
            _voidMenu.LoadContent(graphicsDevice, content);

            _world.AddOverlay(content.Load<Texture2D>("World/tree"), 26, 5);
            _world.AddInteractive(new MenuInteractive("Tree", _divineMenu), 27, 8, 2);
            var river = new MenuInteractive("River", _fishMenu);
            _world.AddInteractive(river, 12, 6, 2);
            _world.AddInteractive(river, 15, 8, 1, 2);
            _world.AddInteractive(new MenuInteractive("Void", _voidMenu), 23, 21);
            _world.AddInteractive(new MenuInteractive("Pond", _characterMenu), 44, 3, 2, 2);
            _world.AddInteractive(new MenuInteractive("Island", _multiplayerMenu), 13, 43);
        }

        public override void Update()
        {
            base.Update();
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

        public override void OnLeftRelease(Point point)
        {
            if (_menuActive)
            {
                _menuPoint.OnLeftRelease(point);
                if (ReferenceEquals(_menuPoint, _voidMenu) && _voidMenu.Confirmed) //TODO: this is not a great way/place to detect this
                {
                    _voidMenu.BackOut();
                    Container.StartNewGame();
                }
                if (_menuPoint.Dead) _menuPoint = null;
            }
        }

        public override void Pack(PackedGame game)
        {
            _characterMenu.Pack(game);
            _divineMenu.Pack(game);
            _multiplayerMenu.Pack(game);
            _fishMenu.Pack(game);
            _voidMenu.Pack(game);
        }

        public override void Unpack(PackedGame game)
        {
            //LoadContent(Container.GraphicsDevice, Container.Content);
            
            _characterMenu.Unpack(game);
            _divineMenu.Unpack(game);
            _multiplayerMenu.Unpack(game);
            _fishMenu.Unpack(game);
            _voidMenu.Unpack(game);
            _world.Reset(Player, new Coordinate(10, 10), new Coordinate(3, 10));
            
        }
    }
}
