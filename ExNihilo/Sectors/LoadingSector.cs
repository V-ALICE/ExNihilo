using ExNihilo.Systems;
using ExNihilo.Util;
using ExNihilo.Util.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace ExNihilo.Sectors
{
    public class LoadingSector : Sector
    {
        private int _timer;
        private float _loadingScale;
        private readonly ScaleRuleSet _rules = TextureLibrary.HalfScaleRuleSet;
        private Vector2 _loadingDrawPos, _loadingCyclePos, _debugPosition;
        private AnimatableTexture _loadingTexture, _loadingCycle;

        public LoadingSector(GameContainer container) : base(container)
        {
        }

/********************************************************************
------->Game loop
********************************************************************/
        public override void Initialize()
        {
            _loadingScale = 1;
            _loadingDrawPos = new Vector2();
            _timer = UniversalTime.NewTimer(true);
            _debugPosition = new Vector2(1, 1 + TextDrawer.AlphaHeight + TextDrawer.LineSpacer);
        }

        public override void LoadContent(GraphicsDevice graphicsDevice, ContentManager content)
        {
            _loadingTexture = TextureLibrary.Lookup("UI/decor/Loading");
            _loadingCycle = null;
        }

        public override void Enter(Point point, Coordinate gameWindow)
        {
            OnResize(Container.GraphicsDevice, gameWindow);
            UniversalTime.ResetTimer(_timer);
            UniversalTime.TurnOnTimer(_timer);
        }

        public override void Exit()
        {
            UniversalTime.TurnOffTimer(_timer);
        }

        public override void Update()
        {
            
        }

        protected override void DrawDebugInfo(SpriteBatch spriteBatch)
        {
            var text = "\n" + UniversalTime.GetCurrentTime(_timer).ToString("0.00");
            TextDrawer.DrawDumbText(spriteBatch, _debugPosition, text, 1, ColorScale.White);
        }

        public override void Draw(SpriteBatch spriteBatch, bool drawDebugInfo)
        {
            _loadingTexture.Draw(spriteBatch, _loadingDrawPos, ColorScale.White, _loadingScale);
            _loadingCycle?.Draw(spriteBatch, _loadingCyclePos, ColorScale.White, _loadingScale);
            if (drawDebugInfo) DrawDebugInfo(spriteBatch);
        }

/********************************************************************
------->Game functions
********************************************************************/
        public void AddNewCycle(AnimatableTexture cycle)
        {
            _loadingCycle = cycle;
        }

        public override void OnExit()
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

        public override void OnResize(GraphicsDevice graphicsDevice, Coordinate gameWindow)
        {
            //TODO: this seems to be wrong at some resolutions (might be all but more noticeable on some)
            _loadingScale = _rules.GetScale(gameWindow);
            _loadingDrawPos = new Vector2(gameWindow.X / 2, gameWindow.Y / 2) - TextureUtilities.GetOffset(TextureUtilities.PositionType.Center, _loadingTexture);
            if (_loadingCycle != null)
            {
                _loadingCyclePos = new Vector2(_loadingDrawPos.X + _loadingTexture.Width, _loadingDrawPos.Y + _loadingTexture.Height) -
                                   TextureUtilities.GetOffset(TextureUtilities.PositionType.BottomLeft, _loadingCycle);
            }
        }


    }
}
