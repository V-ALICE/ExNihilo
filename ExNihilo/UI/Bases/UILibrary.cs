using System.Collections.Generic;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using ExNihilo.Util.Graphics;
using Microsoft.Xna.Framework;

namespace ExNihilo.UI.Bases
{
    public abstract class UILibrary
    {
        private static bool _initialized;
        protected static Dictionary<string, Texture2D> TextureLookUp;

        public static void LoadLibrary(GraphicsDevice graphics, ContentManager content)
        {
            if (_initialized) return;
            _initialized = true;

            var sheet = content.Load<Texture2D>("UI/sheet");
            TextureLookUp = new Dictionary<string, Texture2D>
            {
                {"null", new Texture2D(graphics, 1, 1) },
                {"UI/BigButtonUp", TextureUtilities.GetSubTexture(graphics, sheet, new Rectangle(0, 52, 240, 52))},
                {"UI/BigButtonDown", TextureUtilities.GetSubTexture(graphics, sheet, new Rectangle(0, 0, 240, 52))}
            };
        }

    }
}
