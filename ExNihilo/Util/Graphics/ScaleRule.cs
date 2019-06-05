using System.Collections.Generic;

namespace ExNihilo.Util.Graphics
{
    public class ScaleRule
    {
        public const int MAX_Y = 2560, MAX_X = 3840;
        public const int MIN_Y = 500, MIN_X = 700;
        public readonly int XMax, YMax;
        private readonly float _scale;

        public ScaleRule(float scale, int xMax, int yMax)
        {
            XMax = xMax;
            YMax = yMax;
            _scale = scale;
        }

        public bool CheckRule(ScaleRule previousRule, Coordinate window)
        {
            return window.X >= previousRule.XMax && window.Y >= previousRule.YMax && window.X < XMax && window.Y < YMax;
        }
        public bool CheckRule(Coordinate window)
        {
            return window.X < XMax && window.Y < YMax;
        }

        public float GetScale()
        {
            return _scale;
        }
    }

    public class ScaleRuleSet
    {
        private readonly List<ScaleRule> _rules = new List<ScaleRule>();

        public void AddRules(params ScaleRule[] rules)
        {
            _rules.AddRange(rules);
        }

        public float GetScale(Coordinate window)
        {
            if (_rules.Count == 0) return 1; //No rules

            if (_rules[0].CheckRule(window)) return _rules[0].GetScale(); //inside all boxes

            for (int i = 1; i < _rules.Count; i++)
            {
                if (_rules[i].CheckRule(_rules[i - 1], window))
                {
                    return _rules[i].GetScale();
                }
            }

            return 1; //outside all boxes
        }
    }

}
