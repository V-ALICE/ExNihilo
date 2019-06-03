using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace ExNihilo.Input.Controllers
{
    public class MouseController
    {
        public struct MouseAction
        {
            public bool LeftDown, LeftUp, RightDown, RightUp, MiddleDown, MiddleUp;
            public bool PositionChange, StateChange;
            public Point MousePosition;

            public MouseAction(MouseState state, MouseState lastState)
            {
                MousePosition = state.Position;
                LeftDown = state.LeftButton == ButtonState.Pressed &&
                           lastState.LeftButton == ButtonState.Released;
                LeftUp = state.LeftButton == ButtonState.Released &&
                         lastState.LeftButton == ButtonState.Pressed;
                RightDown = state.RightButton == ButtonState.Pressed &&
                            lastState.RightButton == ButtonState.Released;
                RightUp = state.RightButton == ButtonState.Released &&
                          lastState.RightButton == ButtonState.Pressed;
                MiddleDown = state.MiddleButton == ButtonState.Pressed &&
                             lastState.MiddleButton == ButtonState.Released;
                MiddleUp = state.MiddleButton == ButtonState.Released &&
                           lastState.MiddleButton == ButtonState.Pressed;

                StateChange = LeftDown || LeftUp || RightDown || RightUp || MiddleDown || MiddleUp;
                PositionChange = state.Position != lastState.Position;
            }
        }

        public enum MouseButton
        {
            Left, Right, Middle
        }

        private MouseState _previousMouseState;

        public MouseController()
        {
            _previousMouseState = Mouse.GetState();
        }

        public MouseAction UpdateInput()
        {
            var currentMouseState = Mouse.GetState();
            var rtrn = new MouseAction(currentMouseState, _previousMouseState);
            _previousMouseState = currentMouseState;
            return rtrn;
        }
    }
}
