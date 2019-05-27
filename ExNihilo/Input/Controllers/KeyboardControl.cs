using System.Collections.Generic;
using ExNihilo.Input.Commands;
using Microsoft.Xna.Framework.Input;

namespace ExNihilo.Input.Controllers
{
    public class KeyboardControl : IController
    {
        private KeyboardState PreviousKeyboardState;
        public CommandHandler Handler { get; set; }
        //Only go through the keys that actually perform some action. Requires the list to be kept up to date
        private readonly List<Keys> PossibleKeys = new List<Keys>
        {
            Keys.W, Keys.A, Keys.S, Keys.D,             //Cardinal movement
            Keys.Up, Keys.Down, Keys.Left, Keys.Right,  //Cardinal movement
            Keys.Escape, Keys.Tab,                      //Menus
            Keys.LeftShift, Keys.RightShift,            //Inventory menu (and running)
            Keys.Enter, Keys.E,                         //Interaction
            Keys.D0, Keys.F1, Keys.F2, Keys.F3,         //Debug
            Keys.T, Keys.OemQuestion,                   //Chat
        };
        

        public KeyboardControl(CommandHandler handle)
        {
            PreviousKeyboardState = Keyboard.GetState();
            Handler = handle;
        }

        public void UpdateInput()
        {
            var currentKeyboardState = Keyboard.GetState();
            foreach (var key in PossibleKeys)
            {
                if (currentKeyboardState.IsKeyDown(key) && !PreviousKeyboardState.IsKeyDown(key))
                {
                    Handler.HandleCommand(key, true);
                }
                else if (PreviousKeyboardState.IsKeyDown(key) && !currentKeyboardState.IsKeyDown(key))
                {
                    Handler.HandleCommand(key, false);
                }
            }
            PreviousKeyboardState = currentKeyboardState;
        }
    }
}
