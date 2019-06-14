using ExNihilo.Input.Commands;
using ExNihilo.Systems;
using ExNihilo.Util;
using ExNihilo.Util.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace ExNihilo.Sectors
{
    public class OuterworldSector : Sector
    {
        private CommandHandler _menuHandler;
        private bool _menuActive;
        private Texture2D _world;
        private Vector2 _currentWorldPosition, _debugPosition;
        private readonly ScaleRuleSet _worldRules = TextureLibrary.DefaultScaleRuleSet;
        private float _currentWorldScale;
        private PlayerOverlay _playerOverlay;
        private readonly int _timerID;

        public OuterworldSector(GameContainer container) : base(container)
        {
            _timerID = UniversalTime.NewTimer(false);
            UniversalTime.TurnOnTimer(_timerID);
        }

        /********************************************************************
        ------->Game loop
        ********************************************************************/
        public override void OnResize(GraphicsDevice graphicsDevice, Coordinate gameWindow)
        {
            _currentWorldScale = _worldRules.GetScale(gameWindow);
            _playerOverlay.OnResize(graphicsDevice, gameWindow);
            _debugPosition = new Vector2(1, 1 + TextDrawer.AlphaHeight + TextDrawer.LineSpacer);
        }

        public override void Initialize()
        {
            Handler = new CommandHandler();
            Handler.Initialize(this, false);
            _menuHandler = new CommandHandler();
            _menuHandler.Initialize(this, true);

            _debugPosition = new Vector2(1, 1 + TextDrawer.AlphaHeight + TextDrawer.LineSpacer);

            _currentWorldPosition = new Vector2();
            _currentWorldScale = 1;
            _playerOverlay = new PlayerOverlay("UI/button/RadioSelected");
        }

        public override void LoadContent(GraphicsDevice graphicsDevice, ContentManager content)
        {
            _world = content.Load<Texture2D>("world");
            _playerOverlay.LoadContent(graphicsDevice, content);
        }

        public override void Update()
        {
            if (_menuActive) _menuHandler.UpdateInput();
            else Handler.UpdateInput();
            
            _currentWorldPosition -= 100 * CurrentPushMult * (float)UniversalTime.GetLastTickTime(_timerID) * CurrentPush;
        }

        protected override void DrawDebugInfo(SpriteBatch spriteBatch)
        {
            var text = "World Pos: X:" + _currentWorldPosition.X + " Y:" + _currentWorldPosition.Y + "\nPush Vec:  " + CurrentPush;
            TextDrawer.DrawDumbText(spriteBatch, _debugPosition, text, 1, ColorScale.White);
        }

        public override void Draw(SpriteBatch spriteBatch, bool drawDebugInfo)
        {
            spriteBatch.Draw(_world, _currentWorldPosition, null, Color.White, 0, Vector2.Zero, _currentWorldScale, SpriteEffects.None, 0);
            _playerOverlay.Draw(spriteBatch);
            if (drawDebugInfo) DrawDebugInfo(spriteBatch);
        }

        /********************************************************************
        ------->Game functions
        ********************************************************************/

        protected override void Exit()
        {
        }

        public override void OnMoveMouse(Point point)
        {
        }

        public override bool OnLeftClick(Point point)
        {
            return false;
        }

        public override void OnLeftRelease(Point point)
        {
            
        }

        public override void Pack(PackedGame game)
        {
        }

        public override void Unpack(PackedGame game)
        {
        }

        public override void Touch()
        {

        }
    }
}
