using ExNihilo.Systems;
using ExNihilo.Systems.Backend;

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
            if (_g.Player != null) name = _g.Player.Name + (NetworkLinker._myMiniID > 0 ? "-" + NetworkLinker._myMiniID : "");
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
