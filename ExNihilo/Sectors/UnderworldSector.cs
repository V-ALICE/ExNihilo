using ExNihilo.Input.Commands;

namespace ExNihilo.Sectors
{
    public class UnderworldSector : Sector
    {       
        private CommandHandler _menuHandler;

        public UnderworldSector()
        {
        }

        /********************************************************************
        ------->Game loop
        ********************************************************************/
        public override void Initialize()
        {
            handler = new CommandHandler();
            handler.Initialize(this);
            _menuHandler = new CommandHandler();
            _menuHandler.Initialize(this);
        }

        public override void LoadContent()
        {
        }

        public override void Update()
        {
            handler.UpdateInput();
            //_menuHandler.UpdateInput();
        }

        protected override void DrawDebugInfo()
        {
        }

        public override void Draw(bool drawDebugInfo)
        {  
            if (drawDebugInfo) DrawDebugInfo();
        }

        /********************************************************************
        ------->Game functions
        ********************************************************************/
        public override void ExitGame()
        {
        }
    }
}
