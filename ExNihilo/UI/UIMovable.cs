using ExNihilo.Util;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ExNihilo.UI
{
    public class UIMovable : UIClickable
    {
        protected Point Anchor;
        protected Vector2 ShiftedPos;
        protected bool Ghosting;

        protected Coordinate LastWindow;
        protected Vector2 LastOrigin;

        public UIMovable(string path, Vector2 relPos, string altPath = "", bool ghost = false, PositionType t = PositionType.Center,
            float multiplier = 1.0f) : base(path, relPos, altPath, t, multiplier)
        {
            Ghosting = ghost;
        }

        public override void OnResize(GraphicsDevice graphics, Coordinate window, Vector2 origin)
        {
            if (!Loaded) return;
            base.OnResize(graphics, window, origin);

            //Save these for bounds checking (so elements can't be dragged off screen and lost)
            LastWindow = window.Copy();
            LastOrigin = Utilities.Copy(origin);

            //Make sure moved elements don't go out of bounds
            var opp = LastWindow + LastOrigin;
            var newX = MathHelper.Clamp(Pos.X + TextureOffset.X, LastOrigin.X, opp.X);
            var newY = MathHelper.Clamp(Pos.Y + TextureOffset.Y, LastOrigin.Y, opp.Y);
            Pos = new Vector2(newX, newY) - TextureOffset;
        }

        public override void Draw(SpriteBatch spriteBatch, Color color)
        {
            if (!Loaded) return;

            if (Activated)
            {
                if (Ghosting)
                {
                    var ghost = new Color(color.R, color.G, color.B, color.A / 4);
                    spriteBatch.Draw(AltTexture ?? Texture, Pos, null, color, 0, Vector2.Zero, SizeMult, SpriteEffects.None, 0);
                    spriteBatch.Draw(Texture, Pos + ShiftedPos, null, ghost, 0, Vector2.Zero, SizeMult, SpriteEffects.None, 0);
                }
                else
                {
                    spriteBatch.Draw(Texture, Pos + ShiftedPos, null, color, 0, Vector2.Zero, SizeMult, SpriteEffects.None, 0);
                }
            }
            else
            {
                spriteBatch.Draw(Texture, Pos, null, color, 0, Vector2.Zero, SizeMult, SpriteEffects.None, 0);
            }
        }

        public override void OnMoveMouse(Point point)
        {
            if (Activated)
            {
                var opp = LastWindow + LastOrigin;
                var newX = MathHelper.Clamp(Pos.X + TextureOffset.X + point.X - Anchor.X, LastOrigin.X, opp.X);
                var newY = MathHelper.Clamp(Pos.Y + TextureOffset.Y + point.Y - Anchor.Y, LastOrigin.Y, opp.Y);
                ShiftedPos = new Vector2(newX, newY) - Pos - TextureOffset;
            }
        }

        public override void OnLeftClick(Point point)
        {
            if (IsOver(point))
            {
                Activated = true;
                Anchor = point;
            }
        }

        public override void OnLeftRelease()
        {
            if (Activated)
            {
                Activated = false;
                Pos += ShiftedPos;
                ShiftedPos = new Vector2();

                var relX = LastWindow.X == 0 ? 0 : (Pos.X - LastOrigin.X + TextureOffset.X) / LastWindow.X;
                var relY = LastWindow.Y == 0 ? 0 : (Pos.Y - LastOrigin.Y + TextureOffset.Y) / LastWindow.Y;
                PosRel = new Vector2(relX, relY);
            }
        }
    }
}
