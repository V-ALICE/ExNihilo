using System.Collections.Generic;
using ExNihilo.Input.Commands;
using ExNihilo.Menus;
using ExNihilo.Systems;
using ExNihilo.Systems.Bases;
using ExNihilo.Util;
using ExNihilo.Util.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace ExNihilo.Sectors
{
    public class OuterworldSector : Sector
    {
        private CommandHandler _playerHandler;
        private Vector2 _debugPosition;
        private Point _lastMousePosition;
        private World _world;

        private CharacterMenu _characterMenu;
        private DivineMenu _divineMenu;
        private MultiplayerMenu _multiplayerMenu;
        private NoteMenu _voidMenu;
        private NoteMenu _fishMenu;

        private bool _menuActive => _menuPoint != null;
        private Menu _menuPoint;
        
        public OuterworldSector(GameContainer container) : base(container)
        {
        }

/********************************************************************
------->Game loop
********************************************************************/
        public override void OnResize(GraphicsDevice graphics, Coordinate gameWindow)
        {
            _world?.OnResize(graphics, gameWindow);
            _debugPosition = new Vector2(1, 1 + TextDrawer.AlphaHeight + TextDrawer.LineSpacer);

            _characterMenu.OnResize(graphics, gameWindow);
            _divineMenu.OnResize(graphics, gameWindow);
            _multiplayerMenu.OnResize(graphics, gameWindow);
            _fishMenu.OnResize(graphics, gameWindow);
            _voidMenu.OnResize(graphics, gameWindow);
        }

        public override void Enter(Point point, Coordinate gameWindow)
        {
            OnResize(Container.GraphicsDevice, gameWindow);
        }

        public override void Initialize()
        {
            MenuHandler = new CommandHandler();
            MenuHandler.Initialize(this, true);
            _playerHandler = new CommandHandler();
            _playerHandler.Initialize(this, false);
            _debugPosition = new Vector2(1, 1 + TextDrawer.AlphaHeight + TextDrawer.LineSpacer);
            _lastMousePosition = new Point();

            _world = new World(16);
            _characterMenu = new CharacterMenu(Container, _world);
            _divineMenu = new DivineMenu(Container);
            _multiplayerMenu = new MultiplayerMenu(Container);
            _fishMenu = new NoteMenu(Container, "No fishing allowed", true);
            _voidMenu = new NoteMenu(Container, "Void off limits", true);
        }

        public override void LoadContent(GraphicsDevice graphicsDevice, ContentManager content)
        {
            _characterMenu.LoadContent(graphicsDevice, content);
            _divineMenu.LoadContent(graphicsDevice, content);
            _multiplayerMenu.LoadContent(graphicsDevice, content);
            _fishMenu.LoadContent(graphicsDevice, content);
            _voidMenu.LoadContent(graphicsDevice, content);

            _world.LoadContent(graphicsDevice, content);
            _world.AddOverlay(content.Load<Texture2D>("World/tree"), 26, 5);
            _world.AddInteractive(new MenuInteractive("Tree", _divineMenu), 27, 8, 2);
            var river = new MenuInteractive("River", _fishMenu);
            _world.AddInteractive(river, 12, 6, 2);
            _world.AddInteractive(river, 15, 8, 1, 2);
            _world.AddInteractive(new MenuInteractive("Void", _voidMenu), 23, 22);
            _world.AddInteractive(new MenuInteractive("Pond", _characterMenu), 44, 3, 2, 2);
            _world.AddInteractive(new MenuInteractive("Island", _multiplayerMenu), 13, 43);
        }

        public override void Update()
        {
            if (_menuActive) MenuHandler.UpdateInput();
            else
            {
                _world.ApplyPush(CurrentPush, CurrentPushMult);
                _playerHandler.UpdateInput();
            }
        }

        protected override void DrawDebugInfo(SpriteBatch spriteBatch)
        {
            var text = _world + "\nPush Vec:  " + CurrentPush;
            TextDrawer.DrawDumbText(spriteBatch, _debugPosition, text, 1, ColorScale.White);
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
        public override void BackOut()
        {
            if (_menuActive)
            {
                _menuPoint.BackOut();
                if (_menuPoint.Dead) _menuPoint = null;
            }
            else base.BackOut();
        }

        public override void OnMoveMouse(Point point)
        {
            _menuPoint?.OnMoveMouse(point);
            _lastMousePosition = point;
        }

        public override bool OnLeftClick(Point point)
        {
            return _menuPoint?.OnLeftClick(point) ?? false;
        }

        public override void OnLeftRelease(Point point)
        {
            if (_menuActive)
            {
                _menuPoint.OnLeftRelease(point);
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
            _world.Reset(_characterMenu.GetCurrentChar(), new Coordinate(10, 10), new Coordinate(3, 10));
            
        }

        public override void Touch()
        {
            var obj = _world.CheckForInteraction();
            if (obj != null)
            {
                switch (obj)
                {
                    case MenuInteractive menu:
                        _menuPoint = menu.InteractionMenu;
                        _menuPoint?.Enter(_lastMousePosition);
                        if (_menuActive) _world.Halt();
                        break;
                    case ActionInteractive action:
                        action.Function?.Invoke();
                        break;
                }
            }
        }
    }
}
