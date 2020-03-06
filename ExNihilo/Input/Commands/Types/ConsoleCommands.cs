using ExNihilo.Systems;
using ExNihilo.Systems.Backend;
using ExNihilo.Systems.Backend.Network;

namespace ExNihilo.Input.Commands.Types
{
    public class PushConsole : ConsoleCommand
    {
        private GameContainer _g;
        public PushConsole(GameContainer g, ConsoleHandler game) : base(game)
        {
            _g = g;
        }

        public override void Activate()
        {
            var name = "Console";
            if (_g.Player != null) name = _g.Player.Name + (NetworkLinker.MyMiniID > 0 ? "-" + NetworkLinker.MyMiniID : "");
            Receiver.PushConsole(name, GameContainer.ActiveSectorID == GameContainer.SectorID.Loading);
        }
    }

    public class RememberLastMessage : ConsoleCommand
    {
        public RememberLastMessage(ConsoleHandler game) : base(game)
        {
        }

        public override void Activate()
        {
            Receiver.GetLastMessage();
        }
    }

    public class ForgetCurrentMessage : ConsoleCommand
    {
        public ForgetCurrentMessage(ConsoleHandler game) : base(game)
        {
        }

        public override void Activate()
        {
            Receiver.ClearOutMessage();
        }
    }

}
