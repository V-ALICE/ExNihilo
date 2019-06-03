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
        public override void OnResize(GraphicsDevice graphicsDevice, Coordinate window)
        {
            _title.OnResize(graphicsDevice, window);
        }

        public override void Initialize()
        {
            handler = new CommandHandler();
            handler.Initialize(this);
            _title = new TitleMenu();
        }

        public override void LoadContent(GraphicsDevice graphicsDevice, ContentManager content)
        {
            _title.LoadContent(graphicsDevice, content);
        }

        public override void Update()
        {
            handler.UpdateInput();
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
        }

        public override void OnMoveMouse(Point point)
        {
            _title.OnMoveMouse(point);
        }

        public override void OnLeftClick(Point point)
        {
            _title.OnLeftClick(point);
        }

        public override void OnLeftRelease()
        {
            _title.OnLeftRelease();
        }
    }
}
