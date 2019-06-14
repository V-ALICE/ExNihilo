using System.Collections.Generic;
using ExNihilo.Input.Commands.Types;
using ExNihilo.Input.Controllers;
using ExNihilo.Sectors;
using ExNihilo.Systems;
using ExNihilo.Util;
using Microsoft.Xna.Framework.Input;

namespace ExNihilo.Input.Commands
{
    public class CommandHandler
    {

        private readonly List<IController> _controllers;
        private readonly List<KeyBlock> _bucket;
        private readonly int _timerID;

        public CommandHandler()
        {
            _timerID = UniversalTime.NewTimer(true);
            UniversalTime.TurnOnTimer(_timerID);
            _controllers = new List<IController> { new KeyboardControl(this), new ControllerControl(this) };
            _bucket = new List<KeyBlock>();
        }

        public void Initialize(GameContainer game, bool super)
        {
            if (_bucket.Count > 0) return;

            if (super)
            {
                _bucket.Add(new KeyBlock(new BackspaceMessage(game), new UnbackspaceMessage(game), false, Keys.Back));
                _bucket.Add(new KeyBlock(new BackOutCommand(game), false, Keys.Escape, Buttons.Back));
                _bucket.Add(new KeyBlock(new ToggleDebugUI(game), false, Keys.F1));
                _bucket.Add(new KeyBlock(new ToggleFullScreen(game), false, Keys.F2));
            }
            else
            {
                _bucket.Add(new KeyBlock(new Uncommand(), new OpenConsole(game), false, Keys.T));
                _bucket.Add(new KeyBlock(new Uncommand(), new OpenConsoleForCommand(game), false, Keys.OemQuestion));
            }
        }

        public void Initialize(ConsoleHandler console)
        {
            if (_bucket.Count > 0) return;

            _bucket.Add(new KeyBlock(new PushConsole(console), false, Keys.Enter));
            _bucket.Add(new KeyBlock(new RememberLastMessage(console), false, Keys.Up));
            _bucket.Add(new KeyBlock(new ForgetCurrentMessage(console), false, Keys.Down));
            
        }

        public void Initialize(Sector game, bool menuType)
        {
            if (_bucket.Count > 0) return;

            if (menuType)
            {
                _bucket.Add(new KeyBlock(new MenuUp(game), true, Keys.Up, Keys.W, Buttons.DPadUp, Buttons.LeftThumbstickUp));
                _bucket.Add(new KeyBlock(new MenuDown(game), true, Keys.Down, Keys.S, Buttons.DPadDown, Buttons.LeftThumbstickDown));
                _bucket.Add(new KeyBlock(new MenuLeft(game), true, Keys.Left, Keys.A, Buttons.DPadLeft, Buttons.LeftThumbstickLeft));
                _bucket.Add(new KeyBlock(new MenuRight(game), true, Keys.Right, Keys.D, Buttons.DPadRight, Buttons.LeftThumbstickRight));
                _bucket.Add(new KeyBlock(new MenuSelect(game), false, Keys.Enter, Buttons.A));
                _bucket.Add(new KeyBlock(new MenuBack(game), false, Keys.Q, Buttons.Start));
            }
            else
            {
                _bucket.Add(new KeyBlock(new InteractWithWorld(game), false, Keys.E, Buttons.A));
                _bucket.Add(new KeyBlock(new TurnUp(game), new UnTurnUp(game), false, Keys.Up, Keys.W, Buttons.DPadUp, Buttons.LeftThumbstickUp));
                _bucket.Add(new KeyBlock(new TurnDown(game), new UnTurnDown(game), false, Keys.Down, Keys.S, Buttons.DPadDown, Buttons.LeftThumbstickDown));
                _bucket.Add(new KeyBlock(new TurnLeft(game), new UnTurnLeft(game), false, Keys.Left, Keys.A, Buttons.DPadLeft, Buttons.LeftThumbstickLeft));
                _bucket.Add(new KeyBlock(new TurnRight(game), new UnTurnRight(game), false, Keys.Right, Keys.D, Buttons.DPadRight, Buttons.LeftThumbstickRight));
                _bucket.Add(new KeyBlock(new DoubleSpeed(game), new UnDoubleSpeed(game), false, Keys.LeftShift, Buttons.X, Buttons.Y));
            }
        }

        public void UpdateInput()
        {
            foreach (var controller in _controllers)
            {
                controller.UpdateInput();
            }
            foreach (var block in _bucket)
            {
                if (block.IsActive)
                {
                    block.Fire(UniversalTime.GetLastTickTime(_timerID));
                }
            }
        }

        public void ClearInput()
        {
            foreach (var block in _bucket)
            {
                if (block.IsActive)
                {
                    block.Deactivate();
                }
            }
        }

        public void HandleCommand(object something, bool hook)
        {
            foreach (var block in _bucket)
            {
                if (block.Contains(something))
                {
                    if (hook) //button has been pressed
                    {
                        block.Activate();
                    }
                    else  //button has been released
                    {
                        block.Deactivate();
                    }
                }
            }
        }
    }
}
