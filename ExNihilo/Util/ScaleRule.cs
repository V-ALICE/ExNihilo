using System.Collections.Generic;

namespace ExNihilo.Util
{
    public class ScaleRule
    {
        public const int MAX_Y = 2160, MAX_X = 3840;
        public const int MIN_Y = 500, MIN_X = 700;
        public readonly int XMax, YMax;
        private readonly float _scale;

        public ScaleRule(float scale, int xMax, int yMax)
        {
            XMax = xMax;
            YMax = yMax;
            _scale = scale;
        }

        public bool CheckRule(Coordinate window)
        {
            return window.X < XMax || window.Y < YMax;
        }

        public float GetScale()
        {
            return _scale;
        }
    }

    public class ScaleRuleSet
    {
        private readonly List<ScaleRule> _rules = new List<ScaleRule>();

        public ScaleRuleSet(params ScaleRule[] rules)
        {
            _rules.AddRange(rules);
        }

        public void AddRules(params ScaleRule[] rules)
        {
            _rules.AddRange(rules);
        }

        public float GetScale(Coordinate window)
        {
            foreach (var rule in _rules)
            {
                if (rule.CheckRule(window)) return rule.GetScale();
            }

            return 1; //outside all boxes
        }
    }

}
