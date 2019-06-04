using System.Collections.Generic;
using ExNihilo.Input.Commands;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace ExNihilo.Input.Controllers
{
    internal class ControllerControl : IController
    {
        private GamePadState _previousControllerState;
        private readonly CommandHandler _handler;
        //Only go through the keys that actually perform some action. Requires the list to be kept up to date
        private static readonly List<Buttons> _possibleButtons = new List<Buttons>
        {
            Buttons.LeftThumbstickUp, Buttons.LeftThumbstickDown,                   //Cardinal movement
            Buttons.LeftThumbstickRight, Buttons.LeftThumbstickLeft,                //Cardinal movement
            Buttons.DPadUp, Buttons.DPadDown, Buttons.DPadLeft, Buttons.DPadRight,  //Cardinal movement
            Buttons.A,                                      //Interaction
            Buttons.Y, Buttons.X,                           //Running
            Buttons.Start, Buttons.Back,                    //Menus
            Buttons.LeftShoulder, Buttons.RightShoulder,    //Inventory menus
        };

        public ControllerControl(CommandHandler handle)
        {
            _previousControllerState = GamePad.GetState(PlayerIndex.One);
            _handler = handle;
        }

        public void UpdateInput()
        {
            var currentControllerState = GamePad.GetState(PlayerIndex.One);
            if (currentControllerState.IsConnected)
            {
                foreach (var button in _possibleButtons)
                {
                    if (currentControllerState.IsButtonDown(button) && !_previousControllerState.IsButtonDown(button))
                    {
                        _handler.HandleCommand(button, true);
                    }
                    else if (!currentControllerState.IsButtonDown(button) && _previousControllerState.IsButtonDown(button))
                    {
                        _handler.HandleCommand(button, false);
                    }
                }
            }
            _previousControllerState = currentControllerState;
        }
    }
}
