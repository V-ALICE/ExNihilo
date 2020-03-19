using Microsoft.Xna.Framework;

namespace ExNihilo.UI.Bases
{
    public interface IClickable
    {
        bool OnMoveMouse(Point point);
        bool OnLeftClick(Point point);
        void OnLeftRelease(Point point);
    }
}
