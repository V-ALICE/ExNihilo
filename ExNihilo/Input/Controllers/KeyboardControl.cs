using System.Collections.Generic;
using ExNihilo.Input.Commands;
using Microsoft.Xna.Framework.Input;

namespace ExNihilo.Input.Controllers
{
    public class KeyboardControl : IController
    {
        private KeyboardState _previousKeyboardState;
        private readonly CommandHandler _handler;
        //Only go through the keys that actually perform some action. Requires the list to be kept up to date
        private static readonly List<Keys> PossibleKeys = new List<Keys>
        {
            Keys.W, Keys.A, Keys.S, Keys.D,             //Cardinal movement
            Keys.Up, Keys.Down, Keys.Left, Keys.Right,  //Cardinal movement
            Keys.Escape, Keys.Tab,                      //Menus
            Keys.LeftShift, Keys.RightShift,            //Inventory menu (and running)
            Keys.Enter, Keys.E,                         //Interaction
            Keys.F1, Keys.F2,                           //Debug and fullscreen
            Keys.T, Keys.OemQuestion,                   //Chat
        };
        

        public KeyboardControl(CommandHandler handle)
        {
            _previousKeyboardState = Keyboard.GetState();
            _handler = handle;
        }

        public void UpdateInput()
        {
            var currentKeyboardState = Keyboard.GetState();
            foreach (var key in PossibleKeys)
            {
                if (currentKeyboardState.IsKeyDown(key) && !_previousKeyboardState.IsKeyDown(key))
                {
                    _handler.HandleCommand(key, true);
                }
                else if (_previousKeyboardState.IsKeyDown(key) && !currentKeyboardState.IsKeyDown(key))
                {
                    _handler.HandleCommand(key, false);
                }
            }
            _previousKeyboardState = currentKeyboardState;
        }
    }
}
