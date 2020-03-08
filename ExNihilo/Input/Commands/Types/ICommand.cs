using ExNihilo.Sectors;
using ExNihilo.Systems.Backend;
using ExNihilo.Systems.Bases;

namespace ExNihilo.Input.Commands.Types
{
    public interface ICommand
    {
        void Activate();
    }

    public abstract class SuperCommand : ICommand
    {
        protected readonly GameContainer Receiver;
        protected SuperCommand(GameContainer receiver)
        {
            Receiver = receiver;
        }
        public abstract void Activate();
    }

    public abstract class MenuCommand : ICommand
    {
        protected readonly Sector Receiver;
        protected MenuCommand(Sector receiver)
        {
            Receiver = receiver;
        }
        public abstract void Activate();
    }

    public abstract class GameplayCommand : ICommand
    {
        protected readonly IPlayer Receiver;
        protected GameplayCommand(IPlayer receiver)
        {
            Receiver = receiver;
        }
        public abstract void Activate();
    }

    public abstract class SuperGameplayCommand : ICommand
    {
        protected readonly ISuperPlayer Receiver;
        protected SuperGameplayCommand(ISuperPlayer receiver)
        {
            Receiver = receiver;
        }
        public abstract void Activate();
    }

    public class Uncommand : ICommand
    {
        public void Activate()
        {
        }
    }
}
