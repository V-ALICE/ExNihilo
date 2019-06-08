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
        public static Dictionary<string, Texture2D> TextureLookUp;
        public static Dictionary<string, byte[]> TextureAlphaLookUp;
        public static ScaleRuleSet DefaultScaleRuleSet, ReducedScaleRuleSet, UnscaledScaleRuleSet, HalfScaleRuleSet;

        public static void LoadRuleSets()
        {
            DefaultScaleRuleSet = new ScaleRuleSet();
            DefaultScaleRuleSet.AddRules
            (
                new ScaleRule(1, 1400, 1000),
                new ScaleRule(2, 2100, 1500),
                new ScaleRule(3, 2800, 2000),
                new ScaleRule(4, ScaleRule.MAX_X, ScaleRule.MAX_Y)
            );

            ReducedScaleRuleSet = new ScaleRuleSet();
            ReducedScaleRuleSet.AddRules
            (
                new ScaleRule(1, 2100, 1500),
                new ScaleRule(2, ScaleRule.MAX_X, ScaleRule.MAX_Y)
            );

            UnscaledScaleRuleSet = new ScaleRuleSet();
            UnscaledScaleRuleSet.AddRules
            (
                new ScaleRule(1, ScaleRule.MAX_X, ScaleRule.MAX_Y)
            );

            HalfScaleRuleSet = new ScaleRuleSet();
            HalfScaleRuleSet.AddRules
            (
                new ScaleRule(0.5f, 1400, 1000),
                new ScaleRule(1, 2100, 1500),
                new ScaleRule(1.5f, 2800, 2000),
                new ScaleRule(2, ScaleRule.MAX_X, ScaleRule.MAX_Y)
            );
        }

        public static void LoadLibrary(GraphicsDevice graphics, ContentManager content)
        {
            if (_initialized) return;
            _initialized = true;

            //Eventually this could be from a json or something
            var sheet = content.Load<Texture2D>("UI/sheet");
            TextureLookUp = new Dictionary<string, Texture2D>
            {
                {"null", new Texture2D(graphics, 1, 1)},
                {"UI/Title", content.Load<Texture2D>("UI/title")},
                {"UI/SmallButton", TextureUtilities.GetSubTexture(graphics, sheet, new Rectangle(0, 104, 160, 52))},
                {"UI/SmallButtonDown", TextureUtilities.GetSubTexture(graphics, sheet, new Rectangle(160, 104, 160, 52))},
                {"UI/BigButton", TextureUtilities.GetSubTexture(graphics, sheet, new Rectangle(0, 52, 240, 52))},
                {"UI/BigButtonDown", TextureUtilities.GetSubTexture(graphics, sheet, new Rectangle(0, 0, 240, 52))},
                {"UI/GreenBulb", TextureUtilities.GetSubTexture(graphics, sheet, new Rectangle(240, 0, 28, 28))},
                {"UI/BlueBulb", TextureUtilities.GetSubTexture(graphics, sheet, new Rectangle(268, 0, 28, 28))},
                {"UI/RedBulb", TextureUtilities.GetSubTexture(graphics, sheet, new Rectangle(296, 0, 28, 28))},
                {"UI/BlackBulb", TextureUtilities.GetSubTexture(graphics, sheet, new Rectangle(324, 0, 28, 28))},
                {"UI/GreenBulbHover", TextureUtilities.GetSubTexture(graphics, sheet, new Rectangle(240, 28, 28, 28))},
                {"UI/BlueBulbHover", TextureUtilities.GetSubTexture(graphics, sheet, new Rectangle(268, 28, 28, 28))},
                {"UI/RedBulbHover", TextureUtilities.GetSubTexture(graphics, sheet, new Rectangle(296, 28, 28, 28))},
                {"UI/BlackBulbHover", TextureUtilities.GetSubTexture(graphics, sheet, new Rectangle(324, 28, 28, 28))},
                {"UI/GreenBulbDown", TextureUtilities.GetSubTexture(graphics, sheet, new Rectangle(240, 56, 28, 28))},
                {"UI/BlueBulbDown", TextureUtilities.GetSubTexture(graphics, sheet, new Rectangle(268, 56, 28, 28))},
                {"UI/RedBulbDown", TextureUtilities.GetSubTexture(graphics, sheet, new Rectangle(296, 56, 28, 28))},
                {"UI/BlackBulbDown", TextureUtilities.GetSubTexture(graphics, sheet, new Rectangle(324, 56, 28, 28))},
                {"UI/RadioUnselected", TextureUtilities.GetSubTexture(graphics, sheet, new Rectangle(352, 0, 24, 24))},
                {"UI/RadioSelected", TextureUtilities.GetSubTexture(graphics, sheet, new Rectangle(376, 0, 24, 24))},
                {"UI/PipBarBlue", TextureUtilities.GetSubTexture(graphics, sheet, new Rectangle(352, 24, 16, 24))},
                {"UI/PipBarRed", TextureUtilities.GetSubTexture(graphics, sheet, new Rectangle(368, 24, 16, 24))},
                {"UI/PipBarGreen", TextureUtilities.GetSubTexture(graphics, sheet, new Rectangle(384, 24, 16, 24))},
            };
            sheet.Dispose();

            TextureAlphaLookUp = new Dictionary<string, byte[]>();
            foreach (var texture in TextureLookUp)
            {
                var data = new Color[texture.Value.Width * texture.Value.Height];
                texture.Value.GetData(data);
                var alpha = new byte[data.Length];
                for (int i = 0; i < data.Length; i++) alpha[i] = data[i].A;
                TextureAlphaLookUp.Add(texture.Key, alpha);
            }
        }

    }
}
