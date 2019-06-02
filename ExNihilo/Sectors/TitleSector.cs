using ExNihilo.Input.Commands;

namespace ExNihilo.Sectors
{
    public class TitleSector : Sector
    {
        public TitleSector()
        {
        }

        /********************************************************************
        ------->Game loop
        ********************************************************************/
        public override void Initialize()
        {
            handler = new CommandHandler();
            handler.Initialize(this);
        }

        public override void LoadContent()
        {
        }

        public override void Update()
        {
            handler.UpdateInput();
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
