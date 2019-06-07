using ExNihilo.Input.Commands;
using ExNihilo.Util;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace ExNihilo.Sectors
{
    public class UnderworldSector : Sector
    {       
        private CommandHandler _menuHandler;

        public UnderworldSector(GameContainer container) : base(container)
        {
        }

        /********************************************************************
        ------->Game loop
        ********************************************************************/
        public override void OnResize(GraphicsDevice graphicsDevice, Coordinate gameWindow)
        {
            
        }

        public override void Initialize()
        {
            Handler = new CommandHandler();
            Handler.Initialize(this);
            _menuHandler = new CommandHandler();
            _menuHandler.Initialize(this);
        }

        public override void LoadContent(GraphicsDevice graphicsDevice, ContentManager content)
        {
        }

        public override void Update()
        {
            Handler.UpdateInput();
            //_menuHandler.UpdateInput();
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
        public override void OnExit()
        {
            base.OnExit();
        }

        public override void OnMoveMouse(Point point)
        { 
        }

        public override bool OnLeftClick(Point point)
        {
            return false;
        }

        public override void OnLeftRelease()
        {
            
        }
    }
}
