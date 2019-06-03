using Microsoft.Xna.Framework;

namespace ExNihilo.UI
{
    public interface IClickable
    {
        void OnMoveMouse(Point point);
        void OnLeftClick(Point point);
        void OnLeftRelease();
    }
}
