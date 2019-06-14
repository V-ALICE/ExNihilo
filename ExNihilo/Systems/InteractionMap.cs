using ExNihilo.Systems.Bases;

namespace ExNihilo.Systems
{
    public class InteractionMap
    {
        public enum Type
        {
            Air, Ground, Wall
        }

        private Type[][] _map;
        private readonly int _x, _y;
        private IInteractive[][] _interactive;

        public InteractionMap(int x, int y)
        {
            _x = x;
            _y = y;
            _map = new Type[y][];
            _interactive = new IInteractive[y][];
            for (int i=0; i < y; i++)
            {
                _map[i] = new Type[x];
                _interactive[i] = new IInteractive[x];
                for (int j = 0; j < x; j++)
                {
                    _map[i][j] = Type.Air;
                    _interactive[i][j] = null;
                }
            }
        }

        public void AddInteractive(IInteractive obj, int x, int y, int width, int height)
        {
            //starts from top left

        }
    }
}
