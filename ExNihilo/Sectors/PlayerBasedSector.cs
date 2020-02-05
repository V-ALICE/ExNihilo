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
    public abstract class PlayerBasedSector : Sector
    {
        protected CommandHandler _playerHandler;
        protected Vector2 _debugPosition;
        protected Point _lastMousePosition;
        protected World _world;

        protected bool _menuActive => _menuPoint != null;
        protected Menu _menuPoint;

        protected PlayerBasedSector(GameContainer container) : base(container)
        {
        }

/********************************************************************
------->Game loop
********************************************************************/
        public override void OnResize(GraphicsDevice graphicsDevice, Coordinate gameWindow)
        {
            _world?.OnResize(graphicsDevice, gameWindow);
            _debugPosition = new Vector2(1, 1 + TextDrawer.AlphaHeight + TextDrawer.LineSpacer);
            _menuPoint?.OnResize(graphicsDevice, gameWindow);
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
        }

        public override void LoadContent(GraphicsDevice graphicsDevice, ContentManager content)
        {
            _world.LoadContent(graphicsDevice, content);
        }

        public override void Update()
        {
            if (_menuActive) MenuHandler.UpdateInput();
            else if (!TypingKeyboard.Active)
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
