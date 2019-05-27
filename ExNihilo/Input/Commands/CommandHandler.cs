using System.Collections.Generic;
using System.Diagnostics;
using ExNihilo.Input.Controllers;
using ExNihilo.Util;
using Microsoft.Xna.Framework.Input;

namespace ExNihilo.Input.Commands
{
    public class CommandHandler
    {
        private readonly List<IController> _controllers;
        public enum Mode
        {
            None, Gameplay, Menu, TitleMenu
        }
        private readonly Dictionary<Mode, List<KeyBlock>> _bucket = new Dictionary<Mode, List<KeyBlock>>
        {
            {Mode.Gameplay, new List<KeyBlock>()},
            {Mode.Menu, new List<KeyBlock>()},
            {Mode.None, new List<KeyBlock>()},
            {Mode.TitleMenu, new List<KeyBlock>()}
        };
        private Mode _currentMode;
        private readonly int timerID;

        public CommandHandler()
        {
            timerID = UniversalTime.RequestTimer(true);
            UniversalTime.TurnOnTimer(timerID);
            _controllers = new List<IController> { new KeyboardControl(this), new ControllerControl(this) };
            _currentMode = Mode.TitleMenu;
        }

        public void Initialize(GameContainer game)
        {
            var block = new KeyBlock(new PauseGame(game), false, Keys.Tab, Buttons.Start);
            _bucket[Mode.Gameplay].Add(block);
            _bucket[Mode.Menu].Add(block);
            _bucket[Mode.TitleMenu].Add(block);

            block = new KeyBlock(new MainMenu(game), false, Keys.Escape, Buttons.Back);
            _bucket[Mode.Gameplay].Add(block);
            _bucket[Mode.TitleMenu].Add(block);

            block = new KeyBlock(new ForceDebug(game), false, Keys.D0);
            _bucket[Mode.Gameplay].Add(block);
            _bucket[Mode.Menu].Add(block);

            block = new KeyBlock(new ToggleDebugUI(game), false, Keys.F1);
            _bucket[Mode.Gameplay].Add(block);
            _bucket[Mode.Menu].Add(block);
            _bucket[Mode.TitleMenu].Add(block);

            block = new KeyBlock(new ExportMap(game), false, Keys.F2);
            _bucket[Mode.Gameplay].Add(block);

            block = new KeyBlock(new FullScreen(game), false, Keys.F3);
            _bucket[Mode.Gameplay].Add(block);
            _bucket[Mode.Menu].Add(block);
            _bucket[Mode.TitleMenu].Add(block);

            block = new KeyBlock(new MenuUp(game), true, Keys.Up, Keys.W, Buttons.DPadUp, Buttons.LeftThumbstickUp);
            _bucket[Mode.Menu].Add(block);
            _bucket[Mode.TitleMenu].Add(block);

            block = new KeyBlock(new MenuDown(game), true, Keys.Down, Keys.S, Buttons.DPadDown, Buttons.LeftThumbstickDown);
            _bucket[Mode.Menu].Add(block);
            _bucket[Mode.TitleMenu].Add(block);

            block = new KeyBlock(new MenuLeft(game), true, Keys.Left, Keys.A, Buttons.DPadLeft, Buttons.LeftThumbstickLeft);
            _bucket[Mode.Menu].Add(block);
            _bucket[Mode.TitleMenu].Add(block);

            block = new KeyBlock(new MenuRight(game), true, Keys.Right, Keys.D, Buttons.DPadRight, Buttons.LeftThumbstickRight);
            _bucket[Mode.Menu].Add(block);
            _bucket[Mode.TitleMenu].Add(block);

            block = new KeyBlock(new Select(game), false, Keys.Enter, Buttons.A);
            _bucket[Mode.Menu].Add(block);
            _bucket[Mode.TitleMenu].Add(block);

            block = new KeyBlock(new SwapMenu(game), false, Keys.LeftShift, Keys.RightShift, Buttons.RightShoulder, Buttons.LeftShoulder);
            _bucket[Mode.Menu].Add(block);

            block = new KeyBlock(new Interact(game), false, Keys.E, Keys.Enter, Buttons.A);
            _bucket[Mode.Gameplay].Add(block);

            block = new KeyBlock(new ToggleChat(game), false, Keys.T, Keys.OemQuestion);
            _bucket[Mode.Gameplay].Add(block);
            _bucket[Mode.Menu].Add(block);
            _bucket[Mode.TitleMenu].Add(block);

            /*block = new KeyBlock(new TurnLeft(game.OwnPlayer), new UnTurnLeft(game.OwnPlayer), false, 
                Keys.Left, Keys.A, Buttons.DPadLeft, Buttons.LeftThumbstickLeft);
            _bucket[Mode.Gameplay].Add(block);

            block = new KeyBlock(new TurnUp(game.OwnPlayer), new UnTurnUp(game.OwnPlayer), false, 
                Keys.Up, Keys.W, Buttons.DPadUp, Buttons.LeftThumbstickUp);
            _bucket[Mode.Gameplay].Add(block);

            block = new KeyBlock(new TurnDown(game.OwnPlayer), new UnTurnDown(game.OwnPlayer), false, 
                Keys.Down, Keys.S, Buttons.DPadDown, Buttons.LeftThumbstickDown);
            _bucket[Mode.Gameplay].Add(block);

            block = new KeyBlock(new TurnRight(game.OwnPlayer), new UnTurnRight(game.OwnPlayer), false, 
                Keys.Right, Keys.D, Buttons.DPadRight, Buttons.LeftThumbstickRight);
            _bucket[Mode.Gameplay].Add(block);

            block = new KeyBlock(new DoubleSpeed(game.OwnPlayer), new UnDoubleSpeed(game.OwnPlayer), false,
                Keys.LeftShift, Buttons.X, Buttons.Y);
            _bucket[Mode.Gameplay].Add(block);*/
        }

        public void UpdateInput()
        {
            foreach (var controller in _controllers)
            {
                controller.UpdateInput();
            }
            foreach (var block in _bucket[_currentMode])
            {
                if (block.IsActive)
                {
                    block.Fire(UniversalTime.RequestLastTickTime(timerID));
                }
            }
        }

        public void ClearInput()
        {
            foreach (var block in _bucket[_currentMode])
            {
                if (block.IsActive)
                {
                    block.Deactivate();
                }
            }
        }
        public void ChangeMode(Mode mode)
        {
            if (_bucket.ContainsKey(mode))
            {             
                ClearInput();
                _currentMode = mode;
            }
        }

        public void HandleCommand(object something, bool hook)
        {
            foreach (var block in _bucket[_currentMode])
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
