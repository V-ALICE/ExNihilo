using ExNihilo.Systems;
using ExNihilo.Systems.Backend;
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
        private Coordinate _debugPosition, _centerScreen;
        private AnimatableTexture _loadingTexture;

        public LoadingSector(GameContainer container) : base(container)
        {
        }

/********************************************************************
------->Game loop
********************************************************************/
        public override void Initialize()
        {
            _loadingScale = 1;
            _timer = UniversalTime.NewTimer(true);
            _debugPosition = new Coordinate(1, 1 + TextDrawer.AlphaHeight + TextDrawer.LineSpacer);
        }

        public override void LoadContent(GraphicsDevice graphicsDevice, ContentManager content)
        {
            _loadingTexture = TextureLibrary.Lookup("UI/decor/Loading");
        }

        public override void Enter(Point point, Coordinate gameWindow)
        {
            OnResize(Container.GraphicsDevice, gameWindow);
            UniversalTime.ResetTimer(_timer);
            UniversalTime.TurnOnTimer(_timer);
        }

        public override void Leave(GameContainer.SectorID newSector)
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
            _loadingTexture.Draw(spriteBatch, _centerScreen, ColorScale.White, _loadingScale, 0, TextureUtilities.PositionType.Center);
            if (drawDebugInfo) DrawDebugInfo(spriteBatch);
        }

/********************************************************************
------->Game functions
********************************************************************/

        public override void OnExit()
        {
        }

        public override bool OnMoveMouse(Point point)
        {
            return false;
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
            _loadingScale = _rules.GetScale(gameWindow);
            _centerScreen = new Coordinate(gameWindow.X / 2, gameWindow.Y / 2);
        }


    }
}
