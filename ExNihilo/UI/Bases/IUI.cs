using ExNihilo.Util;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace ExNihilo.UI.Bases
{
    public interface IUI
    {
        void LoadContent(GraphicsDevice graphics, ContentManager content);

        void OnResize(GraphicsDevice graphics, Coordinate window, Vector2 origin);

        void Draw(SpriteBatch spriteBatch);
    }
}
