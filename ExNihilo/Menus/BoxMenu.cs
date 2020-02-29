
using ExNihilo.Util;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace ExNihilo.Menus
{
    public class BoxMenu : Menu
    {
        public static BoxMenu Menu { get; private set; }
        public static void CreateMenu(GameContainer container)
        {
            Menu = new BoxMenu(container);
        }

        private BoxMenu(GameContainer container) : base(container)
        {
        }

        public override void LoadContent(GraphicsDevice graphics, ContentManager content)
        {
            throw new System.NotImplementedException();
        }

        public override void OnResize(GraphicsDevice graphics, Coordinate gameWindow)
        {
            throw new System.NotImplementedException();
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            throw new System.NotImplementedException();
        }

        public override bool OnMoveMouse(Point point)
        {
            throw new System.NotImplementedException();
        }

        public override bool OnLeftClick(Point point)
        {
            throw new System.NotImplementedException();
        }

        public override void OnLeftRelease(Point point)
        {
            throw new System.NotImplementedException();
        }

        public override void ReceiveInput(string input)
        {
            throw new System.NotImplementedException();
        }
    }
}
