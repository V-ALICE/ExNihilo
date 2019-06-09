using ExNihilo.Input.Commands;
using ExNihilo.Systems;
using ExNihilo.Util;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace ExNihilo.Sectors
{
    public class OuterworldSector : Sector
    {

        public OuterworldSector(GameContainer container) : base(container)
        {
        }

        /********************************************************************
        ------->Game loop
        ********************************************************************/
        public override void OnResize(GraphicsDevice graphicsDevice, Coordinate gameWindow)
        {
            
        }

        public override void Enter()
        {
            
        }

        public override void Initialize()
        {
            Handler = new CommandHandler();
            Handler.Initialize(this);
        }

        public override void LoadContent(GraphicsDevice graphicsDevice, ContentManager content)
        {
        }

        public override void Update()
        {
            Handler.UpdateInput();
        }

        protected override void DrawDebugInfo()
        {
        }

        public override void Draw(SpriteBatch spriteBatch, bool drawDebugInfo)
        {
            if (drawDebugInfo) DrawDebugInfo();
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
    }
}
