using ExNihilo.Util;

namespace ExNihilo.Input.Commands.Types
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
            Receiver.OpenConsole();
        }
    }

    public class OpenConsoleForCommand : SuperCommand
    {
        public OpenConsoleForCommand(GameContainer game) : base(game)
        {
        }

        public override void Activate()
        {
            Receiver.OpenConsole("/");
        }
    }

    public class BackOutCommand : SuperCommand
    {
        public BackOutCommand(GameContainer game) : base(game)
        {

        }

        public override void Activate()
        {
            Receiver.BackOut();
        }
    }

    public class BackspaceMessage : SuperCommand
    {
        public BackspaceMessage(GameContainer game) : base(game)
        {
        }

        public override void Activate()
        {
            TypingKeyboard.Backspace();
        }
    }

    public class UnbackspaceMessage : SuperCommand
    {
        public UnbackspaceMessage(GameContainer game) : base(game)
        {
        }

        public override void Activate()
        {
            TypingKeyboard.Unbackspace();
        }
    }
}
