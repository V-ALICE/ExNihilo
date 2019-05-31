namespace ExNihilo.Input.Commands
{

    public class ToggleDebugUI : SuperCommand
    {
        public ToggleDebugUI(GameContainer game) : base(game)
        {
        }

        public override void Activate()
        {
            Receiver.ToggleShowDebugInfo();
        }
    }
    
    public class ToggleTitleMenu : SuperCommand
    {
        public ToggleTitleMenu(GameContainer game) : base(game)
        {
        }

        public override void Activate()
        {
            //Receiver.ResetTitle();
        }
    }

    public class ToggleFullScreen : SuperCommand
    {
        public ToggleFullScreen(GameContainer game) : base(game)
        {
        }

        public override void Activate()
        {
            Receiver.ToggleFullScreen();
        }
    }
    
    public class OpenConsole : SuperCommand
    {
        public OpenConsole(GameContainer game) : base(game)
        {
        }

        public override void Activate()
        {
            //Receiver.ToggleChat();
        }
    }

}
