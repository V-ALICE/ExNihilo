using System.Collections.Generic;
using ExNihilo.Systems.Bases;
using Microsoft.Xna.Framework.Graphics;

namespace ExNihilo.Systems.Game.Items
{
    public class InstantItem : Item
    {
        public InstantItem(GraphicsDevice g, Texture2D sheet, string name, IEnumerable<string> block) : base(ItemType.Instant)
        {
            throw new System.NotImplementedException();
        }
    }
}
