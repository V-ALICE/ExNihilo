using System;
using System.Collections.Generic;
using ExNihilo.Util;
using ExNihilo.Util.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ExNihilo.Entity
{
    public class EntityTexture
    {
        public enum State
        {
            Up = 1, Down = 2, Left = 3, Right = 4,
            UpMoving = -1, DownMoving = -2, LeftMoving = -3, RightMoving = -4
        }

        private Dictionary<State, AnimatableTexture> _textureStates;
        private State _currentState;

        public AnimatableTexture CurrentTexture { get; private set; }

        public EntityTexture(GraphicsDevice graphics, Texture2D sheet, int staticColumnZeroIndex)
        {
            _currentState = State.Down;
            int rowHeight = sheet.Height / 4;
            int columnWidth = sheet.Width / 4;
            _textureStates = new Dictionary<State, AnimatableTexture>
            {
                {
                    State.Up,
                    TextureUtilities.GetSubTexture(graphics, sheet, new Rectangle(staticColumnZeroIndex * columnWidth, 0, columnWidth, rowHeight))
                },
                {
                    State.Right,
                    TextureUtilities.GetSubTexture(graphics, sheet, new Rectangle(staticColumnZeroIndex * columnWidth, rowHeight, columnWidth, rowHeight))
                },
                {
                    State.Down,
                    TextureUtilities.GetSubTexture(graphics, sheet, new Rectangle(staticColumnZeroIndex * columnWidth, 2 * rowHeight, columnWidth, rowHeight))
                },
                {
                    State.Left,
                    TextureUtilities.GetSubTexture(graphics, sheet, new Rectangle(staticColumnZeroIndex * columnWidth, 3 * rowHeight, columnWidth, rowHeight))
                },
                {State.UpMoving, TextureUtilities.GetSubTexture(graphics, sheet, new Rectangle(0, 0, sheet.Width, rowHeight))},
                {State.RightMoving, TextureUtilities.GetSubTexture(graphics, sheet, new Rectangle(0, rowHeight, sheet.Width, rowHeight))},
                {State.DownMoving, TextureUtilities.GetSubTexture(graphics, sheet, new Rectangle(0, 2 * rowHeight, sheet.Width, rowHeight))},
                {State.LeftMoving, TextureUtilities.GetSubTexture(graphics, sheet, new Rectangle(0, 3 * rowHeight, sheet.Width, rowHeight))}
            };

            CurrentTexture = _textureStates[_currentState];
        }

        public void ApplyMovementUpdate(Coordinate push)
        {
            if (!push.Origin())
            {
                //This implies that the player is moving and something changed
                if (push.X > 0) _currentState = State.RightMoving;
                else if (push.X < 0) _currentState = State.LeftMoving;
                else if (push.Y > 0) _currentState = State.DownMoving;
                else if (push.Y < 0) _currentState = State.UpMoving;
            }
            else if (_currentState < 0)
            {
                //This implies that player just stopped moving
                switch (_currentState)
                {
                    //This implies that player just stopped moving
                    case State.RightMoving:
                        _currentState = State.Right;
                        break;
                    case State.LeftMoving:
                        _currentState = State.Left;
                        break;
                    case State.DownMoving:
                        _currentState = State.Down;
                        break;
                    case State.UpMoving:
                        _currentState = State.Up;
                        break;
                }
            }

            CurrentTexture = _textureStates[_currentState];
        }
    }
}
