using ExNihilo.Sectors;

namespace ExNihilo.Input.Commands.Types
{

    public class ToggleMenu : MenuCommand
    {
        public ToggleMenu(Sector game) : base(game)
        {
        }

        public override void Activate()
        {
            //Receiver.Pause();
        }
    }
    
    public class MenuUp : MenuCommand
    {
        public MenuUp(Sector menu) : base(menu)
        {         
        }

        public override void Activate()
        {
            //Receiver.PushMenu(G.Top);
        }
    }

    public class MenuDown : MenuCommand
    {
        public MenuDown(Sector menu) : base(menu)
        {
        }

        public override void Activate()
        {
            //Receiver.PushMenu(G.Bottom);
        }
    }

    public class MenuLeft : MenuCommand
    {
        public MenuLeft(Sector menu) : base(menu)
        {
        }

        public override void Activate()
        {
            //Receiver.PushMenu(G.Left);
        }
    }

    public class MenuRight : MenuCommand
    {
        public MenuRight(Sector menu) : base(menu)
        {
        }

        public override void Activate()
        {
            //Receiver.PushMenu(G.Right);
        }
    }

    public class MenuSelect : MenuCommand
    {
        public MenuSelect(Sector game) : base(game)
        {
        }

        public override void Activate()
        {
            //Receiver.PushMenu(G.None);
        }
    }

}
