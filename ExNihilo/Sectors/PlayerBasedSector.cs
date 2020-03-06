using System.Collections.Generic;
using ExNihilo.Entity;
using ExNihilo.Input.Commands;
using ExNihilo.Menus;
using ExNihilo.Systems;
using ExNihilo.Systems.Bases;
using ExNihilo.Systems.Game;
using ExNihilo.Util;
using ExNihilo.Util.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace ExNihilo.Sectors
{
    public abstract class PlayerBasedSector : Sector, ISuperPlayer, IPlayer
    {
        protected CommandHandler _playerHandler, _superPlayerHandler;
        protected Coordinate _debugPosition;
        protected Point _lastMousePosition;
        protected readonly World _world;
        protected bool _disableCollisions;
        protected float _systemPushSpeed = 1.0f;

        protected bool _menuActive => _menuPoint != null;
        protected Menu _menuPoint;

        protected InventoryMenu _invRef;

        public List<PlayerOverlay> OtherPlayers => _world.GetPlayers();

        protected PlayerBasedSector(GameContainer container, World world) : base(container)
        {
            _world = world;
        }

/********************************************************************
------->Game loop
********************************************************************/
        public override void OnResize(GraphicsDevice graphicsDevice, Coordinate gameWindow)
        {
            _world?.OnResize(graphicsDevice, gameWindow);
            _debugPosition = new Coordinate(1, 1 + TextDrawer.AlphaHeight + TextDrawer.LineSpacer);
            _menuPoint?.OnResize(graphicsDevice, gameWindow);
            if (!(_menuPoint is InventoryMenu)) _invRef.OnResize(graphicsDevice, gameWindow);
        }

        public override void Enter(Point point, Coordinate gameWindow)
        {
            OnResize(Container.GraphicsDevice, gameWindow);
        }

        public override void Initialize()
        {
            _invRef = new InventoryMenu(Container);
            _playerHandler = new CommandHandler();
            _playerHandler.InitializePlayer(this);
            _superPlayerHandler = new CommandHandler();
            _superPlayerHandler.InitializeSuperPlayer(this);
            _debugPosition = new Coordinate(1, 1 + TextDrawer.AlphaHeight + TextDrawer.LineSpacer);
            _lastMousePosition = new Point();
        }

        public override void LoadContent(GraphicsDevice graphicsDevice, ContentManager content)
        {
            _world.LoadContent(graphicsDevice, content);
            _invRef.LoadContent(graphicsDevice, content);
        }

        public override void Update()
        {
            if (TypingKeyboard.Active) return;
            _superPlayerHandler.UpdateInput();
            if (!_menuActive)
            {
                _world.ApplyPush(CurrentPush, _systemPushSpeed*CurrentPushMult, _disableCollisions);
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

        public override void ToggleTabMenu()
        {
            if (!_menuActive)
            {
                _menuPoint = _invRef;
                _invRef.Enter(_lastMousePosition);
                if (_menuActive) _world.Halt();
            }
            else if (_menuPoint is InventoryMenu)
            {
                _menuPoint.BackOut();
                if (_menuPoint.Dead) _menuPoint = null;
            }
        }

        public override bool OnMoveMouse(Point point)
        {
            _menuPoint?.OnMoveMouse(point);
            _lastMousePosition = point;
            return false;
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

        public override void Touch()
        {
            var obj = _world.CheckForInteraction();
            if (obj != null)
            {
                switch (obj)
                {
                    case BoxInteractive box:
                        _menuPoint = box.Access();
                        BoxMenu.Menu.Enter(_lastMousePosition, box);
                        if (_menuActive) _world.Halt();
                        break;
                    case MenuInteractive menu:
                        _menuPoint = menu.Access();
                        _menuPoint?.Enter(_lastMousePosition);
                        if (_menuActive) _world.Halt();
                        break;
                    case ActionInteractive action:
                        action.Function?.Invoke();
                        break;
                }
            }
        }

        public void ClearPlayers() { _world.ClearPlayers(); }
        public void ClearPlayers(long id) { _world.RemovePlayer(id); }
        public void UpdatePlayers(long id, string name, int[] charSet)
        {
            _world.AddPlayer(id, name, charSet);
        }

        public object[] GetStandardUpdateArray()
        {
            return _world.GetStandardUpdateArray();
        }

        public void ToggleCollisions(bool collisionOn)
        {
            _disableCollisions = !collisionOn;
        }

        public void SetSpeedMultiplier(float mult)
        {
            _systemPushSpeed = mult;
        }
    }
}
