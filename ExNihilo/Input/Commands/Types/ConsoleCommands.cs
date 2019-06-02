using ExNihilo.Systems;

namespace ExNihilo.Input.Commands.Types
{
    public class BackOutConsole : ConsoleCommand
    {
        public BackOutConsole(ConsoleHandler game) : base(game)
        {
        }

        public override void Activate()
        {
            Receiver.CloseConsole();
        }
    }

    public class PushConsole : ConsoleCommand
    {
        public PushConsole(ConsoleHandler game) : base(game)
        {
        }

        public override void Activate()
        {
            Receiver.PushConsole();
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

    public class BackspaceMessage : ConsoleCommand
    {
        public BackspaceMessage(ConsoleHandler game) : base(game)
        {
        }

        public override void Activate()
        {
            Receiver.Backspace();
        }
    }

    public class UnBackspaceMessage : ConsoleCommand
    {
        public UnBackspaceMessage(ConsoleHandler game) : base(game)
        {
        }

        public override void Activate()
        {
            Receiver.UnBackspace();
        }
    }
}
