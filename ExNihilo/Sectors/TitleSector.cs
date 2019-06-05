using ExNihilo.Input.Commands;
using ExNihilo.Menus;
using ExNihilo.Util;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace ExNihilo.Sectors
{
    public class TitleSector : Sector
    {
        private TitleMenu _title;

        /********************************************************************
        ------->Game loop
        ********************************************************************/
        public override void OnResize(GraphicsDevice graphicsDevice, Coordinate gameWindow, Coordinate subWindow, Vector2 origin)
        {
            _title.OnResize(graphicsDevice, gameWindow, subWindow, origin);
        }

        public override void Initialize()
        {
            Handler = new CommandHandler();
            Handler.Initialize(this);
            _title = new TitleMenu();
        }

        public override void LoadContent(GraphicsDevice graphicsDevice, ContentManager content)
        {
            _title.LoadContent(graphicsDevice, content);
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
            _title.Draw(spriteBatch);
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
            _title.OnMoveMouse(point);
        }

        public override bool OnLeftClick(Point point)
        {
            return _title.OnLeftClick(point);
        }

        public override void OnLeftRelease()
        {
            _title.OnLeftRelease();
        }
    }
}
