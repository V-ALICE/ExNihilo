using ExNihilo.Util;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace ExNihilo.UI
{
    public interface IUI
    {
        void LoadContent(GraphicsDevice graphics, ContentManager content);

        void OnResize(GraphicsDevice graphics, Coordinate window);

        void Draw(SpriteBatch spriteBatch);
    }
}
