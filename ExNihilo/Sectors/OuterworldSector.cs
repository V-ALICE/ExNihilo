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

        private bool _menuActive => _menuPoint != null;
        private List<Menu> _menuSet;
        private Menu _menuPoint;
        
        public OuterworldSector(GameContainer container) : base(container)
        {
        }

/********************************************************************
------->Game loop
********************************************************************/
        public override void OnResize(GraphicsDevice graphics, Coordinate gameWindow)
        {
            _world.OnResize(graphics, gameWindow);
            foreach (var menu in _menuSet) menu.OnResize(graphics, gameWindow);
            _debugPosition = new Vector2(1, 1 + TextDrawer.AlphaHeight + TextDrawer.LineSpacer);
        }

        public override void Initialize()
        {
            _world = new World(16);
            MenuHandler = new CommandHandler();
            MenuHandler.Initialize(this, true);
            _playerHandler = new CommandHandler();
            _playerHandler.Initialize(this, false);
            _debugPosition = new Vector2(1, 1 + TextDrawer.AlphaHeight + TextDrawer.LineSpacer);
            _menuSet = new List<Menu>();
            _lastMousePosition = new Point();
        }

        public override void LoadContent(GraphicsDevice graphicsDevice, ContentManager content)
        {
            _world.LoadContent(graphicsDevice, content);
            _world.ForcePlayerTexture(TextureLibrary.Lookup("UI/button/RadioSelected"));

            var charMenu = new CharacterMenu(Container);
            var divineMenu = new DivineMenu(Container);
            var multiplayerMenu = new MultiplayerMenu(Container);
            var fishingNote = new NoteMenu(Container, "No fishing allowed", true);
            var voidNote = new NoteMenu(Container, "Void off limits", true);
            _menuSet.Add(charMenu);
            _menuSet.Add(divineMenu);
            _menuSet.Add(multiplayerMenu);
            _menuSet.Add(fishingNote);
            _menuSet.Add(voidNote);

            _world.AddOverlay(content.Load<Texture2D>("World/tree"), 26, 5);
            _world.AddInteractive(new MenuInteractive("Tree", divineMenu), 27, 8, 2);
            var river = new MenuInteractive("River", fishingNote);
            _world.AddInteractive(river, 12, 6, 2);
            _world.AddInteractive(river, 15, 8, 1, 2);
            _world.AddInteractive(new MenuInteractive("Void", voidNote), 23, 22);
            _world.AddInteractive(new MenuInteractive("Pond", charMenu), 44, 4, 2, 2);
            _world.AddInteractive(new MenuInteractive("Island", multiplayerMenu), 13, 43);

            foreach (var menu in _menuSet) menu.LoadContent(graphicsDevice, content);
        }

        public override void Update()
        {
            if (_menuActive) MenuHandler.UpdateInput();
            else
            {
                _playerHandler.UpdateInput();
                if (!CurrentPush.Origin()) _world.ApplyPush(CurrentPush, CurrentPushMult);
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
        }

        public override void Unpack(PackedGame game)
        {
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
                        break;
                    case ActionInteractive action:
                        action.Function?.Invoke();
                        break;
                }
            }
        }
    }
}
