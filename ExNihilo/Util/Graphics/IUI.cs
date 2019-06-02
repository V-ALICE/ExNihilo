using Microsoft.Xna.Framework.Graphics;

namespace ExNihilo.Util.Graphics
{
    public interface IUI
    {
        void ResizeUI(GraphicsDevice graphics);

        void Draw(SpriteBatch spriteBatch);
    }
}
