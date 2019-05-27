namespace ExNihilo.Input.Commands
{

    public class ForceDebug : GameCommand
    {
        public ForceDebug(GameContainer game) : base(game)
        {           
        }

        public override void Activate()
        {
            //Receiver.ForceSomethingToHappenDebug();
        }
    }

    public class ToggleDebugUI : GameCommand
    {
        public ToggleDebugUI(GameContainer game) : base(game)
        {
        }

        public override void Activate()
        {
            //Receiver.ShowDebug = !Receiver.ShowDebug;
        }
    }

    public class ExportMap : GameCommand
    {
        public ExportMap(GameContainer game) : base(game)
        {
        }

        public override void Activate()
        {
            //Receiver.ExportCurrentMap();
        }
    }

    public class MainMenu : GameCommand
    {
        public MainMenu(GameContainer game) : base(game)
        {
        }

        public override void Activate()
        {
            //Receiver.ResetTitle();
        }
    }

    public class FullScreen : GameCommand
    {
        public FullScreen(GameContainer game) : base(game)
        {
        }

        public override void Activate()
        {
            //Receiver.FullScreen();
        }
    }

    public class PauseGame : GameCommand
    {
        public PauseGame(GameContainer game) : base(game)
        {
        }

        public override void Activate()
        {
            //Receiver.Pause();
        }
    }

    public class ToggleChat : GameCommand
    {
        public ToggleChat(GameContainer game) : base(game)
        {
        }

        public override void Activate()
        {
            //Receiver.ToggleChat();
        }
    }

    public class Interact : GameCommand
    {
        public Interact(GameContainer game) : base(game)
        {
        }

        public override void Activate()
        {
            //Receiver.Interact();
        }
    }

    public class MenuUp : GameCommand
    {
        public MenuUp(GameContainer menu) : base(menu)
        {         
        }

        public override void Activate()
        {
            //Receiver.PushMenu(G.Top);
        }
    }

    public class MenuDown : GameCommand
    {
        public MenuDown(GameContainer menu) : base(menu)
        {
        }

        public override void Activate()
        {
            //Receiver.PushMenu(G.Bottom);
        }
    }

    public class MenuLeft : GameCommand
    {
        public MenuLeft(GameContainer menu) : base(menu)
        {
        }

        public override void Activate()
        {
            //Receiver.PushMenu(G.Left);
        }
    }

    public class MenuRight : GameCommand
    {
        public MenuRight(GameContainer menu) : base(menu)
        {
        }

        public override void Activate()
        {
            //Receiver.PushMenu(G.Right);
        }
    }

    public class Select : GameCommand
    {
        public Select(GameContainer game) : base(game)
        {
        }

        public override void Activate()
        {
            //Receiver.PushMenu(G.None);
        }
    }

    public class SwapMenu : GameCommand
    {
        public SwapMenu(GameContainer game) : base(game)
        {
        }

        public override void Activate()
        {
            //Receiver.PushMenu(G.Swap);
        }
    }

}
