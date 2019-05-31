using System.Threading;
using ExNihilo.Input.Commands;

namespace ExNihilo.Sectors
{
    public abstract class Sector
    {
        protected CommandHandler handler;
        protected Thread loadingThread;

        public abstract void Initialize();
        public abstract void LoadContent();
        public abstract void Update();
        protected abstract void DrawDebugInfo();
        public abstract void Draw(bool drawDebugInfo);
        public abstract void ExitGame();
    }
}
