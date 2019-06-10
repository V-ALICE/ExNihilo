using ExNihilo.Util;

namespace ExNihilo.UI.Bases
{
    public interface IScalable
    {
        void SetRules(ScaleRuleSet rules);
        void ReinterpretScale(Coordinate window);
    }
}
