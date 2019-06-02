using System.Collections.Generic;
using ExNihilo.Input.Commands.Types;

namespace ExNihilo.Input.Commands
{
    public class KeyBlock
    {
        private readonly List<object> _keys;
        private readonly ICommand _command;
        private readonly ICommand _uncommand;
        private double _timeSinceLastFire;
        private readonly bool _nonRepeatable;
        private bool _repeating, _firstStrike;

        public const double InputDelayCounter = 0.05;
        public const double RepeatDelayCounter = 0.35;
        public bool IsActive { get; private set; }

        public KeyBlock(ICommand cmd, bool repeatable, params object[] keys)//one time per press
        {
            _uncommand = new Uncommand();
            _keys = new List<object>(keys);
            _command = cmd;
            IsActive = false;
            _nonRepeatable = !repeatable;
        }
        public KeyBlock(ICommand cmd, ICommand uncmd, bool repeatable, params object[] keys)
        {
            _keys = new List<object>(keys);
            _command = cmd;
            _uncommand = uncmd;
            IsActive = false;
            _nonRepeatable = !repeatable;
            _timeSinceLastFire = InputDelayCounter;
        }

        public void Fire(double seconds)
        {
            //only for repeating commands
            if (_nonRepeatable) return;
            _timeSinceLastFire += seconds;
            if (_firstStrike)
            {
                _command.Activate();
                _firstStrike = false;
            }
            if (_repeating && _timeSinceLastFire >= InputDelayCounter)
            {
                _timeSinceLastFire -= InputDelayCounter;
                _command.Activate();
            }
            else if (_timeSinceLastFire >= RepeatDelayCounter)
            {
                _timeSinceLastFire = 0;
                _repeating = true;
            }
        }

        public void Activate()
        {
            if (IsActive) return;
            IsActive = _firstStrike = true;
            if (_nonRepeatable) _command.Activate();               
        }

        public void Deactivate()
        {
            if (!IsActive) return;
            _timeSinceLastFire = InputDelayCounter;
            IsActive = _repeating = false;
            _uncommand.Activate();
        }

        public bool Contains(object key)
        {
            return _keys.Contains(key);
        }
    }
}
