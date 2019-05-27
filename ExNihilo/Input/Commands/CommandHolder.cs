using System.Collections.Generic;

namespace ExNihilo.Input.Commands
{
    public class KeyBlock
    {
        private readonly List<object> _keys;
        public bool IsActive { get; private set; }
        private readonly ICommand _command;
        private readonly ICommand _uncommand;
        private double _timeSinceLastFire;
        private readonly bool _nonRepeatable;
        private bool repeating, firstStrike;
        public const double inputDelayCounter = 0.05;
        public const double repeatDelayCounter = 0.35;

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
            _timeSinceLastFire = inputDelayCounter;
        }

        public void Fire(double seconds)
        {
            //only for repeating commands
            if (_nonRepeatable) return;
            _timeSinceLastFire += seconds;
            if (firstStrike)
            {
                _command.Activate();
                firstStrike = false;
            }
            if (repeating && _timeSinceLastFire >= inputDelayCounter)
            {
                _timeSinceLastFire -= inputDelayCounter;
                _command.Activate();
            }
            else if (_timeSinceLastFire >= repeatDelayCounter)
            {
                _timeSinceLastFire = 0;
                repeating = true;
            }
        }

        public void Activate()
        {
            if (IsActive) return;
            IsActive = firstStrike = true;
            if (_nonRepeatable) _command.Activate();               
        }

        public void Deactivate()
        {
            if (!IsActive) return;
            _timeSinceLastFire = inputDelayCounter;
            IsActive = repeating = false;
            _uncommand.Activate();
        }

        public bool Contains(object key)
        {
            return _keys.Contains(key);
        }
    }
}
