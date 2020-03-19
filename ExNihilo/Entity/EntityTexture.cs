using System.Collections.Generic;
using ExNihilo.Util;
using ExNihilo.Util.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ExNihilo.Entity
{
    public class EntityTexture
    {
        public enum State: sbyte
        {
            Up = 1, Down = 2, Left = 3, Right = 4,
            UpMoving = -1, DownMoving = -2, LeftMoving = -3, RightMoving = -4
        }

        private readonly Dictionary<State, AnimatableTexture> _textureStates;
        public State CurrentState { get; private set; }
        private Coordinate _previousPush;

        public AnimatableTexture CurrentTexture { get; private set; }

        public EntityTexture(GraphicsDevice graphics, Texture2D sheet, int staticColumnZeroIndex=1)
        {
            _previousPush = new Coordinate();
            CurrentState = State.Down;
            int rowHeight = sheet.Height / 4;
            int columnWidth = sheet.Width / 4;
            _textureStates = new Dictionary<State, AnimatableTexture>
            {
                {
                    State.Down,
                    TextureUtilities.GetSubTexture(graphics, sheet, new Rectangle(staticColumnZeroIndex * columnWidth, 0, columnWidth, rowHeight))
                },
                {
                    State.Left,
                    TextureUtilities.GetSubTexture(graphics, sheet, new Rectangle(staticColumnZeroIndex * columnWidth, rowHeight, columnWidth, rowHeight))
                },
                {
                    State.Right,
                    TextureUtilities.GetSubTexture(graphics, sheet, new Rectangle(staticColumnZeroIndex * columnWidth, 2 * rowHeight, columnWidth, rowHeight))
                },
                {
                    State.Up,
                    TextureUtilities.GetSubTexture(graphics, sheet, new Rectangle(staticColumnZeroIndex * columnWidth, 3 * rowHeight, columnWidth, rowHeight))
                },
                {
                    State.DownMoving,
                    new AnimatableTexture(TextureUtilities.GetSubTexture(graphics, sheet, new Rectangle(0, 0, sheet.Width, rowHeight)), 4, 4)
                },
                {
                    State.LeftMoving,
                    new AnimatableTexture(TextureUtilities.GetSubTexture(graphics, sheet, new Rectangle(0, rowHeight, sheet.Width, rowHeight)), 4, 4)
                },
                {
                    State.RightMoving,
                    new AnimatableTexture(TextureUtilities.GetSubTexture(graphics, sheet, new Rectangle(0, 2 * rowHeight, sheet.Width, rowHeight)), 4, 4)
                },
                {
                    State.UpMoving,
                    new AnimatableTexture(TextureUtilities.GetSubTexture(graphics, sheet, new Rectangle(0, 3 * rowHeight, sheet.Width, rowHeight)), 4, 4)
                }
            };

            CurrentTexture = _textureStates[CurrentState];
        }

        public void SetState(State state)
        {
            CurrentState = state;
            CurrentTexture = _textureStates[CurrentState];
        }
        public AnimatableTexture GetTexture(State state)
        {
            return _textureStates[state].Copy();
        }

        public void ApplyMovementUpdate(Coordinate push)
        {
            if (push.Equals(_previousPush)) return;
            if (_previousPush.Origin()) CurrentTexture.ResetFrame();
            _previousPush = push.Copy();

            if (!push.Origin())
            {
                //This implies that the player is moving and something changed
                if (push.X > 0) CurrentState = State.RightMoving;
                else if (push.X < 0) CurrentState = State.LeftMoving;
                else if (push.Y > 0) CurrentState = State.DownMoving;
                else if (push.Y < 0) CurrentState = State.UpMoving;
            }
            else if (CurrentState < 0)
            {
                //This implies that player just stopped moving
                switch (CurrentState)
                {
                    //This implies that player just stopped moving
                    case State.RightMoving:
                        CurrentState = State.Right;
                        break;
                    case State.LeftMoving:
                        CurrentState = State.Left;
                        break;
                    case State.DownMoving:
                        CurrentState = State.Down;
                        break;
                    case State.UpMoving:
                        CurrentState = State.Up;
                        break;
                }
            }

            CurrentTexture = _textureStates[CurrentState];
            
        }
    }
}
