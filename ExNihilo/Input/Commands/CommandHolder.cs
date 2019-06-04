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

        private const double _inputDelayCounter = 0.05;
        private const double _repeatDelayCounter = 0.35;
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
            _timeSinceLastFire = _inputDelayCounter;
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
            if (_repeating && _timeSinceLastFire >= _inputDelayCounter)
            {
                _timeSinceLastFire -= _inputDelayCounter;
                _command.Activate();
            }
            else if (_timeSinceLastFire >= _repeatDelayCounter)
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
            _timeSinceLastFire = _inputDelayCounter;
            IsActive = _repeating = false;
            _uncommand.Activate();
        }

        public bool Contains(object key)
        {
            return _keys.Contains(key);
        }
    }
}
