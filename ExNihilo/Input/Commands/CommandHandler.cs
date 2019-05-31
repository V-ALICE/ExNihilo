using System.Collections.Generic;
using ExNihilo.Input.Controllers;
using ExNihilo.Sectors;
using ExNihilo.Util;
using Microsoft.Xna.Framework.Input;

namespace ExNihilo.Input.Commands
{
    public class CommandHandler
    {
        private readonly List<IController> _controllers;
        private readonly List<KeyBlock> _bucket;
        private readonly int _timerID;

        public CommandHandler(GameContainer game)
        {
            _timerID = UniversalTime.NewTimer(true);
            UniversalTime.TurnOnTimer(_timerID);
            _controllers = new List<IController> { new KeyboardControl(this), new ControllerControl(this) };
            _bucket = new List<KeyBlock>
            {
                new KeyBlock(new ToggleTitleMenu(game), false, Keys.Escape, Buttons.Back),
                new KeyBlock(new ToggleDebugUI(game), false, Keys.F1),
                new KeyBlock(new ToggleFullScreen(game), false, Keys.F2),
                new KeyBlock(new OpenConsole(game), false, Keys.T, Keys.OemQuestion)
            };
        }
        public CommandHandler(Sector sector)
        {
            _timerID = UniversalTime.NewTimer(true);
            UniversalTime.TurnOnTimer(_timerID);
            _controllers = new List<IController> { new KeyboardControl(this), new ControllerControl(this) };
            _bucket = new List<KeyBlock>();
            Initialize(sector);
        }

        private void Initialize(Sector game)
        {
            if (_bucket.Count > 0) return;

            if (game is UnderworldSector sector)
            {
                _bucket.Add(new KeyBlock(new InteractWithWorld(sector), false, Keys.E, Keys.Enter, Buttons.A));
                /*
                _bucket.Add(new KeyBlock(new TurnLeft(game.OwnPlayer), new UnTurnLeft(game.OwnPlayer), false, Keys.Left, Keys.A, Buttons.DPadLeft, Buttons.LeftThumbstickLeft));
                _bucket.Add(new KeyBlock(new TurnUp(game.OwnPlayer), new UnTurnUp(game.OwnPlayer), false, Keys.Up, Keys.W, Buttons.DPadUp, Buttons.LeftThumbstickUp));
                _bucket.Add(new KeyBlock(new TurnDown(game.OwnPlayer), new UnTurnDown(game.OwnPlayer), false, Keys.Down, Keys.S, Buttons.DPadDown, Buttons.LeftThumbstickDown));
                _bucket.Add(new KeyBlock(new TurnRight(game.OwnPlayer), new UnTurnRight(game.OwnPlayer), false, Keys.Right, Keys.D, Buttons.DPadRight, Buttons.LeftThumbstickRight));
                _bucket.Add(new KeyBlock(new DoubleSpeed(game.OwnPlayer), new UnDoubleSpeed(game.OwnPlayer), false, Keys.LeftShift, Buttons.X, Buttons.Y));
                */
            }
            else
            {
                _bucket.Add(new KeyBlock(new MenuUp(game), true, Keys.Up, Keys.W, Buttons.DPadUp, Buttons.LeftThumbstickUp));
                _bucket.Add(new KeyBlock(new MenuDown(game), true, Keys.Down, Keys.S, Buttons.DPadDown, Buttons.LeftThumbstickDown));
                _bucket.Add(new KeyBlock(new MenuLeft(game), true, Keys.Left, Keys.A, Buttons.DPadLeft, Buttons.LeftThumbstickLeft));
                _bucket.Add(new KeyBlock(new MenuRight(game), true, Keys.Right, Keys.D, Buttons.DPadRight, Buttons.LeftThumbstickRight));
                _bucket.Add(new KeyBlock(new MenuSelect(game), false, Keys.Enter, Buttons.A));
                _bucket.Add(new KeyBlock(new ToggleMenu(game), false, Keys.Tab, Buttons.Start));
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
