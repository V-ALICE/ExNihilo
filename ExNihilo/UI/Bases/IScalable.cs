using ExNihilo.Util;
using ExNihilo.Util.Graphics;

namespace ExNihilo.UI.Bases
{
    public interface IScalable
    {
        void AddRules(params ScaleRule[] rules);
        void ReinterpretScale(Coordinate window);
    }
}
