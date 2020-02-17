using ExNihilo.Systems;
using ExNihilo.Systems.Backend;

namespace ExNihilo.Input.Commands.Types
{
    public class PushConsole : ConsoleCommand
    {
        public PushConsole(ConsoleHandler game) : base(game)
        {
        }

        public override void Activate()
        {
            Receiver.PushConsole(GameContainer.ActiveSectorID == GameContainer.SectorID.Loading);
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
