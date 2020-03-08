using ExNihilo.Systems;
using ExNihilo.Systems.Backend;
using ExNihilo.Systems.Backend.Network;

namespace ExNihilo.Input.Commands.Types
{
    public class PushConsole : SuperCommand
    {
        public PushConsole(GameContainer game) : base(game)
        {
        }

        public override void Activate()
        {
            var name = "Console";
            if (Receiver.Player != null) name = Receiver.Player.Name + (NetworkLinker.MyMiniID > 0 ? "-" + NetworkLinker.MyMiniID : "");
            SystemConsole.PushConsole(name, GameContainer.ActiveSectorID == GameContainer.SectorID.Loading);
        }
    }

    public class RememberLastMessage : SuperCommand
    {
        public RememberLastMessage(GameContainer game) : base(game)
        {
        }

        public override void Activate()
        {
            SystemConsole.GetLastMessage();
        }
    }

    public class ForgetCurrentMessage : SuperCommand
    {
        public ForgetCurrentMessage(GameContainer game) : base(game)
        {
        }

        public override void Activate()
        {
            SystemConsole.ClearOutMessage();
        }
    }

}
